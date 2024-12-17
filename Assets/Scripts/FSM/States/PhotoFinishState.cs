using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.Scripts.FSM.States
{
    /// <summary>
    /// FSM state for the photo-finish mode at the race's end.
    /// </summary>
    public class PhotoFinishState : MonoBehaviour
    {

        /// <summary>
        /// Timeline that controls the photo finish cutscene
        /// </summary>
        public PlayableDirector photoFinishTimeline;


        void Start()
        {

        }

        /// <summary>
        /// Starts PhotoFinish state
        /// Handles event fired by <see cref="StateManager.onGameStateChanged"/>
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void StartPhotoFinish(GAME_STATE from, GAME_STATE to)
        {
            if (to == GAME_STATE.PHOTO_FINISH)
            {
                //Enable our various components (in case we skipped here via debug statements)
                var raceManager = StateManager.instance.stateDictionary[GAME_STATE.RACE].GetComponent<RaceState>();
                StateManager.instance.EnableRaceComponents(true); 
                StateManager.instance.players.ForEach(p =>
                {
                    p.isPhotoFinish = true;
                    p.targetSpeed = p.targetSpeed > 0 ? p.targetSpeed : raceManager.maxSpeed; //In case they weren't running beforehand (i.e. we skipped here through menu options)
                    p.currentSpeed = p.currentSpeed > 0 ? p.currentSpeed : raceManager.maxSpeed;
                    p.GetComponent<Animator>().Play("RunTree");
                });
                StateManager.instance.DisableRaceCameras();
                UIManager.instance.ToggleRaceUI(false);
                UIManager.instance.ToggleScreenSplitter(false);
                Time.timeScale = 0f;
                photoFinishTimeline.Play();
            }
        }

        /// <summary>
        /// End photoFinish (transition to next state)
        /// </summary>
        public void EndPhotoFinish()
        {
            UIManager.instance.ToggleFinishText(false);
            SoundManager.instance.StopRaceMusic();
            StateManager.instance.TransitionTo(GAME_STATE.FINISH);
            photoFinishTimeline.Stop();
        }

        /// <summary>
        /// Stop the player running animations. 
        /// Lets player animators transition to winning/losing animations.
        /// </summary>
        public void StopRunning()
        {
            StateManager.instance.players.ForEach(p =>
            {
                p.GetComponent<Animator>().SetTrigger("finishRace");
            });
        }

        /// <summary>
        /// Shows finished UI element on screen
        /// </summary>
        public void ShowFinishText()
        {
            UIManager.instance.ToggleFinishText(true);
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