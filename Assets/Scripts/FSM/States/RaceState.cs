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

        public NewMovement firstPlace
        {
            get => playerPos[0];
        }

        [Range(5f, 50f)]
        public float maxRubberbandDistance = 30f;

        [Range(0f, 20f)]
        public float maxRubberbandBoost = 10f;

        float pathLength;

        public float percentDone
        {
            get => playerPos[0].GetComponent<CinemachineDollyCart>().m_Position / pathLength;
        }

        public float speedMax
        {
            get => 600f + 400f * percentDone;
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



        // Use this for initialization
        void Start()
        {
            pathLength = StateManager.instance.players[0].GetComponent<CinemachineDollyCart>().m_Path.PathLength;
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
                
                if (Options.instance.isAuto) 
                {
                    StateManager.instance.players.ForEach(p => p.isAuto = true);
                }
                else if(Options.instance.isSolo)
                {
                    var players = StateManager.instance.players.OrderByDescending(p => p.playerNum);
                    players.First().isAuto = false;
                    foreach(NewMovement player in players.Skip(1))
                    {
                        player.isAuto = true;
                    }
                }
                else
                {
                    StateManager.instance.players.ForEach(p => p.isAuto = false); 
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