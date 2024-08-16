using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.FSM.States
{
    public class FinishState : MonoBehaviour
    {

        public TextMeshProUGUI finish;
        public TextMeshProUGUI restart;
        public TextMeshProUGUI menu;

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
            }

        }

        private void OnEnable()
        {
            StateManager.onGameStateChanged += StartFinish;
        }
    }
}