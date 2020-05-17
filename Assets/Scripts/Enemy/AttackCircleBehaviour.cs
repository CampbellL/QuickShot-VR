using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class AttackCircleBehaviour : MonoBehaviour
    {
        private Transform _centerEyeAnchor;
        private Transform _owner;
        private List<Transform> _attackCircles = new List<Transform>();
        private Transform _activeAttackCircle;
        private bool _isEnabled;

        public float startScale = 1.5f;
        public float zRotDegPerSec = 100f;
    
        [Space][Header("Shrink Only Settings (Default)")]
        public float minScale = 0.2f;
        public float shrinkSpeed = 0.005f;
    
        [Space][Header("Pulse Settings")]
        public bool usePulse;
        public float pulseMaxScale = 1f;
        public float pulseMinScale = 0.2f;
        public float pulseShrinkSpeed = 0.02f;
        public float pulseGrowSpeed = 0.04f;
    
    
        // Start is called before the first frame update
        void Start()
        {
            _centerEyeAnchor = GameObject.Find("CenterEyeAnchor").transform;
            _owner = transform.parent;
            transform.parent = null;
        
            foreach (Transform ac in transform)
            {
                ac.gameObject.SetActive(false);
                _attackCircles.Add(ac);
            }
        }
    
        private void LateUpdate()
        {
            if (_owner)
            {
                transform.position = _owner.position;
            
                if (_isEnabled && _activeAttackCircle)
                {
                    _activeAttackCircle.LookAt(_centerEyeAnchor, _activeAttackCircle.up);
                    _activeAttackCircle.Rotate(0, 0, zRotDegPerSec * Time.deltaTime); //rotates 50 deg/s around z axis
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ActivateCircle()
        {
            _isEnabled = true;
        
            if (_attackCircles.Count > 0)
            {
                int rand = Random.Range(0, _attackCircles.Count);
                _activeAttackCircle = _attackCircles[rand];
                if(_activeAttackCircle) _activeAttackCircle.gameObject.SetActive(true);

                StartCoroutine(usePulse ? Pulse() : ShrinkAttackCircle());
            }
        }

        public void DeactivateCircle()
        {
            //transform.parent = _owner;
            _isEnabled = false;
            _activeAttackCircle = null;
            
            StopAllCoroutines();
            
            foreach (Transform ac in transform)
            {
                ac.gameObject.SetActive(false);
            }
        }

        public bool CompareOwner(GameObject owner)
        {
            return owner.transform == _owner;
        }

        public EnemyBehaviourScript GetOwner()
        {
            return _owner.GetComponent<EnemyBehaviourScript>();
        }

        IEnumerator ShrinkAttackCircle()
        {
            yield return new WaitForSecondsRealtime(3f);

            float currentRatio = startScale;

            // Shrink for a few seconds
            while (currentRatio != minScale)
            {
                // Determine the new ratio to use
                currentRatio = Mathf.MoveTowards( currentRatio, minScale, shrinkSpeed);
 
                // Update our text element
                _activeAttackCircle.localScale = Vector3.one * currentRatio;

                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator Pulse()
        {
            float currentRatio = startScale;
        
            // Run this indefinitely
            while (_isEnabled)
            {
                // Grow until min is reached
                while (currentRatio != pulseMaxScale)
                {
                    // Determine the new ratio to use
                    currentRatio = Mathf.MoveTowards( currentRatio, pulseMaxScale, pulseGrowSpeed);
 
                    // Update our object scale
                    _activeAttackCircle.localScale = Vector3.one * currentRatio;

                    yield return new WaitForEndOfFrame();
                }
 
                // Shrink until min is reached
                while (currentRatio != pulseMinScale)
                {
                    // Determine the new ratio to use
                    currentRatio = Mathf.MoveTowards( currentRatio, pulseMinScale, pulseShrinkSpeed);
 
                    // Update our text element
                    _activeAttackCircle.localScale = Vector3.one * currentRatio;

                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }
}
