using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.Scripts.FSM.States
{
    public class PrerollState : MonoBehaviour
    {
        public PlayableDirector preRollTimeline;


        void Start()
        {

        }

        void Update()
        {

        }


        private void StartPreRoll(GAME_STATE from, GAME_STATE to)
        {
            if (to == GAME_STATE.PREROLL)
            {

                Cursor.visible = false;
                preRollTimeline.Play();  
            }
        }

        public void FinishedPreRoll()
        {
            Debug.Log("FINISHED PREROLL!");
            if(!StateManager.instance.options.skipCountdown)
            {
                StateManager.instance.TransitionTo(GAME_STATE.COUNTDOWN);
            }
            else
            {
                StateManager.instance.TransitionTo(GAME_STATE.RACE);
            }
        }

        private void OnEnable()
        {
            StateManager.onGameStateChanged += StartPreRoll;
        }

        private void OnDisable()
        {
            StateManager.onGameStateChanged -= StartPreRoll;
        }
    }
}