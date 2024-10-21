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
                if(StateManager.instance.options.skipRace)
                {
                    StateManager.instance.players.ForEach(p => { 
                        p.enabled = true;
                        p.desiredSpeed = p.maxRunnerSpeed;
                        p.isAuto = true;
                        p.GetComponent<Animator>().Play("RunTree");
                        p.GetComponent<CinemachineDollyCart>().m_Position = StateManager.instance.finishLinePos;
                    }); //Enable players and set their speedBoost to the config'd value
                }
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