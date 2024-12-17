using System.Collections;
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
        [SerializeField] GameObject speedMeters;

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

        public void ToggleStartUI(bool toggle)
        {
            startMenu.SetActive(toggle);
        }

        public void ToggleFinishUI(bool toggle)
        {
            finishMenu.SetActive(toggle);
        }

        public void ToggleRaceUI(bool toggle)
        {
            raceUI.SetActive(toggle);
        }

        public void ToggleCountdownUI(bool toggle)
        {
            countdown.SetActive(toggle);
        }

        public void ToggleFinishText(bool toggle)
        {
            finishText.SetActive(toggle);
        }

        public void ToggleScreenSplitter(bool toggle)
        {
            screenSplitter.SetActive(toggle);
        }

        /// <summary>
        /// Toggle Speed Meter visibility
        /// </summary>
        /// <param name="toggle"></param>
        public void ToggleSpeedMeters(bool toggle)
        {
            foreach(Transform t in speedMeters.transform)
            {
                t.GetChild(0).GetComponent<SpeedMeter>().enabled = toggle;
            }

        }

        /// <summary>
        /// Clean up after restart
        /// </summary>
        public void CleanUp()
        {
            foreach (Transform t in speedMeters.transform)
            {
                t.GetChild(0).GetComponent<SpeedMeter>().CleanUp();
            }
        }

        private void OnApplicationQuit()
        {
            Debug.Log("Stopping operation");
            ToggleStartUI(false);
            ToggleFinishUI(false);
            ToggleRaceUI(false);
            ToggleCountdownUI(false);
            ToggleScreenSplitter(false);
            ToggleFinishText(false);
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