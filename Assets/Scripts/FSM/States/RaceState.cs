using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.FSM.States
{
    public class RaceState : MonoBehaviour
    {

        public MovementHandler player1;
        public MovementHandler player2;

        bool inRace;

        private GameObject standings;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
        }

        void startRace(GAME_STATE from, GAME_STATE to)
        {
            if(to == GAME_STATE.RACE)
            {
                inRace = true;
                player1.GetComponent<NewMovement>().enabled = true;
                //player2.GetComponent<NewMovement>().enabled = true;
            }
        }

        private void OnEnable()
        {
            StateManager.onGameStateChanged += startRace;
        }

        private void OnDisable()
        {
            StateManager.onGameStateChanged -= startRace;
        }


    }
}