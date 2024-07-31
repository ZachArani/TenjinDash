using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Assets.Scripts
{

    public enum CountdownStates
    {
        START,
        UPDATE,
        END,
        HIDDEN
    }

    public class RaceManager : MonoBehaviour
    {


        public static event Action<CountdownStates> OnCountdownEvent;



        [field: SerializeField, ReadOnly] public bool raceStarted {get; private set;}
        [field: ReadOnly] public bool raceFinished { get; private set; }
        [field: SerializeField, ReadOnly] public GameObject[] standings {get; private set;}
        public GameObject[] players;
        [field: SerializeField] public List<Camera> playerCams;
        public int countdown;

        [field: SerializeField] public PlayableDirector preRoll;

        private bool isAuto;

        Action<GAME_STATE, GAME_STATE> menuHandler;

        void Start()
        {
            standings = players;

            menuHandler = (from, to) =>
            {
                if (from.Equals(GAME_STATE.FINISH_MENU) && to.Equals(GAME_STATE.NONE)) //We're restarting the race (not going to a menu)
                {
                    StartRace();
                }
                else if (to.Equals(GAME_STATE.START_MENU))
                {
                    DisableRaceCameras();
                }
            };
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Updates the race standings by moving a specified player either up or down the standing list
        /// </summary>
        /// <param name="player">Player to be moved up or down</param>
        /// <param name="movingUp">Indicates if the player is going up or down the standing ranks</param>
        public void updateStanding(GameObject player, bool movingUp)
        {
            if(standings.Contains(player))
            {
                int playerPos = Array.IndexOf(standings, player); 
                if(movingUp && !standings.First().Equals(player)) //If we can move up
                {
                    var temp = standings[playerPos - 1]; 
                    standings[playerPos - 1] = player; //Swap standings
                    standings[playerPos] = temp;
                }
                else if(!movingUp && !standings.Last().Equals(player)) //If we can move down
                {
                    var temp = standings[playerPos + 1]; 
                    standings[playerPos + 1] = player; //Swap standings
                    standings[playerPos] = temp;
                }
            }
        }

        public void StartCountdown()
        {
            //TODO: Countdown related stuff
            Debug.Log("Start countdown called");
            countdown = 3;
            OnCountdownEvent.Invoke(CountdownStates.START);
        }

        public void UpdateCountdown()
        {
            Debug.Log("Updating countdown");
            countdown--;
            OnCountdownEvent.Invoke(CountdownStates.UPDATE); 
            if(countdown == 0)
            {
                OnCountdownEvent.Invoke(CountdownStates.END);
            }
        }

        public void FinishCountdown()
        {
            //TODO: Start game
        }

        public void HideCountdown()
        {
            OnCountdownEvent.Invoke(CountdownStates.HIDDEN);
        }

        public void EnableRaceCameras()
        {
            playerCams.ForEach(c => c.enabled = true);
        }

        public void DisableRaceCameras()
        {
            Debug.Log("Disabling Race Cameras.");
            playerCams.ForEach(c => c.enabled = false);
        }

        public void StartRace()
        {
            preRoll.Play();
        }

        public void ResetRace()
        {

        }

        public void EndRace()
        {
            isAuto = false;
        }

        private void OnEnable()
        {

            UIManager.OnMenuChange += menuHandler;

            UIManager.OnEndMenuExit += StartRace;
            UIManager.OnStartMenuEnter += DisableRaceCameras;

        }

        private void OnDisable()
        {

            UIManager.OnMenuChange -= menuHandler;

            UIManager.OnEndMenuExit -= StartRace;
            UIManager.OnStartMenuEnter -= DisableRaceCameras;

        }

        private void SetAuto()
        {
            isAuto = true;
        }




    }
}