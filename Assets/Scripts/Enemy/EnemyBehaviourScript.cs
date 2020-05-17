using System.Collections;
using Assets.Animations.Enemies.NormalEnemy;
using Game;
using LevelGeneration;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy
{
    public enum EnemyType
    {
        Tank,
        Melee,
        Normal
    }

    public class EnemyBehaviourScript : MonoBehaviour, IEnemyDamagable
    {
        public Renderer bodyRenderer;
        public Transform head;
        public GameObject ragdollRoot;
        [HideInInspector] public int hitPoints;
        public float turnSpeed = 20f;
        public float moveSpeed = 5f;
        public float minShootDelay;
        public float maxShootDelay;
        public float distanceToSpawn = 10f;
        private float _currentShootDelay;

        public EnemyType type;
        public LayerMask shootLayer;
        public float maxShootDistance;
        public GameObject deathEffect;

        [HideInInspector] public Transform target;
        private Transform _sight;
        private CapsuleCollider _colliderForOffset;

        private WeaponBehaviour _gun;
        private AnimScript _animator;
        private bool _animationMoving;
        private Vector3 _startPosition;
        private Quaternion _startRotation;

        public bool move = true;
        public Transform location;
        public AttackCircleBehaviour attackCircles;
        
        private bool _isActive;
        private bool _isAlive = true;
        private Coroutine _spawnRoutine;
        private Test.EnemyDamageType.Script.EnemyType _enemyTypeScript;
        
        private static readonly int Amount = Shader.PropertyToID("_Amount");
        private static readonly int GlowFalloff = Shader.PropertyToID("_GlowFalloff");
        private static readonly int GlowRange = Shader.PropertyToID("_GlowRange");

        private void Start()
        {
            _startPosition = transform.localPosition;
            _startRotation = transform.localRotation;
            
            //Assign attributes
            _gun = GetComponentInChildren<WeaponBehaviour>();
            _animator = GetComponent<AnimScript>();

            target = GameObject.FindWithTag("Player").transform;
            _sight = GameObject.Find("ObstacleCollisionDetector").transform;
            _colliderForOffset = _sight.GetComponent<CapsuleCollider>();

            _enemyTypeScript = GetComponent<Test.EnemyDamageType.Script.EnemyType>();
            
            Setup();
        }

        private void Setup()
        {
            //detach location transform from enemy 
            location.parent = null;
            
            _gun.SetEnemyUser(transform);
            _currentShootDelay = Random.Range(minShootDelay, maxShootDelay);
            SetHp();

            SetRigidbodyState(true);
            SetColliderState(false);
        }

        private void SetHp()
        {
            switch (type)
            {
                case EnemyType.Tank:
                    hitPoints = 3;
                    break;
                default:
                    hitPoints = 1;
                    break;
            }
        }
        
        private void Respawn()
        {
            GetComponent<Animator>().enabled = true;
            _animator.enabled = true;
            _animator.Reset();
            Setup();

            transform.localPosition = _startPosition;
            transform.localRotation = _startRotation;

            _isActive = false;
            _animationMoving = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!GameHandler.Instance.inGame) return;
            
            if (!_isActive)
            {
                var pos = transform.position;
                pos.y = 0;
                
                if (_spawnRoutine == null && Vector3.Distance(pos, target.position) < distanceToSpawn)
                {
                    _spawnRoutine = StartCoroutine(Spawn(0.5f));
                }
                return;
            }
            
            if (move && _isAlive)
            {
                MoveToLocation();
                return;
            }
            
            TurnToPlayer();
            CheckShoot();
        }
        
        private void MoveToLocation()
        {
            if (!location) return;
            
            if (!_animationMoving)
            {
                transform.LookAt(location);
                _animator.SwitchToAnimation(AnimScript.AnimationType.Move);
                _animationMoving = true;
            }
            
            transform.position = Vector3.MoveTowards(transform.position, location.position, moveSpeed * Time.deltaTime);
            
            if (Vector3.Distance(location.position, transform.position) > 0.1f) return;
            
            move = false;
            _animator.SwitchToAnimation(AnimScript.AnimationType.Idle02);
        }

        //Checks sight to player is not obstructed by anything
        private bool PlayerInSight()
        {
            //Raycast from enemy head to target head
            //if nothing between sight returns true
            var headPos = head.position;
            var tempSight = _sight.position + _colliderForOffset.center;
            Vector3 direction = (tempSight - headPos).normalized;

            if (Physics.Raycast(head.position, direction, out var hit, maxShootDistance, shootLayer))
            {
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    return true;
                }
            }
            return false;
        }

        //Enemy shoot Player
        private void CheckShoot()
        {
            if (!((_currentShootDelay -= Time.deltaTime) < 0)) return;
            
            if (PlayerInSight())
            {
                //_gun.CreateHitIndicator(Camera.main.transform.position, GameSettings.Instance.indicatorPrefab, GameSettings.Instance.indicatorLifetime);
                _animator.PlayShoot();
            }
            
            _currentShootDelay = Random.Range(minShootDelay, maxShootDelay);
        }
        
        //Called as an event in the enemy shoot animation 
        public void Shoot()
        {
            _gun.TriggerShoot();
        }

        //Gets called when the Player hits the enemy
        public void TakeDamage(Vector3 hitpoint, bool isCrit, bool isBlue)
        {
            if (!_isActive) return;
            
            if (_enemyTypeScript.CheckShield(isBlue, hitpoint) && _enemyTypeScript.CheckEnemy(isBlue, hitpoint))
            {
                //Checks if the Enemy still has HP left
                if (--hitPoints == 0)
                {
                    Die(hitpoint, isCrit);
                }
                else
                {
                    //Resets the time between shots
                    _currentShootDelay = Random.Range(minShootDelay, maxShootDelay);
                }
            }
        }

        //Gets called when this has HP=0 by TakeDamage()
        public void Die(Vector3 hitPoint, bool isCrit)
        {
            //enable Ragdoll mode
            GetComponent<Animator>().enabled = false;
            _animator.enabled = false;
            SetRigidbodyState(false);
            SetColliderState(true);
            
            attackCircles.DeactivateCircle();
            location.parent = transform;

            _isAlive = false;
            
            //add force to hitpoint
            foreach (var obj in Physics.OverlapSphere(hitPoint, .5f))
            {
                var rb = obj.GetComponent<Rigidbody>();
                if(rb) rb.AddExplosionForce(3000, hitPoint, .5f);
            }

            //Give points to player
            int scoreReward = (isCrit) ? 200 : 100;
            PlayerManager.Instance.AddScore(scoreReward);
            TextPopup.Create(hitPoint, scoreReward, isCrit, PlayerManager.Instance.CurrentCombo);

            if (deathEffect)
            {
                var effect = Instantiate(deathEffect, hitPoint, Quaternion.identity);
                effect.transform.LookAt(PlayerManager.Instance.transform);
                Destroy(effect, 1f);
            }
            
            //Start Dissolve Effect on enemy
            StartCoroutine(Dissolve(1f, 0.5f));
            
            //Round Stats
            GameStats.Stats.enemiesKilled++;
        }

        //Player turns in the direction of the target
        private void TurnToPlayer()
        {
            var lookPos = target.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * turnSpeed);
        }

        private void SetRigidbodyState(bool state)
        {
            Rigidbody[] rigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = state;
            }
        }
    
        private void SetColliderState(bool state)
        {
            Collider[] colliders = ragdollRoot.GetComponentsInChildren<Collider>();

            foreach (Collider coll in colliders)
            {
                coll.enabled = state;
            }

            GetComponent<Collider>().enabled = !state;
        }

        private IEnumerator Dissolve(float duration, float startDelay)
        {
            yield return new WaitForSecondsRealtime(startDelay);
            
            float counter = 0;
            bodyRenderer.material.SetFloat(GlowFalloff, 0.06f);
            bodyRenderer.material.SetFloat(GlowRange, 0.035f);

            while (counter < duration)
            {
                counter += Time.deltaTime;

                bodyRenderer.material.SetFloat(Amount, Mathf.Lerp(0, 1, counter / duration));
                yield return new WaitForEndOfFrame();
            }

            EnemySpawner.Instance.RemoveActiveEnemy(gameObject);
            //Destroy(gameObject);
            Respawn();
        }

        private IEnumerator Spawn(float duration)
        {
            float counter = 0;
            GetComponent<Test.EnemyDamageType.Script.EnemyType>().DisplayShield();
            bodyRenderer.material.SetFloat(GlowFalloff, 0.06f);
            bodyRenderer.material.SetFloat(GlowRange, 0.035f);

            while (counter < duration)
            {
                counter += Time.deltaTime;

                bodyRenderer.material.SetFloat(Amount, Mathf.Lerp(1, 0, counter / duration));
                yield return new WaitForEndOfFrame();
            }
            
            bodyRenderer.material.SetFloat(GlowFalloff, 0.001f);
            bodyRenderer.material.SetFloat(GlowRange, 0);
            attackCircles.ActivateCircle();
            _isActive = true;
            EnemySpawner.Instance.activeEnemies.Add(gameObject);
        }
    }
}