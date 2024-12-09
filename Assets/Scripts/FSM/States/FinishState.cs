using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.Scripts.FSM.States
{
    public class FinishState : MonoBehaviour
    {


        [SerializeField]
        public PlayableDirector finishedTimeline;

        public string winnerTrackName;
        public string loserTrackName;

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
        /// <param name="to">State we're at (this one)</param>
        void StartFinish(GAME_STATE from, GAME_STATE to)
        {
            if (to == GAME_STATE.FINISH)
            {
                raceManager = StateManager.instance.stateDictionary[GAME_STATE.RACE].GetComponent<RaceState>();
                var winner = raceManager.standings[0].gameObject;
                var loser = raceManager.standings[1].gameObject;
                StateManager.instance.EnableRaceComponents(false);
                foreach (var playableAssetOutput in finishedTimeline.playableAsset.outputs)
                {
                    if (playableAssetOutput.streamName == winnerTrackName)
                    {
                        winner.GetComponent<Animator>().SetTrigger("win");
                        finishedTimeline.SetGenericBinding(playableAssetOutput.sourceObject, winner);
                    }
                    else if (playableAssetOutput.streamName == loserTrackName)
                    {
                        loser.GetComponent<Animator>().SetTrigger("lose");
                        finishedTimeline.SetGenericBinding(playableAssetOutput.sourceObject, loser);
                    }
                }
                Cursor.visible = true;
                UIManager.instance.toggleFinishUI(true);
                finishedTimeline.Play();
            }

        }

        public void onRestartSelect()
        {
            cleanUp();
            StateManager.instance.TransitionTo(GAME_STATE.PREROLL);
            finishedTimeline.Stop();
        }

        public void onMenuSelect()
        {
            cleanUp();
            StateManager.instance.TransitionTo(GAME_STATE.START_MENU);
            finishedTimeline.Stop();
        }

        /// <summary>
        /// Handles any sort of 'clean up' that needs to be done before restarting the game.
        /// </summary>
        public void cleanUp()
        {
            raceManager.standings.ForEach(p =>
            {
                p.GetComponent<Animator>().SetTrigger("Restart");
            });
            UIManager.instance.toggleFinishUI(false);
        }

        private void OnEnable()
        {
            StateManager.onGameStateChanged += StartFinish;
        }
    }
}