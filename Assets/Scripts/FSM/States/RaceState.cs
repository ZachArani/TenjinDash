using Assets.Scripts.UI;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        float raceCloseness = 1f;

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
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

                StateManager.instance.EnableRaceComponents(true);
                
                if (Options.instance.isAuto) //If auto mode
                {
                    StateManager.instance.players.ForEach(p => p.isAuto = true);
                }
                else if(Options.instance.isSolo) //If 1 player
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

                if(Options.instance.recordPlayerData)
                {
                    StateManager.instance.players.ForEach(p => { p.GetComponent<InputRecorder>().enabled = true;});
                }
            }
        }

        /// <summary>
        /// Ends the race and switches to the Photo Finish mode.
        /// </summary>
        void endRace()
        {
            StateManager.instance.EnableRaceComponents(false);
            StateManager.instance.EnableRecorders(false);
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