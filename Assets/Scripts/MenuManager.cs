using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Assets.Scripts
{

    public class MenuManager : MonoBehaviour
    {
        [field: SerializeField] public RaceManager raceManager;
        [field: SerializeField] public UIManager UIManager;
        //[field: SerializeField] public 

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnEnable()
        {
            //UIManager.OnStartMenuEnter +=
        }

        private void OnDisable()
        {

        }
    }

}

