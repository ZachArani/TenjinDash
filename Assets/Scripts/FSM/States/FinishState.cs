using Assets.Scripts.UI;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Assets.Scripts.FSM.States
{
    public class FinishState : MonoBehaviour
    {

        public TextMeshProUGUI finish;
        public TextMeshProUGUI restart;
        public TextMeshProUGUI menu;

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

        void StartFinish(GAME_STATE from, GAME_STATE to)
        {
            if(to == GAME_STATE.FINISH)
            {
                var winner = raceManager.standings[0].gameObject;
                var loser = raceManager.standings[1].gameObject;
                foreach (var playableAssetOutput in finishedTimeline.playableAsset.outputs)
                {
                    if (playableAssetOutput.streamName == winnerTrackName)
                    {
                        winner.GetComponent<Animator>().SetTrigger("Win");
                        finishedTimeline.SetGenericBinding(playableAssetOutput.sourceObject, winner);
                    }
                    else if(playableAssetOutput.streamName == loserTrackName)
                    {
                        loser.GetComponent<Animator>().SetTrigger("Lose");
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

        public void cleanUp()
        {
            raceManager.standings.ForEach(p => {
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