using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.Scripts.FSM.States
{
    /// <summary>
    /// FSM state for the pre-race cutscenes.-
    /// </summary>
    public class PrerollState : MonoBehaviour
    {
        public PlayableDirector preRollTimeline;


        void Start()
        {

        }

        void Update()
        {

        }

        /// <summary>
        /// Starts PreRoll state. 
        /// Handles event fired by <see cref="StateManager.onGameStateChanged"/>
        /// </summary>
        /// <param name="from">State we transitioned from</param>
        /// <param name="to">State we're going to. Always this (PreRoll) state in this case.</param>
        private void StartPreRoll(GAME_STATE from, GAME_STATE to)
        {
            if (to == GAME_STATE.PREROLL)
            {
                Cursor.visible = false;
                preRollTimeline.Play();
                StateManager.instance.players.ForEach(p => p.GetComponent<CinemachineDollyCart>().enabled = true);
            }
        }

        /// <summary>
        /// Handle preroll ending.
        /// Transition to next state.
        /// </summary>
        public void FinishedPreRoll()
        {
            Debug.Log("FINISHED PREROLL!");
            if (!Options.instance.skipCountdown)
            {
                StateManager.instance.TransitionTo(GAME_STATE.COUNTDOWN);
            }
            else
            {
                StateManager.instance.TransitionTo(GAME_STATE.RACE);
            }
        }

        private void OnEnable()
        {
            StateManager.onGameStateChanged += StartPreRoll;
        }

        private void OnDisable()
        {
            StateManager.onGameStateChanged -= StartPreRoll;
        }
    }
}