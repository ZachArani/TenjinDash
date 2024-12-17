using Assets.Scripts.UI;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.Scripts.FSM.States
{
    public class CountdownState : MonoBehaviour
    {
        /// <summary>
        /// UI element for countdown graphics
        /// </summary>
        public TextMeshProUGUI countdownGUI;
        /// <summary>
        /// Timeline that controls the countdown
        /// </summary>
        public PlayableDirector countdownTimeline;

        /// <summary>
        /// Current number in the countdown
        /// </summary>
        int countdown;

        public int preMoveTime = 240;
        public AnimationCurve preMoveCurve;

        RaceState raceManager;

        void Start()
        {
            raceManager = StateManager.instance.stateDictionary[GAME_STATE.RACE].GetComponent<RaceState>();
        }

        // Update is called once per frame
        void Update()
        {

        }


        /// <summary>
        /// Handles event fired by <see cref="StateManager.onGameStateChanged"/>
        /// </summary>
        /// <param name="from">State we came from</param>
        /// <param name="to">State we're at (this one).</param>
        void StartCountdown(GAME_STATE from, GAME_STATE to)
        {
            if (to == GAME_STATE.COUNTDOWN)
            {
                countdown = 3;
                if (Options.instance.skipPreroll) //We need to enable the player cameras manually if we skipped Preroll.
                {
                    StateManager.instance.EnableRaceCameras();
                    Cursor.visible = false;
                    StateManager.instance.players.ForEach(p => p.GetComponent<CinemachineDollyCart>().enabled = true);
                }
                countdownTimeline.Play();
                countdownGUI.text = countdown.ToString();
                countdownGUI.enabled = true;
                UIManager.instance.ToggleCountdownUI(true);
                UIManager.instance.ToggleScreenSplitter(true);
                UIManager.instance.ToggleRaceUI(true);
            }
        }

        /// <summary>
        /// Update countdown UI in-game
        /// Runs every second via timeline signals
        /// </summary>
        public void UpdateCountdown()
        {
            countdownGUI.text = countdown.ToString();
            countdown--;
            if (countdown == -1)
            {
                countdownGUI.text = "GO!";
                StateManager.instance.TransitionTo(GAME_STATE.RACE);
            }
            else if (countdown == -2) //We've past the countdown time, hide from screen and transition to next state.
            {
                countdownGUI.enabled = false;
                StateManager.instance.TransitionTo(GAME_STATE.RACE);
            }
        }

        public void StartPreMove()
        {
        }


        private void OnEnable()
        {
            StateManager.onGameStateChanged += StartCountdown;
        }

        private void OnDisable()
        {
            StateManager.onGameStateChanged -= StartCountdown;
        }

    }
}