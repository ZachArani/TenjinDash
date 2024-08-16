using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.Scripts.FSM.States
{
    public class PrerollState : MonoBehaviour
    {
        public PlayableDirector preRollTimeline;


        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
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
            StateManager.instance.TransitionTo(GAME_STATE.COUNTDOWN);
        }

        private void OnEnable()
        {
            StateManager.onGameStateChanged += StartPreRoll;
            //preRollTimeline.
        }

        private void OnDisable()
        {
            StateManager.onGameStateChanged -= StartPreRoll;
        }
    }
}