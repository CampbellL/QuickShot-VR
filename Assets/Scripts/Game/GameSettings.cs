using Player;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    [System.Serializable]
    public class GameSettings : MonoBehaviour
    {
        public static GameSettings Instance;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
        
        [Header("Settings")] 
        public bool isRightHand = true;
        [Range(0, 1)] public float enemyShieldChance;
        public int bulletDamage = 30;
        public int obstacleDamage = 30;
        public int meleeHeal = 30;
        public float playerSpeed;
        public GameObject notAllowedPrefab;
        public AudioClip inGameMusic;
        public AudioClip menuMusic;

        [Space] [Header("Controls")] 
        public Slider masterVolumeSlider;
        public VrUiElement creditsButton;
        public VrUiElement settingsButton;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public Image rightButtonBg;
        public Image leftButtonBg;

        [Space] [Header("References")] 
        public AudioSource musicPlayer;
        public AudioSource sfxTestPlayer;
        public StatsUi bestRunScreen;
        public StatsUi gameOverScreen;
        public GameObject settingsScreen;
        public GameObject creditsScreen;

        [HideInInspector] public float sfxVolume = 1f;

        private static readonly int FadeIn = Animator.StringToHash("FadeIn");
        private static readonly int FadeOut = Animator.StringToHash("FadeOut");

        public void UpdateMasterVolume()
        {
            AudioListener.volume = masterVolumeSlider.value;
        }

        public void UpdateMusicVolume()
        {
            musicPlayer.volume = musicVolumeSlider.value;
        }

        public void UpdateSfxVolume()
        {
            sfxVolume = sfxTestPlayer.volume = sfxVolumeSlider.value;
        }

        public void SetRightHand()
        {
            isRightHand = true;
            AssaultRifleGrabber.Instance.SetRifleParent(true);
            rightButtonBg.color = Color.yellow;
            leftButtonBg.color = Color.white;
        }

        public void SetLeftHand()
        {
            isRightHand = false;
            AssaultRifleGrabber.Instance.SetRifleParent(false);
            leftButtonBg.color = Color.yellow;
            rightButtonBg.color = Color.white;
        }

        public void ApplySettings(Settings savedSettings)
        {
            masterVolumeSlider.value = savedSettings.masterVolumeValue;
            musicVolumeSlider.value = savedSettings.musicVolumeValue;
            sfxVolumeSlider.value = savedSettings.sfxVolumeValue;
            isRightHand = savedSettings.isRightHand;

            UpdateMasterVolume();
            UpdateMusicVolume();
            UpdateSfxVolume();
        }

        public void DisplayStatScreen()
        {
            bestRunScreen.DisplayStats(GameStats.BestStats);

            if (GameStats.IsReset)
            {
                gameOverScreen.DisplayStats(GameStats.Stats);
                gameOverScreen.GetComponent<Animator>().SetTrigger(FadeIn);
            }
        }

        public void HideGameOverScreen()
        {
            if (GameStats.IsReset)
            {
                gameOverScreen.GetComponent<Animator>().SetTrigger(FadeOut);
            }
        }

        public void DisplayCredits()
        {
            creditsButton.ToggleActivation(false);
            settingsButton.ToggleActivation(true);
            settingsScreen.SetActive(false);
            creditsScreen.SetActive(true);
        }

        public void DisplaySettings()
        {
            settingsButton.ToggleActivation(false);
            creditsButton.ToggleActivation(true);
            settingsScreen.SetActive(true);
            creditsScreen.SetActive(false);
        }

        public void SetBackgroundMusic(bool isInGame)
        {
            musicPlayer.clip = isInGame ? inGameMusic : menuMusic;
            musicPlayer.Play();
        }
    }
}