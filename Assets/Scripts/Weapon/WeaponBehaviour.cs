using System.Collections.Generic;
using Enemy;
using Game;
using OculusSampleFramework;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using Player;

public class WeaponBehaviour : MonoBehaviour
{
    [Space] [Header("Prefabs")] public GameObject bulletPrefab;
    public GameObject shootLinePrefab;
    public GameObject casingPrefab;
    public GameObject muzzleFlashPrefab;

    [Space] [Header("References")] public Transform barrelLocation;
    public Transform casingExitLocation;
    public TextMeshPro ammoDisplay;
    public Renderer[] partsToColor;

    [Space] [Header("Sound Effects")] public AudioClip shootingSound;
    public AudioClip noAmmoShootingSound;
    public AudioClip reloadSound;

    [Space] [Header("Settings")] public OVRInput.Button shootingButton;
    public LayerMask shootLayerFilter;
    public LayerMask hitIndicatorLayerFilter;
    public int maxAmmo = 10;
    public float bulletCd = 0.1f;
    public bool isAutomatic;
    public bool isTwoHanded;

    //References
    private Transform _enemyUser; //ref to enemy (if the enemy is using the gun)
    private OVRGrabbable _ovrGrabbable; //oculus grab script
    private Transform _secondaryControllerTransform; //hold a reference to the second controller (only needed if two-handed)
    private AudioSource _audioSource;
    private Rigidbody _rb;
    private Animator _animator;
    private LineRenderer _uiBeam;
    private VrUiElement _hitUiScript;
    private Camera _mainCam;

    //Temp variables
    private bool _isBlue;
    private int _currentAmmo;
    private bool _isReloading;
    private float _currentBulletCd;
    private bool _isActivated = true;
    private RaycastHit[] _hits;
    private bool _hasLastShotHitEnemy;
    private bool _hasHitUi;
    
    private static readonly int ColorProp = Shader.PropertyToID("_Color");
    private static readonly int HasAmmo = Animator.StringToHash("hasAmmo");
    private static readonly int Fire = Animator.StringToHash("Fire");
    private static readonly int IsFiring = Animator.StringToHash("isFiring");

    private void Awake()
    {
        _ovrGrabbable = GetComponent<OVRGrabbable>();
        _audioSource = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _uiBeam = GetComponent<LineRenderer>();

        if (partsToColor.Length == 0 && GetComponent<Renderer>())
        {
            partsToColor = new[] {GetComponent<Renderer>()};
        }
    }

    private void Start()
    {
        _currentAmmo = maxAmmo;
        _currentBulletCd = 0;
        _hits = new RaycastHit[2];
        _audioSource.volume = GameSettings.Instance.sfxVolume;
        _mainCam = Camera.main;
    }

    private void Update()
    {
        if (_isActivated)
        {
            //Check if the gun is grabbed by the PLAYER
            if (_ovrGrabbable.isGrabbed)
            {
                if (!GameHandler.Instance.inGame)
                {
                    if (!_uiBeam.enabled) _uiBeam.enabled = true;
                    SetUiBeamPositions();
                } 
                else if (_uiBeam.enabled)
                {
                    _uiBeam.enabled = false;
                }
                
                _currentBulletCd -= Time.deltaTime;

                if (isAutomatic)
                {
                    //Automatic (Assault Rifle) --> keep shooting while button is held down
                    if (OVRInput.Get(shootingButton, _ovrGrabbable.grabbedBy.GetController()) && _currentBulletCd <= 0)
                    {
                        //Shoot
                        TriggerShoot();
                        _currentBulletCd = bulletCd;
                        if(_animator) _animator.SetBool(IsFiring, true);
                    }
                    else if(OVRInput.GetUp(shootingButton, _ovrGrabbable.grabbedBy.GetController()))
                    {
                        if(_animator) _animator.SetBool(IsFiring, false);
                    }
                }
                else
                {
                    //Handgun, Shotgun --> only shoots once and resets when button is released
                    if (OVRInput.GetDown(shootingButton, _ovrGrabbable.grabbedBy.GetController()) &&
                        _currentBulletCd <= 0)
                    {
                        //Shoot
                        TriggerShoot();
                        _currentBulletCd = bulletCd;
                    }
                }

                //point weapon straight up or down to reload
                float reloadAngle = Vector3.Angle(transform.forward, Vector3.down);
                if ((reloadAngle <= 20f || reloadAngle >= 150f) && _currentAmmo < maxAmmo) //165
                {
                    _isReloading = true;
                } 
                else if (_isReloading) //prevents constant reloading when shooting straight up
                {
                    Reload();
                    _isReloading = false;
                }
            }

            //set the ammo on the HUD of the weapon
            if (ammoDisplay)
                ammoDisplay.SetText(_currentAmmo.ToString());
        }
    }

    public void TriggerShoot()
    {
        if (_currentAmmo > 0)
            Shoot();
        else if (_audioSource && noAmmoShootingSound)
            _audioSource.PlayOneShot(noAmmoShootingSound);
    }

    private void Shoot()
    {
        //play weapon shoot animation
        if (_animator)
        {
            _animator.ResetTrigger(Fire);
            _animator.SetTrigger(Fire);
            if (_currentAmmo == 1) _animator.SetBool(HasAmmo, false);
        }

        //spawn bullet casing + display muzzle flash
        //CasingRelease();
        var muzzleFlash = Instantiate(muzzleFlashPrefab, barrelLocation.position, barrelLocation.rotation, transform.parent);
        Destroy(muzzleFlash, 0.1f);

        if (_audioSource && shootingSound)
        {
            //play shooting sound + trigger controller vibration
            _audioSource.PlayOneShot(shootingSound);
            if (_ovrGrabbable.isGrabbed)
                ControllerVibrationManager.Instance.TriggerVibration(shootingSound,
                    _ovrGrabbable.grabbedBy.GetController());
        }

        if (_enemyUser)
        {
            //if the enemy is using the weapon, spawn a bullet and direct it towards the player
            Transform target = _mainCam.transform;

            var targetPos = (PlayerManager.Instance.autoMove.speed <= 0) ? target.position : PredictPlayerPosition(target);
            if(targetPos != Vector3.zero)
            {
                var bulletSpawnPos = barrelLocation.position;
                var bullet = ObjectPooler.Instance.SpawnFromPool("Bullet", bulletSpawnPos, barrelLocation.rotation).GetComponent<Rigidbody>();
                var direction = (targetPos - bulletSpawnPos).normalized;

                bullet.velocity = Vector3.zero;
                bullet.AddForce(direction * 100f);
                bullet.transform.rotation = Quaternion.LookRotation(direction);
                //Destroy(bullet, 10f);
                StartCoroutine(ObjectPooler.Instance.CustomDestroy(bullet.gameObject, 10f));

                GameStats.Stats.totalEnemyShots++;
            }
        }
        else
        {
            //if the player hit a UiElement
            if (_hasHitUi)
            {
                _hitUiScript.ExecuteAction();
                _hasHitUi = false;
                return;
            }
            
            //if the player is using the weapon, use a raycast to check if the shot hit
            _currentAmmo--;
            GameStats.Stats.totalShots++; //Round Stats

            int hitCount = Physics.RaycastNonAlloc(barrelLocation.position, barrelLocation.forward, _hits, 70, shootLayerFilter);

            if (hitCount > 0)
            {
                List<IEnemyDamagable> hitEnemies = new List<IEnemyDamagable>();
                int enemyIndex = 0;

                for (int i = 0; i < hitCount; i++)
                {
                    
                    if (_hits[i].transform.gameObject.CompareTag("AttackCircle"))
                    {
                        GameStats.Stats.attackCircleHits++; //Round Stats
                        GameStats.Stats.hitShots++;

                        if(_hasLastShotHitEnemy) PlayerManager.Instance.AddCombo(true);
                        _hasLastShotHitEnemy = true;
                        var hitAttackCircle = _hits[i].transform.parent.GetComponent<AttackCircleBehaviour>();
                        hitAttackCircle.GetOwner().TakeDamage(_hits[i].point, true, _isBlue);
                        hitAttackCircle.DeactivateCircle();
                        break;
                    }

                    if (_hits[i].transform.gameObject.CompareTag("Enemy"))
                    {
                        GameStats.Stats.hitShots++; //Round Stats
                        
                        if(_hasLastShotHitEnemy) PlayerManager.Instance.AddCombo(false);
                        _hasLastShotHitEnemy = true;
                        hitEnemies.Add(_hits[i].transform.GetComponent<IEnemyDamagable>());
                        break;
                    }

                    _hasLastShotHitEnemy = false;
                    PlayerManager.Instance.BreakCombo();
                }

                if (hitEnemies.Count > 0)
                {
                    if (hitEnemies.Count == 2)
                    {
                        //if 2 enemies where hit, get the closest one
                        float closestDistance = Vector3.Distance(transform.position, hitEnemies[0].transform.position);

                        for (int i = 1; i < hitEnemies.Count; i++)
                        {
                            float distance = Vector3.Distance(transform.position, hitEnemies[i].transform.position);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                enemyIndex = i;
                            }
                        }
                    }
                    
                    hitEnemies[enemyIndex].TakeDamage(_hits[enemyIndex].point, false, _isBlue);
                }
            }
            else
            {
                PlayerManager.Instance.BreakCombo();
            }
            
            //spawn a line to show the trajectory of the shot
            var spawnPosition = barrelLocation.position;
            SpawnShootLine(spawnPosition, hitCount > 0 ? _hits[0].point : spawnPosition + barrelLocation.forward * 100f);
        }
    }

    private void SpawnShootLine(Vector3 startPos, Vector3 endPos)
    {
        //spawn a line and sets its start and end position
        if (shootLinePrefab)
        {
            GameObject shootLine = Instantiate(shootLinePrefab, transform.parent);
            shootLine.GetComponent<LineRenderer>().SetPositions(
                new[]
                {
                    startPos,
                    endPos
                }
            );

            Destroy(shootLine, .01f);
        }
    }

    private Vector3 PredictPlayerPosition(Transform target)
    {
        Vector3 displacement = target.position - barrelLocation.position;
        Vector3 playerSpeed = new Vector3(0, 0, PlayerManager.Instance.autoMove.speed * 90f); //PlayerManager.Instance.autoMove.speed * 90f
        float targetMoveAngle = Vector3.Angle(-displacement, playerSpeed) * Mathf.Deg2Rad;
        
        //if the target is stopping or if it is impossible for the projectile to catch up with the target (Sine Formula)
        float projectileSpeed = 1.11f;
        
        if (playerSpeed.magnitude == 0 || playerSpeed.magnitude > projectileSpeed && Mathf.Sin(targetMoveAngle) / projectileSpeed > Mathf.Cos(targetMoveAngle) / playerSpeed.magnitude)
        {
            return Vector3.zero;
        }
        
        //also Sine Formula
        float shootAngle = Mathf.Asin(Mathf.Sin(targetMoveAngle) * playerSpeed.magnitude / projectileSpeed);
        
        return target.position + playerSpeed * displacement.magnitude / Mathf.Sin(Mathf.PI - targetMoveAngle - shootAngle) * Mathf.Sin(shootAngle) / playerSpeed.magnitude;
    }

    private void Reload()
    {
        if (_audioSource && reloadSound)
            _audioSource.PlayOneShot(reloadSound);

        if (_animator) _animator.SetBool(HasAmmo, true);
        
        //Round Stats
        GameStats.Stats.reloadCount++;
        GameStats.Stats.bulletWasteCount += _currentAmmo;
        
        _currentAmmo = maxAmmo;
    }

    private void CasingRelease()
    {
        //Spawn a casing at the casingExitLocation and make if fly away to the side of the weapon
        if (casingExitLocation && casingPrefab)
        {
            var exitPos = casingExitLocation.position;
            var casing = Instantiate(casingPrefab, exitPos, casingExitLocation.rotation);
            
            var transform1 = transform;
            casing.GetComponent<Rigidbody>().AddExplosionForce(550f,
                (exitPos - transform1.right * 0.3f - transform1.up * 0.6f), 1f);
            casing.GetComponent<Rigidbody>()
                .AddTorque(new Vector3(0, Random.Range(100f, 500f), Random.Range(10f, 1000f)), ForceMode.Impulse);

            Destroy(casing, 5f);
        }
    }

    private void SetUiBeamPositions()
    {
        //do a raycast from the weapon straight forward 
        var forward = barrelLocation.forward;
        var position = barrelLocation.position;
        
        bool hasHit = Physics.Raycast(new Ray(position, forward), out var hit, 70, shootLayerFilter);
        
        //end point for the beam
        var endPoint = position + forward * 100f;

        if (hasHit)
        {
            //override endpoint to hit point
            endPoint = hit.point;
            _hasHitUi = hit.transform.CompareTag("UI");
            
            
            if (_hasHitUi)
            {
                //set hit attribute if UI element has been hit
                _hitUiScript = hit.transform.GetComponent<VrUiElement>();
                _hitUiScript.SetHoverState(true);
            }
        }
        else
        {
            _hasHitUi = false;
            if(_hitUiScript) _hitUiScript.SetHoverState(false);
            _hitUiScript = null;
        }
        
        //set the positions of the LineRenderer component 
        _uiBeam.SetPositions(
            new[]
            {
                barrelLocation.position,
                endPoint
            }
        );
        
        SetUiPointerColor(hasHit && _hasHitUi);
    }

    private void SetUiPointerColor(bool isHovering)
    {
        if(isHovering)
            //green beam color if UI was hit 
            _uiBeam.material.SetColor(ColorProp, Color.green);
        else if(_uiBeam.startColor != Color.red)
            //red beam color as default
            _uiBeam.material.SetColor(ColorProp, Color.red);
    }

    public void SetEnemyUser(Transform enemy)
    {
        //called if the enemy is using the weapon
        _enemyUser = enemy;
        _rb.isKinematic = enemy;
        GetComponent<DistanceGrabbable>().enabled = !enemy;
        gameObject.layer = LayerMask.NameToLayer(enemy ? "Default" : "Grabbable");

        foreach (var coll in GetComponentsInChildren<Collider>())
        {
            coll.enabled = !enemy;
        }
    }

    /*public void CreateHitIndicator(Vector3 target, GameObject indicatorPrefab, float indicatorLifetime)
    {
        var position = barrelLocation.position;
        var dir = target - position;
        bool hasHit = Physics.Raycast(position, dir, out var hit, Mathf.Infinity, hitIndicatorLayerFilter);
        //Debug.DrawLine(barrelLocation.position, target)
        
        if (!hasHit) return;
        
        var indicator = Instantiate(indicatorPrefab, hit.point, Quaternion.identity);
        Destroy(indicator, indicatorLifetime);
    }*/

    public void SetGrabState(bool state, OVRGrabber hand)
    {
        //hide the hand that's holding the weapon
        hand.handSkeleton.SetActive(!state);

        Color color = Color.white;

        if (state)
        {
            if (hand.GetController() == OVRInput.Controller.RTouch)
            {
                //right hand grabbed the weapon
                _isBlue = true;
                color = Color.blue;
                _secondaryControllerTransform = GameObject.FindWithTag("LHand").transform;
            }
            else
            {
                //left hand grabbed the weapon
                _isBlue = false;
                color = Color.red;
                _secondaryControllerTransform = GameObject.FindWithTag("RHand").transform;
            }

            if (isTwoHanded)
            {
                //assault rifle was grabbed
                color = Color.green;
            }
        }
        else
        {
            //disable the laser in UI mode
            _uiBeam.enabled = false;
        }

        //apply the color to all parts of the weapon
        if (partsToColor.Length > 0)
        {
            foreach (var part in partsToColor)
            {
                part.material.SetColor(ColorProp, color);
            }
        }

        if (isTwoHanded)
        {
            var grabber = _secondaryControllerTransform.GetComponent<DistanceGrabber>();
            grabber.handSkeleton.SetActive(!state);
            grabber.useHand = !state;

            if (state)
            {
                grabber.handSkeleton.SetActive(false);
            }
            else if (grabber.grabbedObject)
            {
                grabber.handSkeleton.SetActive(false);
            }
            else
            {
                grabber.handSkeleton.SetActive(true);
            }
        }
    }

    public void SetWeaponActivation(bool isActivated)
    {
        //Activates/Deactivates weapon functionality
        _isActivated = isActivated;
        ammoDisplay.enabled = isActivated;
    }

    public Quaternion GetLookAtSecondaryHandRotation()
    {
        //return a look-rotation to the second controller (the one that is not holding the weapon)
        return Quaternion.LookRotation(_secondaryControllerTransform.position - transform.position);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<IEnemyDamagable>().Die(other.GetContact(0).point, true);
            PlayerManager.Instance.AddHealth(GameSettings.Instance.meleeHeal);
        }
    }
}