using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class UIManager : MonoBehaviour
    {

        [SerializeField] public TextMeshProUGUI countdown;
        [SerializeField] public TextMeshProUGUI finish;
        [SerializeField] public TextMeshProUGUI menu;
        [SerializeField] public TextMeshProUGUI auto;
        [SerializeField] public TextMeshProUGUI logo;
        public GameObject JoyCons;
        public GameObject credits;


        List<TextMeshProUGUI> textObj = new List<TextMeshProUGUI>();


        void Start()
        {

            textObj = new List<TextMeshProUGUI>{ countdown, finish, menu, auto, logo };
            foreach(TextMeshProUGUI credit in credits.GetComponentsInChildren<TextMeshProUGUI>())
            { 
                textObj.Add(credit);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ShowMainMenu()
        {
            textObj.ForEach(text => text.enabled = true);
            foreach (Image joycon in JoyCons.GetComponentsInChildren<Image>())
            {
                joycon.enabled = true;
            }
        }

        public void HideMainMenu()
        {
            textObj.ForEach(text => text.enabled = false);
            foreach(Image joycon in JoyCons.GetComponentsInChildren<Image>())
            {
                joycon.enabled = false;
            }
        }
    }
}