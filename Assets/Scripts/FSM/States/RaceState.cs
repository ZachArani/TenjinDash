using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.FSM.States
{
    public class RaceState : MonoBehaviour
    {
        //Player movement objects
        [SerializeField]
        NewMovement player1;
        [SerializeField]
        NewMovement player2;

        //Player UI elements 
        [SerializeField]
        TextMeshProUGUI player1PosUI;
        [SerializeField] 
        TextMeshProUGUI player2PosUI;
        [SerializeField]
        [TextArea(1,1)]
        public string firstPlaceText = "1st";
        [SerializeField]
        [TextArea(1,1)]
        public string secondPlaceText = "2nd";

        //Player dolly carts
        CinemachineDollyCart player1Pos;
        CinemachineDollyCart player2Pos;
        
        //Margin of error when calcuating player positions.
        //If there is no MOE, then the 1st/2nd place UI flashes a *ton* when its a close race.
        [SerializeField]
        [Range(0, 2f)]
        float player1Moe;
        [SerializeField]
        [Range(0, 2f)]
        float player2Moe;

        /// <summary>
        /// Used during demo modes to "fake" running speed. Decides how often (in terms of distance) to randomly roll a new speed for the player.
        /// </summary>
        [SerializeField]
        [Range(10f, 100f)]
        float fakeItDistance = 15f;


        bool inRace;

        /// <summary>
        /// Utility object to reference who is currently in first place.
        /// </summary>
        GameObject inFirst;


        [SerializeField]
        [Range(0f, 3f)]
        float posDistanceMOE = 0.5f;

        /// <summary>
        /// Decides how much of a speed boost to give to the 2nd place runner. 
        /// </summary>
        [SerializeField]
        [Range(0f, 2f)]
        float losingSpeedBoost = 0.3f;

        /// <summary>
        /// Basically enforces "rubberbanding" behavior. 
        /// If a player falls too far behind, the game will give them a massive speed boost so they can catch up with the other runner.
        /// </summary>
        [SerializeField]
        bool enforceCloseRace = false;

        /// <summary>
        /// Indicates the maximum difference between player speeds. If someone is too far behind or ahead, the game will force them into this speed range.
        /// Used alongside enforceCloseRace to keep the race close. 
        /// </summary>
        [SerializeField]
        float raceCloseness = 1f;

        // Use this for initialization
        void Start()
        {
            player1Pos = player1.GetComponent<CinemachineDollyCart>();
            player2Pos = player2.GetComponent<CinemachineDollyCart>();
        }

        // Update is called once per frame
        void Update()
        {
            if(inFirst != player1 && (player1Pos.m_Position + player1Moe) - (player2Pos.m_Position + player2Moe) > posDistanceMOE) //IF P1 ahead of P2
            {
                inFirst = player1.gameObject;
                player1.losingSpeedBoost = 0f;
                player2.losingSpeedBoost = losingSpeedBoost;
                updateStandingUI();
            }
            else if(inFirst != player2 && (player2Pos.m_Position + player2Moe) - (player1Pos.m_Position + player1Moe) > posDistanceMOE) //If P2 ahead of P1
            {
                inFirst = player2.gameObject;
                player2.losingSpeedBoost = 0f;
                player1.GetComponent<NewMovement>().losingSpeedBoost = losingSpeedBoost;
                updateStandingUI();
            }

            if(enforceCloseRace) //If we're enforcing a close race.
            {
                if(Mathf.Abs(player1.desiredSpeed - player2.desiredSpeed) > raceCloseness) //If one player is too fast or slow
                {
                    Debug.Log("Players too far apart! Enforcing close race!");
                    float avgSpeed = (player1.desiredSpeed + player2.desiredSpeed) / 2; 
                    player1.desiredSpeed = (inFirst == player1.gameObject) ? //Pushes both players in the same speed range based on their combined average speed.
                        Mathf.Clamp(player1.desiredSpeed, player1.desiredSpeed - raceCloseness, player1.desiredSpeed) :
                        Mathf.Clamp(player1.desiredSpeed, player2.desiredSpeed - raceCloseness, player2.desiredSpeed);
                    player2.desiredSpeed = (inFirst == player2.gameObject) ?
                        Mathf.Clamp(player2.desiredSpeed, player2.desiredSpeed - raceCloseness, player2.desiredSpeed) :
                        Mathf.Clamp(player2.desiredSpeed, player1.desiredSpeed - raceCloseness, player1.desiredSpeed);
                }
            }
        }

        void startRace(GAME_STATE from, GAME_STATE to)
        {
            if(to == GAME_STATE.RACE)
            {
                inRace = true;
                player1.GetComponent<NewMovement>().enabled = true;
                player2.GetComponent<NewMovement>().enabled = true;
                if (StateManager.instance.contexts.Contains(GAME_CONTEXTS.AUTO)) //If auto mode
                {
                }
                else if(StateManager.instance.contexts.Contains(GAME_CONTEXTS.SOLO)) //If 1 player
                {
                }
                else //If 2 player
                {

                }
            }
        }

        void updateStandingUI()
        {
            if(inFirst == player1)
            {
                player1PosUI.text = firstPlaceText;
                player2PosUI.text = secondPlaceText;
            }
            else if(inFirst == player2)
            {
                player1PosUI.text = secondPlaceText;
                player2PosUI.text = firstPlaceText;
            }
            //Debug.Log($"{inFirst}");
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