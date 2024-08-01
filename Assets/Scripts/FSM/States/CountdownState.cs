using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.Scripts.FSM.States
{
    public class CountdownState : MonoBehaviour
    {

        public TextMeshProUGUI countdownGUI;
        public PlayableDirector countdownTimeline;

        int countdown;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void StartCountdown(GAME_STATE from, GAME_STATE to)
        {
            if(to == GAME_STATE.COUNTDOWN)
            {
                countdown = 3;
                if (StateManager.instance.skipPreroll)
                {
                    StateManager.instance.EnableRaceCameras();
                }
                countdownTimeline.Play();
            }
        }

        public void UpdateCountdown()
        {
            countdownGUI.enabled = true;
            countdownGUI.text = countdown.ToString();
            countdown--;
            if (countdown == -1)
            {
                countdownGUI.text = "GO!";
            }
            else if(countdown == -2)
            {
                countdownGUI.enabled = false;
                StateManager.instance.TransitionTo(GAME_STATE.RACE);
            }
        }

        private void OnEnable()
        {
            StateManager.onGameStateChanged += StartCountdown;
        }

        private void OnDisable()
        {
            StateManager.onGameStateChanged -= StartCountdown;
        }

    }
}