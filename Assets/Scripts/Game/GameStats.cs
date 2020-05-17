using System.Diagnostics;
using UnityEngine;

namespace Game
{
    [System.Serializable]
    public struct Stats
    {
        //RoundStats
        public int score;
        public float startTime;
        public float endTime;
        public float distance;
        public int totalShots;
        public int hitShots;
        public int highestCombo;
        public int attackCircleHits;
        public int enemiesKilled;
        public int reloadCount;
        public int bulletWasteCount;
        public int totalEnemyShots;
        public int enemyHitCount;
        public int totalObstaclesSpawned;
        public int obstacleHitCount;

        public int accuracyBonus;
        public int obstaclesDodgedBonus;
        public int distanceBonus;
        public int bulletsDodgedBonus;
        public int highestComboBonus;
    }
    
    [System.Serializable]
    public struct Settings
    {
        public bool isRightHand;
        public float masterVolumeValue;
        public float musicVolumeValue;
        public float sfxVolumeValue;
    }
    
    [System.Serializable]
    public struct Save
    {
        public Stats bestStats;
        public Settings gameSettings;
    }
    
    public static class GameStats
    {
        public static Stats BestStats;
        public static Stats Stats;
        public static bool IsReset;
        public static int Highscore = -1;

        public static void ResetStats()
        {
            Stats = new Stats();
        }

        public static void Save()
        {
            var settings = new Settings
            {
                isRightHand = GameSettings.Instance.isRightHand,
                masterVolumeValue = GameSettings.Instance.masterVolumeSlider.value,
                musicVolumeValue = GameSettings.Instance.musicVolumeSlider.value,
                sfxVolumeValue = GameSettings.Instance.sfxVolumeSlider.value
            };

            //Save the best run (only the stats of the best run is saved) and the game settings
            var save = new Save {bestStats = BestStats, gameSettings = settings};
            SaveGameManager<Save>.Save(save);
        }

        public static void Load()
        {
            var save = SaveGameManager<Save>.Load();
            BestStats = save.bestStats; //set the best score
            GameSettings.Instance.ApplySettings(save.gameSettings); //apply the saved game settings
            GameSettings.Instance.DisplayStatScreen(); //display stats on the respective panels
        }
    }
}
