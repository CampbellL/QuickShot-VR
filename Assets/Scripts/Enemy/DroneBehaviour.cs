using System.Collections;
using Game;
using Player;
using UnityEngine;

namespace Enemy
{
    public class DroneBehaviour : MonoBehaviour, IEnemyDamagable, IPoolerObject
    {
        public GameObject deathEffect;
        public float takeOffSpeed;
        public float turnSpeed;
        public float flightSpeed;
        public float hover;
        public float minFlightHeight;
        public float maxFlightHeight;
        public float minDistanceFromPlayer;
        public float maxDistanceFromPlayer;
        
        private bool _takingOff = true;
        private bool _reachedPlayer;
        private Rigidbody _rb;
        public float minShootCd;
        public float maxShootCd;
        private float _currentShootCd;
        [HideInInspector]public Transform target;
        private Transform _sight;
        private CapsuleCollider _colliderForOffset;
        public Transform bulletExit;
        public LayerMask shootLayer;
        private int _hitPoints = 1;
        private bool _isAlive = true;
        
        private float _flightHeight;
        private float _distanceFromPlayer;
        private Collider _coll;
        private Camera _camera;
    
        private static readonly int Amount = Shader.PropertyToID("_Amount");

        // Start is called before the first frame update
        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            target = GameObject.FindWithTag("Player").transform;
            _sight = GameObject.Find("ObstacleCollisionDetector").transform;
            _colliderForOffset = _sight.GetComponent<CapsuleCollider>();
            _coll = GetComponent<Collider>();
            _coll.enabled = false;
            _camera = Camera.main;
        }
        
        public void OnObjectSpawn()
        {
            _flightHeight = Random.Range(minFlightHeight, maxFlightHeight);
            _distanceFromPlayer = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
        }

        // Update is called once per frame
        private void FixedUpdate() 
        {
            if(!_isAlive) return;
        
            _TakeOff();
            _MoveToPosition();
            _Hovering();
        }

        void Update()
        {
            if (!target || !_isAlive) return;
            
            _TurnToPlayer();
            Shoot();
        }

        //Movement
        private void _TakeOff(){
            if(_takingOff && transform.position.y < _flightHeight-(1/takeOffSpeed)){
                _rb.velocity = new Vector3(0,takeOffSpeed,0);
                if(_flightHeight-transform.position.y<takeOffSpeed){
                    _rb.velocity=new Vector3(0,_flightHeight-transform.position.y,0);
                }
            }else if(_takingOff){
                _rb.velocity = Vector3.zero;
                _takingOff = false;
            }
        }

        private void _MoveToPosition(){
            if(!_takingOff && !_reachedPlayer){
                if(transform.position.z - target.position.z < _distanceFromPlayer){
                    _rb.velocity = new Vector3(0,0,GameSettings.Instance.playerSpeed * 90 + flightSpeed);
                }else{
                    _reachedPlayer = true;
                    _coll.enabled = true;
                }
            }else if(!_takingOff){
                if(GameSettings.Instance.playerSpeed > 0){
                    _rb.velocity = new Vector3 (0,0,PlayerManager.Instance.autoMove.speed * 90f);
                }else{
                    _rb.velocity = Vector3.zero;
                }
            }
        }
        
        private void _Hovering()
        {
            hover += Time.deltaTime * 4;
            var tempPos = transform.position;
            tempPos.y += Mathf.Sin(hover)*0.02f;
            transform.position = tempPos;
        }

        private void _TurnToPlayer()
        {
            if(!_takingOff && _reachedPlayer){
                var lookPos = target.position - transform.position;
                lookPos.y = 0;
                var rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * turnSpeed);
            }
        }

        //Combat
        private void Shoot(){
            if(!_takingOff && _reachedPlayer){
                if(ShootCd() && _PlayerInSight()){
                    target = _camera.transform;

                    var targetPos = (PlayerManager.Instance.autoMove.speed <= 0) ? target.position : _PredictPlayerPosition(target);
                    if(targetPos != Vector3.zero)
                    {
                        //var bullet = Instantiate(bulletPrefab, bulletExit.position, bulletExit.rotation).GetComponent<Rigidbody>();
                        var bulletExitPos = bulletExit.position;
                        var bullet = ObjectPooler.Instance.SpawnFromPool("Bullet", bulletExitPos, bulletExit.rotation).GetComponent<Rigidbody>();
                        var direction = (targetPos - bulletExitPos).normalized;

                        bullet.AddForce(direction * 100f);
                        bullet.transform.rotation = Quaternion.LookRotation(direction);
                        //Destroy(bullet, 10f);
                        StartCoroutine(ObjectPooler.Instance.CustomDestroy(bullet.gameObject, 10f));
                    }
                }
            }
        }

        private Vector3 _PredictPlayerPosition(Transform player)
        {
            Vector3 displacement = player.position - bulletExit.position;
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
        
            return player.position + playerSpeed * displacement.magnitude / Mathf.Sin(Mathf.PI - targetMoveAngle - shootAngle) * Mathf.Sin(shootAngle) / playerSpeed.magnitude;
        }

        private bool ShootCd(){
            if((_currentShootCd-=Time.deltaTime)<0){
                _currentShootCd = Random.Range(minShootCd,maxShootCd);
                return true;
            }else{
                return false;
            }
        }

        private bool _PlayerInSight()
        {
            //Raycast from Bullet exit to target head
            //if nothing between sight returns true
            var tempSight = _sight.position + _colliderForOffset.center;
            Vector3 direction = (tempSight - bulletExit.position).normalized;
            
            if (Physics.Raycast(bulletExit.position, direction, out var hit, Mathf.Infinity, shootLayer))
            {
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    return true;
                }
            }
            return false;
        }

        //Gets called when the Player hits the enemy
        public Transform transform { get => base.transform; }

        public void TakeDamage(Vector3 hitpoint, bool isCrit, bool isBlue)
        {
            //Checks if the Enemy still has HP left
            if (--_hitPoints == 0)
            {
                Die(hitpoint, isCrit);
            }
        }

        public void Die(Vector3 hitpoint, bool isCrit)
        {
            _rb.useGravity = true;
            _isAlive = false;

            //Give points to player
            int scoreReward = (isCrit) ? 200 : 100;
            PlayerManager.Instance.AddScore(scoreReward);
            TextPopup.Create(hitpoint, scoreReward, isCrit, PlayerManager.Instance.CurrentCombo);

            if (deathEffect)
            {
                Instantiate(deathEffect, hitpoint, Quaternion.identity).transform.LookAt(PlayerManager.Instance.transform);
            }

            StartCoroutine(Dissolve(1f, .7f));
            StartCoroutine(RotateOverTime(0.3f));
        
            //Round Stats
            GameStats.Stats.enemiesKilled++;
        }
    
        private IEnumerator Dissolve(float duration, float startDelay)
        {
            yield return new WaitForSecondsRealtime(startDelay);
            
            float counter = 0;

            while (counter < duration)
            {
                counter += Time.deltaTime;

                GetComponent<MeshRenderer>().material.SetFloat(Amount, Mathf.Lerp(0, 1, counter / duration));
                yield return new WaitForEndOfFrame();
            }

            StartCoroutine(ObjectPooler.Instance.CustomDestroy(gameObject));
        }

        private IEnumerator RotateOverTime(float duration)
        {
            float counter = 0;

            while (counter < duration)
            {
                counter += Time.deltaTime;
                transform.Rotate(transform.right, Mathf.Lerp(0, 90f, counter / duration));
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
