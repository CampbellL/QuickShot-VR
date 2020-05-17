using UniRx;
using UnityEngine;

namespace Player
{
    public class AutoMove : MonoBehaviour
    {
        [HideInInspector] public float speed;
        private Transform _transform;

        public Subject<bool> levelRefreshSubject;
        // Start is called before the first frame update
        private void Awake()
        {
            this._transform = this.GetComponent<Transform>();
            this.levelRefreshSubject = new Subject<bool>();
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            Vector3 position = this._transform.position;
            position = new Vector3(position.x, position.y, position.z + speed);
            this._transform.position = position;
        
            PlayerManager.Instance.AddDistance(speed);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.CompareTag("Tracker"))
                this.levelRefreshSubject.OnNext(true);
        }
    }
}
