using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.Scripts.FSM.States
{
    public class CountdownState : MonoBehaviour
    {
        /// <summary>
        /// UI element for countdown graphics
        /// </summary>
        public TextMeshProUGUI countdownGUI;
        /// <summary>
        /// Timeline that controls the countdown
        /// </summary>
        public PlayableDirector countdownTimeline;

        /// <summary>
        /// Current number in the countdown
        /// </summary>
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
                if (StateManager.instance.skipPreroll) //We need to enable the player cameras manually if we skipped Preroll.
                {
                    StateManager.instance.EnableRaceCameras();
                    Cursor.visible = false;
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
            else if(countdown == -2) //We've past the countdown time, hide from screen and transition to next state.
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