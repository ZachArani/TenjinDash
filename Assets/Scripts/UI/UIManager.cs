using System.Collections;
using UnityEditor.Profiling.Memory.Experimental;
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
        [SerializeField] GameObject finishText;
        [SerializeField] GameObject joyCons;

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

        public void toggleFinishText(bool toggle)
        {
            finishText.SetActive(toggle);
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
            toggleFinishText(false);
        }

        /// <summary>
        /// Checks how many joycons are connected every three seconds.
        /// Updates JoyCon UI element on start menu.
        /// Needs to be started and stopped as a Unity Coroutine.
        /// </summary>
        /// <returns>IEnumerator for Unity Coroutine use</returns>
        public void UpdateJoyConStatus()
        {
            Debug.Log($"Checking Joycon Status! {JoyconManager.Instance.j.Count} connected!");
            switch (JoyconManager.Instance.j.Count)
            {
                case 1: //One joycon connected
                    joyCons.transform.GetChild(0).gameObject.SetActive(true);
                    joyCons.transform.GetChild(1).gameObject.SetActive(false);
                    break;
                case 0: //No joycons
                    joyCons.transform.GetChild(0).gameObject.SetActive(false);
                    joyCons.transform.GetChild(1).gameObject.SetActive(false);
                    break;
                default: //In 2 or more:
                    joyCons.transform.GetChild(0).gameObject.SetActive(true);
                    joyCons.transform.GetChild(1).gameObject.SetActive(true);
                    break;
            }
        }
    }
}