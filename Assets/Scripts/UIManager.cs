using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace Assets.Scripts
{


    public class UIManager : MonoBehaviour
    {
        public static event Action OnStartMenuExit;
        public static event Action OnStartMenuEnter;

        public static event Action OnEndMenuExit;
        public static event Action OnEndMenuEnter;

        [SerializeField] public TextMeshProUGUI countdown;
        [SerializeField] public TextMeshProUGUI finish;
        [SerializeField] public TextMeshProUGUI menu;
        [SerializeField] public TextMeshProUGUI auto;
        [SerializeField] public TextMeshProUGUI logo;
        [SerializeField] public TextMeshProUGUI restart;
        [SerializeField] public TextMeshProUGUI start;
        [SerializeField] public RaceManager raceManager;
        public GameObject JoyCons;
        public GameObject credits;


        List<TextMeshProUGUI> textObj = new List<TextMeshProUGUI>();


        void Start()
        {

            textObj = new List<TextMeshProUGUI>{ countdown, finish, menu, auto, logo, restart, start };
            credits.GetComponentsInChildren<TextMeshProUGUI>().ToList().ForEach(c => textObj.Add(c));
          
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ReceiveTimelineSignal() //Look, I know this is stupid but blame Unity's fucking awful signals.
        {
            OnStartMenuEnter.Invoke();
        }

        public void ShowMainMenu()
        {
            textObj.Where(text => text != finish && text != restart && text != menu).ToList().ForEach(text => text.enabled = true);
            //textObj.ForEach(text => text.enabled = true);
            JoyCons.GetComponentsInChildren<Image>().ToList().ForEach(joycon => joycon.enabled = true);
        }

        public void HideMainMenu()
        {
            textObj.ForEach(text => text.enabled = false);
            JoyCons.GetComponentsInChildren<Image>().ToList().ForEach(joycon => joycon.enabled = false);
        }

        public void OnAutoSelect()
        {
            OnStartMenuExit.Invoke();
            raceManager.StartRace();
        }

        public void OnStartSelect()
        {
            OnStartMenuExit.Invoke();
            raceManager.StartRace();
        }

        public void ShowCountdown() { countdown.enabled = true; countdown.text = raceManager.countdown.ToString(); }

        public void HideCountdown() { countdown.enabled = false; }

        public void UpdateCountdown() { countdown.text = raceManager.countdown.ToString(); }

        public void FinishCountdown() { countdown.text = "GO!"; }

        private void OnEnable()
        {

            OnStartMenuExit += HideMainMenu;
            OnStartMenuEnter += ShowMainMenu;
            RaceManager.OnCountdownStart += ShowCountdown;
            RaceManager.OnCountdownUpdate += UpdateCountdown;
            RaceManager.OnCountdownEnd += FinishCountdown;
            RaceManager.OnCountdownHidden += HideCountdown;
        }

        private void OnDisable()
        {

            OnStartMenuExit -= HideMainMenu;
            OnStartMenuEnter -= ShowMainMenu;
            RaceManager.OnCountdownStart -= ShowCountdown;
            RaceManager.OnCountdownUpdate -= UpdateCountdown;
            RaceManager.OnCountdownEnd -= FinishCountdown;
            RaceManager.OnCountdownHidden -= HideCountdown;
        }

    }

}