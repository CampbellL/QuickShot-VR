using System;
using System.Collections;
using System.Collections.Generic;
using LevelGeneration;
using Player;
using UnityEngine;

namespace Game
{
    public class GameHandler : MonoBehaviour
    {
        public static GameHandler Instance;

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
        
        public List<GameObject> chunks;
        private ProceduralGenerator _proceduralGenerator;
        public ProceduralGeneratorSetup proceduralGeneratorOptions;
        public GameObject uiEnvironment;

        [HideInInspector] public bool inGame;

        private void Start()
        {
            GameSettings.Instance.SetBackgroundMusic(false);
            SetupProceduralGeneration();
            
            try
            {
                GameStats.Load();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Setup the procedural generator using the options passed through the inspector and the players progress tracker.
        /// </summary>
        private void SetupProceduralGeneration()
        {
            this._proceduralGenerator = new ProceduralGenerator(
                PlayerManager.Instance.GetComponent<AutoMove>().levelRefreshSubject,
                proceduralGeneratorOptions
            );
        }

        public void StartGame()
        {
            GameStats.ResetStats();
            GameStats.Stats.startTime = Time.time;
            GameSettings.Instance.HideGameOverScreen();
            inGame = true;
            PlayerManager.Instance.StartMoving();
            GameSettings.Instance.SetBackgroundMusic(true);
            Destroy(uiEnvironment, 20f);
        }

        public void EndGame(bool isNewHighscore)
        {
            GameStats.Stats.endTime = Time.time;

            if (isNewHighscore)
            {
                GameStats.BestStats = GameStats.Stats;
                GameStats.Save();
            }

            GameStats.IsReset = true; //makes sure score is displayed after reload
            ScreenFade.Instance.FadeToLevel(0); //reloads scene
        }
    }
}
