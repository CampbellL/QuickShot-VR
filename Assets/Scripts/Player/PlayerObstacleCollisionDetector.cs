using UnityEngine;
using Game;

namespace Player
{
    public class PlayerObstacleCollisionDetector : MonoBehaviour
    {
        [SerializeField] private Transform centerEyeCamera;
        [SerializeField] private float collisionRadius;
        private CapsuleCollider _collider;

        // Start is called before the first frame update
        void Start()
        {
            _collider = GetComponent<CapsuleCollider>();
            _collider.radius = collisionRadius;
        }
    
        void LateUpdate()
        {
            UpdateCollider();
        }

        void UpdateCollider()
        {
            var cameraWorldPos = centerEyeCamera.position;
            var cameraLocalPos = centerEyeCamera.localPosition;
        
            //Adjust collider height + collider position according to the HMD position
            _collider.center = new Vector3(cameraLocalPos.x, (cameraWorldPos.y / 2) + 0.15f, cameraLocalPos.z);
            _collider.height = cameraWorldPos.y;
        }

        private void OnTriggerEnter(Collider other)
        {
            //Check For Collisions with obstacles
            if (other.CompareTag("Obstacle") || other.CompareTag("Enemy"))
            {
                GameStats.Stats.obstacleHitCount++; //Round Stats
                PlayerManager.Instance.AddHealth(-GameSettings.Instance.bulletDamage);
            }
            else if (other.CompareTag("Bullet"))
            {
                GameStats.Stats.enemyHitCount++; //Round Stats
                PlayerManager.Instance.AddHealth(-GameSettings.Instance.bulletDamage);
            }
        }
    }
}
