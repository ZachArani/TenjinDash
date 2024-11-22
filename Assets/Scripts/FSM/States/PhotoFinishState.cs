using Assets.Scripts.UI;
using Cinemachine;
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
                var raceManager = StateManager.instance.stateDictionary[GAME_STATE.RACE].GetComponent<RaceState>();
                StateManager.instance.EnableRaceComponents(true);
                StateManager.instance.players.ForEach(p => {
                    p.isPhotoFinish = true;
                    p.targetSpeed = p.targetSpeed > 0 ? p.targetSpeed : raceManager.maxSpeed; //In case they weren't running beforehand (i.e. we skipped here through menu options)
                    p.currentSpeed = p.currentSpeed > 0 ? p.currentSpeed : raceManager.maxSpeed;
                    p.GetComponent<Animator>().Play("RunTree");
                    p.GetComponent<CinemachineDollyCart>().m_Position = 545;
                });
                StateManager.instance.DisableRaceCameras();
                UIManager.instance.toggleRaceUI(false);
                UIManager.instance.toggleScreenSplitter(false);
                Time.timeScale = 0f;
                photoFinishTimeline.Play();
            }
        }

        public void EndPhotoFinish()
        {
            UIManager.instance.toggleFinishText(false);
            StateManager.instance.TransitionTo(GAME_STATE.FINISH);
            photoFinishTimeline.Stop();
        }

        public void StopRunning()
        {
            StateManager.instance.players.ForEach(p =>
            {
                p.GetComponent<Animator>().SetTrigger("finishRace");
            });
        }

        public void ShowFinishText()
        {
            UIManager.instance.toggleFinishText(true);
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