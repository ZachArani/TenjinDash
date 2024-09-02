using System.Collections;
using UnityEngine;

namespace Assets.Scripts.FSM.States
{
    public class PhotoFinishState : MonoBehaviour
    {
        /// <summary>
        /// The distance along the path where we enter photo finish mode.
        /// </summary>
        [SerializeField]
        private float _photoFinishStart;
        public float photoFinishStart { get { return _photoFinishStart; } }

        void Start()
        {

        }

        void StartPhotoFinish(GAME_STATE from, GAME_STATE to)
        {
            if(to == GAME_STATE.PHOTO_FINISH)
            {
            }
        }

        // Update is called once per frame
        void Update()
        {

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