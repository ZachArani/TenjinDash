using Assets.Scripts.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.Scripts.FSM.States
{
    public class PhotoFinishState : MonoBehaviour
    {

        /// <summary>
        /// Timeline that controls the photo finish cutscene
        /// </summary>
        public PlayableDirector photoFinishTimeline;


        void Start()
        {

        }

        void StartPhotoFinish(GAME_STATE from, GAME_STATE to)
        {
            if(to == GAME_STATE.PHOTO_FINISH)
            {
                StateManager.instance.DisableRaceCameras();
                UIManager.instance.toggleRaceUI(false);
                UIManager.instance.toggleScreenSplitter(false);
                Time.timeScale = 0f;
                photoFinishTimeline.Play();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ResumeTime()
        {
            Time.timeScale = 1f;
        }

        private void OnEnable()
        {
            StateManager.onGameStateChanged += StartPhotoFinish;   
        }

        private void OnDisable()
        {
            StateManager.onGameStateChanged -= StartPhotoFinish;
        }
    }
}