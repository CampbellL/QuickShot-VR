using System;
using TMPro;
using UnityEngine;

namespace Game
{
    public class StatsUi : MonoBehaviour
    {
        public TextMeshProUGUI scoreField;
        public TextMeshProUGUI distanceField;
        public TextMeshProUGUI enemiesKilledField;
        public TextMeshProUGUI timeField;
        public TextMeshProUGUI highestComboField;
        public TextMeshProUGUI shootAccuracyField;
        public TextMeshProUGUI reloadCountField;
        public TextMeshProUGUI obstaclesDodgedField;
        public TextMeshProUGUI bulletsDodgedField;

        private void Start()
        {
            DisplayStats(GameStats.BestStats);
        }

        public void DisplayStats(Stats stats)
        {
            TimeSpan time = TimeSpan.FromSeconds(stats.endTime - stats.startTime);
            string shootAccuracy = (stats.totalShots != 0) ? stats.hitShots * 100 / stats.totalShots + "% (+" + stats.accuracyBonus + " pts)" : "0% (+0 pts)";
            string obstaclesDodged = (stats.totalObstaclesSpawned != 0) ? stats.obstacleHitCount * 100 / stats.totalObstaclesSpawned + "% (+" + stats.obstaclesDodgedBonus + " pts)" : "0% (+0 pts)";
            string bulletsDodged = (stats.totalEnemyShots != 0) ? stats.enemyHitCount * 100 / stats.totalEnemyShots + "% (+" + stats.bulletsDodgedBonus+ " pts)" : "0% (+0 pts)";
            
            scoreField.SetText(stats.score + " pts");
            distanceField.SetText((int)stats.distance + "m" + " (+" + stats.distanceBonus + " pts)");
            enemiesKilledField.SetText(stats.enemiesKilled.ToString());
            timeField.SetText(time.ToString(@"hh\:mm\:ss\:fff"));
            highestComboField.SetText("x" + stats.highestCombo + " (+" + stats.highestComboBonus + " pts)");
            shootAccuracyField.SetText(shootAccuracy);
            reloadCountField.SetText(stats.reloadCount.ToString());
            obstaclesDodgedField.SetText(obstaclesDodged);
            bulletsDodgedField.SetText(bulletsDodged);
        }
    }
}
