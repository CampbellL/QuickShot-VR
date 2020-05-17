using System.Collections;
using Game;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;

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
            
            autoMove = GetComponent<AutoMove>();
        }

        public int maxHealth = 100;
        public int maxCombo = 10;
        public PostProcessVolume ppDamageEffect;
        public PostProcessVolume ppHealEffect;
        public GameObject pointsPopupPrefab;
        public int CurrentHealth { get; private set; }
        public int CurrentScore { get; private set; }
        public int CurrentCombo { get; private set; }
        public float CurrentDistance { get; private set; }

        [HideInInspector] public AutoMove autoMove;
        private Coroutine _currentEffect;

        // Start is called before the first frame update
        private void Start()
        {
            CurrentHealth = maxHealth;
            CurrentCombo = 1;
        }

        public void StartMoving()
        {
            autoMove.speed = GameSettings.Instance.playerSpeed;
        }
        
        public void AddHealth(int amount)
        {
            if (amount > 0)
            {
                if(_currentEffect != null)
                    StopCoroutine(_currentEffect);
                    
                _currentEffect = StartCoroutine(DisplayHealEffect());
            }
            else if (amount < 0)
            {
                if(_currentEffect != null)
                    StopCoroutine(_currentEffect);
                
                _currentEffect = StartCoroutine(DisplayDamageEffect());
            }

            CurrentHealth += amount;
            PlayerHud.Instance.SetHealthHud(CurrentHealth);
            
            if(CurrentHealth <= 0) Die();
        }

        public void AddScore(int amount)
        {
            CurrentScore += amount * CurrentCombo;
            PlayerHud.Instance.SetScoreHud(CurrentScore);
        }

        public void AddCombo(bool isAttackCircle)
        {
            if (isAttackCircle)
            {
                if (CurrentCombo + 2 <= maxCombo) CurrentCombo += 2;
                else CurrentCombo = maxCombo;
            }
            else if (CurrentCombo != maxCombo)
                CurrentCombo++;

            if (CurrentCombo > GameStats.Stats.highestCombo)
                GameStats.Stats.highestCombo = CurrentCombo;
            
            PlayerHud.Instance.SetComboHud(CurrentCombo);
        }

        public void BreakCombo()
        {
            CurrentCombo = 1;
            PlayerHud.Instance.SetComboHud(CurrentCombo, true);
        }

        public void AddDistance(float distance)
        {
            CurrentDistance += distance;
            PlayerHud.Instance.SetDistanceHud(CurrentDistance);
        }

        private void Die()
        {
            autoMove.speed = 0f;

            var stats = GameStats.Stats;
            GameStats.Stats.accuracyBonus = (stats.totalShots != 0) ? CurrentScore * (stats.hitShots / stats.totalShots) : 0;
            GameStats.Stats.obstaclesDodgedBonus = (stats.totalObstaclesSpawned != 0) ? CurrentScore * (stats.obstacleHitCount / stats.totalObstaclesSpawned) : 0;
            GameStats.Stats.distanceBonus = (int)(CurrentDistance / 2);
            GameStats.Stats.bulletsDodgedBonus = (stats.totalEnemyShots != 0) ? CurrentScore * (stats.enemyHitCount / stats.totalEnemyShots) : 0;
            GameStats.Stats.highestComboBonus = (stats.highestCombo != 1) ? CurrentScore * (1 + stats.highestCombo / 100) : 0;
            
            CurrentScore += GameStats.Stats.accuracyBonus; //Accuracy bonus
            CurrentScore += GameStats.Stats.obstaclesDodgedBonus; //obstacles dodged bonus
            CurrentScore += GameStats.Stats.distanceBonus; //Distance bonus
            CurrentScore += GameStats.Stats.bulletsDodgedBonus; //bullets dodged bonus
            CurrentScore += GameStats.Stats.highestComboBonus; //highest combo bonus

            GameStats.Stats.score = CurrentScore;
            GameStats.Stats.distance = CurrentDistance;
            GameHandler.Instance.EndGame(CurrentScore > GameStats.Highscore);
        }

        private IEnumerator DisplayDamageEffect()
        {
            if (!ppDamageEffect)
            {
                Debug.LogError("No Damage Effect Volume selected!");
                yield break;
            }
            
            float currentRatio = ppDamageEffect.weight;
            
                // Grow until min is reached
            while (currentRatio != 1f) 
            {
                // Determine the new ratio to use
                currentRatio = Mathf.MoveTowards( currentRatio, 1f, 0.1f);
 
                // Update the weight of the effect
                ppDamageEffect.weight = currentRatio;

                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForSecondsRealtime(1f);
            
            // Shrink until min is reached
            while (currentRatio != 0)
            {
                // Determine the new ratio to use
                currentRatio = Mathf.MoveTowards( currentRatio, 0, 0.01f);
 
                // Update the weight of the effect
                ppDamageEffect.weight = currentRatio;

                yield return new WaitForEndOfFrame();
            }

            _currentEffect = null;
        }
        
        IEnumerator DisplayHealEffect()
        {
            if (!ppHealEffect)
            {
                Debug.LogError("No Heal Effect Volume selected!");
                yield break;
            }
            
            float currentRatio = ppHealEffect.weight;
            
            // Grow until min is reached
            while (currentRatio != 1f) 
            {
                // Determine the new ratio to use
                currentRatio = Mathf.MoveTowards( currentRatio, 1f, 0.1f);
 
                // Update our object scale
                ppHealEffect.weight = currentRatio;

                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForSecondsRealtime(.5f);
            
            // Shrink until min is reached
            while (currentRatio != 0)
            {
                // Determine the new ratio to use
                currentRatio = Mathf.MoveTowards( currentRatio, 0, 0.1f);
 
                // Update our text element
                ppHealEffect.weight = currentRatio;

                yield return new WaitForEndOfFrame();
            }
            
            _currentEffect = null;
           
        }
    }
}
