using UnityEngine;

namespace Assets.Animations.Enemies.NormalEnemy
{
    public class AnimScript : MonoBehaviour
    {

        public Animator anim;
    
        public enum AnimationType {Idle01, Idle02, Move};        //Those are the anims you can choose from

        private const float IdlePositionX01 = -1.0f; //For all the different animations
        private const float IdlePositionY01 = -1.0f; //For all the different animations

        private const float IdlePositionX02 = 1.0f; //For all the different animations
        private const float IdlePositionY02 = -1.0f; //For all the different animations

        private const float MovePositionX = 0.0f; //For all the different animations
        private const float MovePositionY = 1.0f; //For all the different animations


        private readonly float _duration = 0.5f;    // Time taken for the transition.
        private float _startTime;
        private float _startPosX;
        private float _startPosY;
        private AnimationType _destinationType;
    
        private bool _hasTransitioned = true;
        private static readonly int PosX = Animator.StringToHash("PosX");
        private static readonly int PosY = Animator.StringToHash("PosY");
        private static readonly int Shoot = Animator.StringToHash("Shoot");


        // Start is called before the first frame update
        void Start()
        {
            //startTime = Time.time;    // Make a note of the time the script started.
            Reset();
        }

        public void Reset()
        {
            anim.SetFloat(PosX, IdlePositionX02);
            anim.SetFloat(PosY, IdlePositionY02);
        }

        // Update is called once per frame
        void Update()
        {
            if (_hasTransitioned == false)
            {
                TransitionAnimation(_destinationType);
            }
        }

        public void SwitchToAnimation(AnimationType animType)
        {
            _destinationType = animType;
            _startTime = Time.time;
            _startPosX = anim.GetFloat(PosX);
            _startPosY = anim.GetFloat(PosY);
            _hasTransitioned = false;
            TransitionAnimation(animType);
        }

        public void PlayShoot()
        {
            anim.SetTrigger(Shoot);
        }

        private void TransitionAnimation(AnimationType animType)
        {
            float destPosX;
            float destPosY;
        
            switch (animType)
            {
                case AnimationType.Idle01:
                {
                    destPosX = IdlePositionX01;
                    destPosY = IdlePositionY01;
                
                    break;
                }
                case AnimationType.Idle02:
                {
                    destPosX = IdlePositionX02;
                    destPosY = IdlePositionY02;
                
                    break;
                }
                case AnimationType.Move:
                {
                    destPosX = MovePositionX;
                    destPosY = MovePositionY;
                
                    break;
                }

                default:
                {
                    destPosX = 0.0f;
                    destPosY = 0.0f;
                
                    break;
                }
            }
        
            float t = (Time.time - _startTime) / _duration;    // Calculate the fraction of the total duration that has passed.
                
            float newPosX = Mathf.SmoothStep(_startPosX, destPosX, t);
            float newPosY = Mathf.SmoothStep(_startPosY, destPosY, t);

            anim.SetFloat(PosX, newPosX);
            anim.SetFloat(PosY, newPosY);

            if (newPosX == destPosX && newPosY == destPosY)
            {
                _hasTransitioned = true;
            }
        }
    
    
    }
}
