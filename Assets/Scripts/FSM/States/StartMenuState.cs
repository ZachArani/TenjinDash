﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.Playables;
using Assets.Scripts.UI;

namespace Assets.Scripts
{

    /// <summary>
    /// FSM state for the game's start menu
    /// </summary>
    public class StartMenuState : MonoBehaviour
    {

        /// <summary>
        /// Timeline that controls the start menu cutscene
        /// </summary>
        public PlayableDirector startMenuTimeline;

        void Start()
        {
          
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Method called when FSM transitions to this state
        /// Updates the start menu's visibility
        /// If we are transitioning *to* StartMenu, then we show the UI elements
        /// Else if we are transitioning *away* from StartMenu, then we hide the UI elements
        /// </summary>
        /// <param name="from">State we transition from</param>
        /// <param name="to">State we transition to</param>
        public void UpdateMenuVisibility(GAME_STATE from, GAME_STATE to)
        {
            if (to == GAME_STATE.START_MENU)
            {
                startMenuTimeline.Play();
                UIManager.instance.toggleStartUI(true);
                Cursor.visible = true;
            }
            else if(from == GAME_STATE.START_MENU) 
            {
                UIManager.instance.toggleStartUI(false);
                Cursor.visible = false;
            }
            
        }

        /// <summary>
        /// Called when Auto mode is selected.
        /// Adds 'auto' flag to context.
        /// </summary>
        public void OnAutoSelect()
        {
            StateManager.instance.options.isAuto = true;
            StateManager.instance.TransitionTo(GAME_STATE.PREROLL);
        }

        /// <summary>
        /// Called when Solo mode is selected. 
        /// Adds 'solo' flag to context
        /// </summary>
        public void OnSoloSelect()
        {
            StateManager.instance.options.isSolo = true;
            StateManager.instance.TransitionTo(GAME_STATE.PREROLL);
        }

        /// <summary>
        /// Called when start button is selected. Chooses next state based on options.
        /// </summary>
        public void OnStartSelect()
        {
            if(!StateManager.instance.options.skipPreroll)
            {
                StateManager.instance.TransitionTo(GAME_STATE.PREROLL);
            }
            else
            {
                if (StateManager.instance.options.skipCountdown)
                {
                    StateManager.instance.TransitionTo(GAME_STATE.RACE);
                }
                StateManager.instance.TransitionTo(GAME_STATE.COUNTDOWN);
            }   
        }

        private void OnEnable()
        {
            StateManager.onGameStateChanged += UpdateMenuVisibility;
        }

        private void OnDisable()
        {

            StateManager.onGameStateChanged -= UpdateMenuVisibility;
        }

    }

}