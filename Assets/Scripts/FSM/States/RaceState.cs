using Assets.Scripts.UI;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Assets.Scripts.FSM.States
{
    public class RaceState : MonoBehaviour
    {



        /// <summary>
        /// Current standings in the race. Returns an List of the players based on their current position on their track.
        /// </summary>
        public List<NewMovement> playerPos 
        {
            get => StateManager.instance.players.OrderByDescending(p => p.GetComponent<CinemachineDollyCart>().m_Position).ToList();
        }

        /// <summary>
        /// List of all UI elements for player positions. Index 0 == P1, Index 1 == P2, etc.
        /// </summary>
        [SerializeField]
        private List<TextMeshProUGUI> _playerPosUI;
        public List<TextMeshProUGUI> playerPosUI { get { return _playerPosUI; } private set { _playerPosUI = value; } }

        [SerializeField]
        private List<string> _playerPosText;
        public List<string> playerPosText { get { return _playerPosText; } private set { _playerPosText = value; } } 

        
        //Margin of error when calcuating player positions.
        //If there is no MOE, then the 1st/2nd place UI flashes a *ton* when its a close race.
        [SerializeField]
        [Range(0, 2f)]
        float playerPositionMOE;

        /// <summary>
        /// Used during demo modes to "fake" running speed. Decides how often (in terms of distance) to randomly roll a new speed for the player.
        /// </summary>
        [SerializeField]
        [Range(10f, 100f)]
        float fakeItDistance = 15f;



        [SerializeField]
        [Range(0f, 3f)]
        float posDistanceMOE = 0.5f;


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
        }

        // Update is called once per frame
        void Update()
        {
            /* if(inFirst != player1 && (player1Pos.m_Position + playerPositionMOE) - (player2Pos.m_Position + player2Moe) > posDistanceMOE) //IF P1 ahead of P2
             {
                 inFirst = player1.gameObject;
                 player1.losingSpeedBoost = 0f;
                 player2.losingSpeedBoost = losingSpeedBoost;
                 updateStandingUI();
             }
             else if(inFirst != player2 && (player2Pos.m_Position + player2Moe) - (player1Pos.m_Position + playerPositionMOE) > posDistanceMOE) //If P2 ahead of P1
             {
                 inFirst = player2.gameObject;
                 player2.losingSpeedBoost = 0f;
                 player1.GetComponent<NewMovement>().losingSpeedBoost = losingSpeedBoost;
                 updateStandingUI();
             }

            if(enforceCloseRace) //If we're enforcing a close race.
            {
                if(Mathf.Abs(player1._desiredSpeed - player2._desiredSpeed) > raceCloseness) //If one player is too fast or slow
                {
                    Debug.Log("Players too far apart! Enforcing close race!");
                    float avgSpeed = (player1._desiredSpeed + player2._desiredSpeed) / 2; 
                    player1._desiredSpeed = (inFirst == player1.gameObject) ? //Pushes both players in the same speed range based on their combined average speed.
                        Mathf.Clamp(player1._desiredSpeed, player1._desiredSpeed - raceCloseness, player1._desiredSpeed) :
                        Mathf.Clamp(player1._desiredSpeed, player2._desiredSpeed - raceCloseness, player2._desiredSpeed);
                    player2._desiredSpeed = (inFirst == player2.gameObject) ?
                        Mathf.Clamp(player2._desiredSpeed, player2._desiredSpeed - raceCloseness, player2._desiredSpeed) :
                        Mathf.Clamp(player2._desiredSpeed, player1._desiredSpeed - raceCloseness, player1._desiredSpeed);
                }
            } */
            updateStandingUI();
            if(Options.instance.isAuto)
            {

            }

            //A little dirty (uses GetComponent in Update) TODO: Improve system
            if(playerPos.First().transform.position.x >= StateManager.instance.finishLinePos && StateManager.instance.currentState == GAME_STATE.RACE)
            {
                StateManager.instance.TransitionTo(GAME_STATE.PHOTO_FINISH);
            }
        }

        void startRace(GAME_STATE from, GAME_STATE to)
        {
            if(to == GAME_STATE.RACE)
            {
                if (Options.instance.skipCountdown) //We need to enable the player cameras manually if we skipped countdown.
                {
                    StateManager.instance.EnableRaceCameras();
                    UIManager.instance.toggleRaceUI(true);
                    UIManager.instance.toggleScreenSplitter(false);
                    Cursor.visible = false;
                }

                StateManager.instance.EnableRaceComponents();
                
                if (Options.instance.isAuto) //If auto mode
                {
                    StateManager.instance.players.ForEach(p => p.isAuto = true);
                }
                else if(Options.instance.isAuto) //If 1 player
                {
                    var players = StateManager.instance.players.OrderByDescending(p => p.playerNum);
                    players.First().isAuto = false; //First player is playing
                    foreach(NewMovement player in players.Skip(1))
                    {
                        player.isAuto = true; //The rest are auto
                    }
                }
                else
                {
                    StateManager.instance.players.ForEach(p => p.isAuto = false); //Everyone is playing
                }
            }
        }

        /// <summary>
        /// Ends the race and switches to the Photo Finish mode.
        /// </summary>
        void endRace()
        {
            StateManager.instance.DisableRaceComponents();
            StateManager.instance.TransitionTo(GAME_STATE.PHOTO_FINISH);
        }

        /// <summary>
        /// Updates the UI elements for current standings
        /// </summary>
        void updateStandingUI()
        {
            StateManager.instance.players.ForEach(p =>
            {
                var pos = playerPos.FindIndex(i => i == p);
                playerPosUI[StateManager.instance.players.FindIndex(i => i == p)].text = _playerPosText[pos];
            });
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