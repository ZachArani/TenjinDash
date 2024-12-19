using Assets.Scripts.UI;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.FSM.States
{
    /// <summary>
    /// FSM state for the proper in-game race.
    /// </summary>
    public class RaceState : MonoBehaviour
    {



        /// <summary>
        /// Current newStandings in the race. Returns an List of the players based on their current position on their runningTrack.
        /// </summary>
        [ReadOnly]
        public List<NewMovement> standings;

        public NewMovement firstPlace { get => standings[0]; }

        /// <summary>
        /// How far a player has to be in order to get the max RB boost. 30 means a player 30 units away gets 100% of the boost, while 15 away would give 50% etc.
        /// </summary>
        [Range(5f, 50f)]
        public float maxRubberbandDistance = 30f;

        [Range(0f, 20f)]
        public float rubberbandBoostMax = 10f;

        /// <summary>
        /// How far away players must be before we recalculate who's in first or second place
        /// </summary>
        [Range(0, 2f)]
        public float standingsUpdateThreshold = 0.5f;

        /// <summary>
        /// Maximum slipstream rotationSpeed boost
        /// </summary>
        [Range(0, 1f)]
        public float slipstreamMax = 0.5f;

        /// <summary>
        /// Maximum increase of current slipstream boost per frame
        /// </summary>
        [Range(0.00001f, 0.001f)]
        public float slipstreamInc = 0.001f;

        /// <summary>
        /// How long to wait before applying rotationSpeed boost changes AFTER players switch standings (1st becomes 2nd, etc.)
        /// </summary>
        [Range(0, 5f)]
        public float loserBoostWait = 2f;
        [Range(0, 5f)]
        public float winnerBoostWait = 2f;

        /// <summary>
        /// How long the players 'pretend' to race before we start using real player data
        /// i.e., 3 = 'fake player rotationSpeed for the first three seconds of the race'
        /// </summary>
        public float openingTime = 3f;


        float racePathLength;

        public bool p1Won;

        /// <summary>
        /// Where we define the race's finish line. Used for other calculations.
        /// </summary>
        [SerializeField]
        float finishLinePos;

        public List<TextAsset> autoRunFiles;

        IEnumerator loserBoost;
        IEnumerator winnerBoost;

        /// <summary>
        /// Curve for rotationSpeed/animations used BEFORE we start reading player data (the opening of the race)
        /// </summary>
        public AnimationCurve openingCurve;

        /// <summary>
        /// Percent of the race done (based on 1st place)
        /// </summary>
        public float percentDone
        {
            get => standings[0].GetComponent<CinemachineDollyCart>().m_Position / racePathLength;
        }

        public float maxSpeed
        {
            get => 12f + (8 * percentDone);
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
            racePathLength = StateManager.instance.players[0].GetComponent<CinemachineDollyCart>().m_Path.PathLength;
            standings = StateManager.instance.players;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateStandings();
            UpdateStandingUI();

            //A little dirty (uses GetComponent in Update) TODO: Improve system
            if (firstPlace.transform.position.x >= finishLinePos && StateManager.instance.currentState == GAME_STATE.RACE)
            {
                endRace();
            }
        }

        /// <summary>
        /// Handles event fired by <see cref="StateManager.onGameStateChanged"/>
        /// </summary>
        /// <param name="from">State we came from</param>
        /// <param name="to">State we're at (this one)</param>
        void startRace(GAME_STATE from, GAME_STATE to)
        {
            if (to == GAME_STATE.RACE)
            {
                if (Options.instance.skipCountdown) //We need to enable the player cameras manually if we skipped countdown.
                {
                    StateManager.instance.EnableRaceCameras();
                    UIManager.instance.ToggleRaceUI(true);
                    UIManager.instance.ToggleScreenSplitter(false);
                    Cursor.visible = false;
                }
                SoundManager.instance.PlayCountDownFinish();
                SoundManager.instance.PlayRaceMusic();
                UIManager.instance.ToggleSpeedMeters(true);
                standings.ForEach(p => { p.enabled = true; p.isOpening = true; p.GetComponent<Animator>().SetTrigger("startRace"); });
                StartCoroutine(OpenRace());
                StateManager.instance.EnableRaceComponents(true);

                if (Options.instance.recordPlayerData)
                {
                    StateManager.instance.players.ForEach(p => { p.GetComponent<InputRecorder>().enabled = true; });
                }
            }
        }

        /// <summary>
        /// Ends the race and switches to the Photo Finish mode.
        /// </summary>
        void endRace()
        {
            //Make final check for winner
            standings = StateManager.instance.players.OrderByDescending(p => p.transform.position.x).ToList();
            p1Won = firstPlace.playerNum == 0;

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

        /// <summary>
        /// Checks to see if we need to update the race standings. 
        /// If so, handle rotationSpeed boosts accordingly
        /// </summary>
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

        /// <summary>
        /// Coroutine used for race opening.
        /// Ignores user data. Uses a pre-baked speed curve instead
        /// </summary>
        /// <returns>IEnumerator used for coroutine work</returns>
        public IEnumerator OpenRace()
        {
            yield return new WaitForSeconds(1); //Some slight hackery with the countdown state. Have to wait for the countdown to finish.
            StateManager.instance.stateDictionary[GAME_STATE.COUNTDOWN].GetComponent<CountdownState>().countdownGUI.enabled = false;
            yield return new WaitForSeconds(openingTime - 1);
            Debug.Log("Exiting Opening!");

            loserBoost = AddLoserBoost();
            winnerBoost = RemoveWinnerBoost();
            StartCoroutine(loserBoost);
            StartCoroutine(winnerBoost);

            standings.ForEach(p =>
            {
                p.isOpening = false;
            });
        }

        /// <summary>
        /// Gives the losing a player a slight rotationSpeed boost over time. Based on <see cref="loserBoostWait"/> and <see cref="slipstreamInc"/> 
        /// </summary>
        /// <returns>IEnumerator used in coroutines</returns>
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

        /// <summary>
        /// Gradually removes rotationSpeed boost from previously losing (and now winning) player.
        /// Used whenever 2nd place overtakes 1st.
        /// Based on <see cref="winnerBoostWait"/> and <see cref="slipstreamInc"/>
        /// </summary>
        /// <returns></returns>
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
            var file = autoRunFiles[Random.Range(0, autoRunFiles.Count)];
            autoRunFiles.Remove(file);
            return file;
        }

        public void CleanUp()
        {

        }


    }
}