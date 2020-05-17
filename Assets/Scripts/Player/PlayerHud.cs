using TMPro;
using UnityEngine;

namespace Player
{
    public class PlayerHud : MonoBehaviour
    {
        public static PlayerHud Instance;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        public Transform hud;
        public Transform targetTransform;
        public float smoothFactor = 2f;
        public bool rotateWithHmd;
        public Animator comboAnim;

        public TextMeshPro healthText;
        public TextMeshPro scoreText;
        public TextMeshPro distanceText;
        public TextMeshPro comboText;
        
        private Transform _camera;
        private static readonly int ComboEffectTrigger = Animator.StringToHash("ComboEffectTrigger");

        private void Start()
        {
            _camera = Camera.main.transform;
            hud.parent = null;
            hud.position = targetTransform.position;
        }

        private void LateUpdate()
        {
            MoveToTarget(targetTransform.position, rotateWithHmd);
        }

        private void MoveToTarget(Vector3 targetPosition, bool copyZRotation = false)
        {
            var hudPos = hud.position;
            hudPos = Vector3.Lerp(hudPos, targetPosition, Time.deltaTime * smoothFactor);
            hudPos.z = targetPosition.z;
            
            hud.position = hudPos;
            
            if (copyZRotation)
            {
                hud.LookAt(_camera, targetTransform.up);
                return;
            }
            
            hud.LookAt(_camera);
        }

        public void SetHealthHud(int health)
        {
            healthText.SetText(health.ToString());
        }
        
        public void SetScoreHud(int score)
        {
            scoreText.SetText(score.ToString());
        }
        
        public void SetDistanceHud(float distance)
        {
            distanceText.SetText((int)distance + "m");
        }

        public void SetComboHud(int combo, bool isComboBreak = false)
        {
            comboText.SetText( "x" + combo);

            if (!isComboBreak)
            {
                comboAnim.SetTrigger(ComboEffectTrigger);
            }
        }
    }
}
