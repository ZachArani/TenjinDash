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

        public string podiumTrackName;
        public GameObject P1WinPodium;
        public GameObject P2WinPodium;

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
                GameObject podiums;
                if(raceManager.p1Won)
                {
                    podiums = Instantiate(P1WinPodium);
                }
                else
                {
                    podiums = Instantiate(P2WinPodium);
                }

                foreach (var playableAssetOutput in finishedTimeline.playableAsset.outputs)
                {
                    if (playableAssetOutput.streamName == podiumTrackName)
                    {
                        //finishedTimeline.SetGenericBinding(playableAssetOutput.sourceObject, podiums);
                        ControlPlayableAsset test = (ControlPlayableAsset)(((ControlTrack)playableAssetOutput.sourceObject).GetClips().ElementAt(0).asset);
                        test.prefabGameObject = podiums;
                        Debug.Log(test.prefabGameObject);
                        break;
                    }
                }
                Cursor.visible = true;
                UIManager.instance.toggleFinishUI(true);
                finishedTimeline.Play();
            }

        }

        public void onRestartSelect()
        {
            UIManager.instance.toggleFinishUI(false);
            StateManager.instance.TransitionTo(GAME_STATE.PREROLL);
            finishedTimeline.Pause();
        }

        public void onMenuSelect()
        {
            UIManager.instance.toggleFinishUI(false);
            StateManager.instance.TransitionTo(GAME_STATE.START_MENU);
            finishedTimeline.Pause();
        }

        private void OnEnable()
        {
            StateManager.onGameStateChanged += StartFinish;
        }
    }
}