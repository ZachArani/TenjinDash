using Assets.Scripts.UI;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Assets.Scripts.FSM.States
{
    public class RaceState : MonoBehaviour
    {



        /// <summary>
        /// Current newStandings in the race. Returns an List of the players based on their current position on their runningTrack.
        /// </summary>
        [ReadOnly]
        public List<NewMovement> standings;

        public NewMovement firstPlace { get => standings[0]; }

        //How far a player has to be in order to get the max RB boost. 30 means a player 30 units away gets 100% of the boost, while 15 away would give 50% etc.
        [Range(5f, 50f)]
        public float maxRubberbandDistance = 30f;

        //How much extra boost we'll give for RB
        [Range(0f, 20f)]
        public float rubberbandBoostMax = 10f;

        [Range(0, 2f)]
        public float standingsUpdateThreshold = 0.5f;

        [Range(0, 1f)]
        public float slipstreamMax = 0.5f;

        [Range(0.00001f, 0.001f)]
        public float slipstreamInc = 0.001f;

        [Range(0, 5f)]
        public float loserBoostWait = 2f;
        [Range(0, 5f)]
        public float winnerBoostWait = 2f;

        public float PreMoveTime = 3f;

        float pathLength;

        public bool p1Won;

        [SerializeField]
        float finishLinePos;

        public List<TextAsset> autoFiles;

        IEnumerator loserBoost;
        IEnumerator winnerBoost;

        public AnimationCurve preMoveCurve;


        public float percentDone
        {
            get => standings[0].GetComponent<CinemachineDollyCart>().m_Position / pathLength;
        }

        public float maxSpeed
        {
            get => 12f + 8 * percentDone;
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
            standings = StateManager.instance.players;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateStandings();
            UpdateStandingUI();

            //A little dirty (uses GetComponent in Update) TODO: Improve system
            if(firstPlace.transform.position.x >= finishLinePos && StateManager.instance.currentState == GAME_STATE.RACE)
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
                standings.ForEach(p => { p.enabled = true; p.isPreMove = true; p.GetComponent<Animator>().SetTrigger("startRace"); });
                StartCoroutine(PreMove());
                StateManager.instance.EnableRaceComponents(true);
                
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
            p1Won = firstPlace.playerNum == 0 ? true : false;

            StateManager.instance.EnableRaceComponents(false);
            StateManager.instance.EnableRecorders(false);
            StateManager.instance.TransitionTo(GAME_STATE.PHOTO_FINISH);
        }

        /// <summary>
        /// Updates the UI elements for current newStandings
        /// </summary>
        void UpdateStandingUI()
        {
            StateManager.instance.players.ForEach(p =>
            {
                var pos = standings.FindIndex(i => i == p);
                playerPosUI[StateManager.instance.players.FindIndex(i => i == p)].text = _playerPosText[pos];
            });
        }

        void UpdateStandings()
        {
            var newStandings = StateManager.instance.players.OrderByDescending(p => p.GetComponent<CinemachineDollyCart>().m_Position).ToList();
            if (newStandings[0] != firstPlace)
            {
                if (newStandings[0].GetComponent<CinemachineDollyCart>().m_Position - newStandings[1].GetComponent<CinemachineDollyCart>().m_Position > standingsUpdateThreshold)
                {
                    StopCoroutine(loserBoost);
                    StopCoroutine(winnerBoost);

                    standings = newStandings;

                    loserBoost = AddLoserBoost();
                    winnerBoost = RemoveWinnerBoost();

                    StartCoroutine(loserBoost);
                    StartCoroutine(winnerBoost);

                    Debug.Log("Updated Standings!");
                }
                
            }
        }

        public IEnumerator PreMove()
        {
            yield return new WaitForSeconds(1);
            StateManager.instance.stateDictionary[GAME_STATE.COUNTDOWN].GetComponent<CountdownState>().countdownGUI.enabled = false;
            yield return new WaitForSeconds(PreMoveTime-1);
            Debug.Log("Exiting Premove");
            loserBoost = AddLoserBoost();
            winnerBoost = RemoveWinnerBoost();
            StartCoroutine(loserBoost);
            StartCoroutine(winnerBoost);
            standings.ForEach(p =>
            {
                p.isPreMove = false;
            });
        }

        IEnumerator AddLoserBoost()
        {
            yield return new WaitForSeconds(loserBoostWait);
            Debug.Log($"Boosting Loser: {standings[1].name}");
            while (standings[1].slipstream + slipstreamInc <= slipstreamMax)
            {
                standings[1].slipstream += slipstreamInc;
                yield return null;
            }
            Debug.Log($"FINISHED boosting {standings[1].name}");
        }

        IEnumerator RemoveWinnerBoost()
        {
            yield return new WaitForSeconds(winnerBoostWait);
            Debug.Log($"Deboosting Winner: {standings[0].name}");
            while (standings[0].slipstream - slipstreamInc >= 0)
            {
                standings[0].slipstream -= slipstreamInc;
                yield return null;
            }
            Debug.Log($"FINISHED deboosting {standings[0].name}");
        }

        private void OnEnable()
        {
            StateManager.onGameStateChanged += startRace;
        }

        private void OnDisable()
        {
            StateManager.onGameStateChanged -= startRace;
        }

        /// <summary>
        /// Picks a random auto file and removes from the list (so both runners don't use the same file)
        /// </summary>
        /// <returns></returns>
        public TextAsset PickAutoFile()
        {
            var file = autoFiles[Random.Range(0, autoFiles.Count - 1)];
            autoFiles.Remove(file);
            return file;
        }


    }
}