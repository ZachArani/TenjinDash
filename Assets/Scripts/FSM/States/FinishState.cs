using Assets.Scripts.UI;
using System.Collections;
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

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void StartFinish(GAME_STATE from, GAME_STATE to)
        {
            if(to == GAME_STATE.FINISH)
            {

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