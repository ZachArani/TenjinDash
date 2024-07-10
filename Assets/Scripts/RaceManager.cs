using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Scripts
{
    public class RaceManager : MonoBehaviour
    {
        [field: SerializeField, ReadOnly] public bool raceStarted {get; private set;}
        [field: ReadOnly] public bool raceFinished { get; private set; }
        [field: SerializeField, ReadOnly] public GameObject[] standings {get; private set;}
        public GameObject[] players;
        public CountdownHandler countdown;



        void Start()
        {
            standings = players;
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
            countdown.count = 3;
        }

        public void FinishCountdown()
        {
            //TODO: Start game
        }

    }
}