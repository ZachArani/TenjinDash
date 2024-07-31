﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.Playables;

namespace Assets.Scripts
{


    public class StartMenuState : MonoBehaviour
    {


        public PlayableDirector startMenuTimeline;

        public TextMeshProUGUI auto;
        public TextMeshProUGUI logo;
        public TextMeshProUGUI start;
        public GameObject JoyCons;
        public GameObject credits;


        List<TextMeshProUGUI> textObj = new List<TextMeshProUGUI>();


        void Start()
        {

            textObj = new List<TextMeshProUGUI>{ auto, logo, start };
            credits.GetComponentsInChildren<TextMeshProUGUI>().ToList().ForEach(c => textObj.Add(c));
          
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateMenuVisibility(GAME_STATE from, GAME_STATE to)
        {
            if (to == GAME_STATE.START_MENU)
            {
                startMenuTimeline.Play();
                textObj.ForEach(text => text.enabled = true);
                JoyCons.GetComponentsInChildren<Image>().ToList().ForEach(joycon => joycon.enabled = true);
            }
            else if(from == GAME_STATE.START_MENU) 
            {
                textObj.ForEach(text => text.enabled = false);
                JoyCons.GetComponentsInChildren<Image>().ToList().ForEach(joycon => joycon.enabled = false);
            }
            
        }

        public void OnAutoSelect()
        {
            StateManager.instance.contexts.Add(GAME_CONTEXTS.AUTO);
            StateManager.instance.TransitionTo(GAME_STATE.PREROLL);
        }

        public void OnSoloSelect()
        {
            StateManager.instance.contexts.Add(GAME_CONTEXTS.SOLO);
            StateManager.instance.TransitionTo(GAME_STATE.PREROLL);
        }

        public void OnStartSelect()
        {
            if(!StateManager.instance.skipPreroll)
            {
                StateManager.instance.TransitionTo(GAME_STATE.PREROLL);
            }
            else
            {
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