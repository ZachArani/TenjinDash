using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Singleton for managing UI actions.
    /// </summary>
    public class UIManager : MonoBehaviour
    {

        /// <summary>
        /// UIManager singleton
        /// </summary>
        public static UIManager instance { get; private set; }

        //Collection of start menu UI elements 
        [SerializeField] GameObject startMenu;
        [SerializeField] GameObject finishMenu;
        [SerializeField] GameObject raceUI;
        [SerializeField] GameObject countdown;
        [SerializeField] GameObject screenSplitter;

        /// <summary>
        /// Used to ensure singleton is properly loaded. Either creates one instance or kills any other instance.
        /// </summary>
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
            DontDestroyOnLoad(this);
        }

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void toggleStartUI(bool toggle)
        {
            startMenu.SetActive(toggle);
        }

        public void toggleFinishUI(bool toggle) 
        {
            finishMenu.SetActive(toggle); 
        }

        public void toggleRaceUI(bool toggle)
        {
            raceUI.SetActive(toggle);
        }

        public void toggleCountdownUI(bool toggle)
        {
            countdown.SetActive(toggle);
        }

        public void toggleScreenSplitter(bool toggle)
        {
            screenSplitter.SetActive(toggle); 
        }

        private void OnApplicationQuit()
        {
            Debug.Log("Stopping operation");
            toggleStartUI(false);
            toggleFinishUI(false);
            toggleRaceUI(false);
            toggleCountdownUI(false);
            toggleScreenSplitter(false);
        }
    }
}