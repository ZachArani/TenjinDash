using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.Scripts
{

    /// <summary>
    /// FSM state for the game's start menu
    /// </summary>
    public class StartMenuState : MonoBehaviour
    {
        /// <summary>
        /// Timeline that controls the start menu cutscene
        /// </summary>
        public PlayableDirector startMenuTimeline;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Method called when FSM transitions to this state
        /// Updates the start menu's visibility
        /// If we are transitioning *to* StartMenu, then we show the UI elements
        /// Else if we are transitioning *away* from StartMenu, then we hide the UI elements
        /// Handles event fired by <see cref="StateManager.onGameStateChanged"/>
        /// </summary>
        /// <param name="from">State we transition from</param>
        /// <param name="to">State we transition to (this one)</param>
        public void UpdateMenuVisibility(GAME_STATE from, GAME_STATE to)
        {
            if (to == GAME_STATE.START_MENU)
            {
                startMenuTimeline.Play();
                UIManager.instance.ToggleStartUI(true);
                Cursor.visible = true;
                UIManager.instance.UpdateJoyConStatus();
            }
            else if (from == GAME_STATE.START_MENU)
            {
                UIManager.instance.ToggleStartUI(false);
                Cursor.visible = false;
            }

        }

        /// <summary>
        /// Called when Auto mode is selected.
        /// Adds 'auto' flag to context.
        /// </summary>
        public void OnAutoSelect()
        {
            Options.instance.isAuto = true;
            moveToNextState();
        }


        /// <summary>
        /// Called when start button is selected. Chooses next state based on options.
        /// </summary>
        public void OnStartSelect()
        {
            moveToNextState();
        }

        /// <summary>
        /// Decides which state to transition to based on flags.
        /// </summary>
        void moveToNextState()
        {
            startMenuTimeline.Stop();
            if (!Options.instance.skipPreroll)
            {
                StateManager.instance.TransitionTo(GAME_STATE.PREROLL);
            }
            else
            {
                if (Options.instance.skipCountdown)
                {
                    StateManager.instance.TransitionTo(GAME_STATE.RACE);
                }
                StateManager.instance.TransitionTo(GAME_STATE.COUNTDOWN);
            }
        }

        private void OnEnable()
        {
            StateManager.onGameStateChanged += UpdateMenuVisibility;
        }

        private void OnDisable()
        {

            StateManager.onGameStateChanged -= UpdateMenuVisibility;
        }

    }

}