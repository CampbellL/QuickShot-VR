using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class ScreenFade : MonoBehaviour
    {
        public static ScreenFade Instance;

        private Animator _animator;
        private int _levelToLoad;
        private static readonly int FadeOut = Animator.StringToHash("FadeOut");

        private void Awake()
        {
            if (Instance)
                Destroy(this);
            else
                Instance = this;

            _animator = GetComponent<Animator>();
        }
        
        public void FadeToLevel(int levelIndex)
        {
            //Start the Fade-Out animation
            _animator.SetTrigger(FadeOut);
            _levelToLoad = levelIndex;
        }
        
        //Called as an animation event at the end of the FadeOut animation
        public void OnFadeComplete()
        {
            SceneManager.LoadScene(_levelToLoad);
        }
    }
}