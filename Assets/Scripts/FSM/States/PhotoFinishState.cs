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

        // Update is called once per frame
        void Update()
        {

        }
    }
}