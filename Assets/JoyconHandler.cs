using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class JoyconHandler : MonoBehaviour
{
    // Start is called before the first frame update
    private List<Joycon> joycons;

    void Start()
    {
        joycons = JoyconManager.Instance.j; 
    }

    // Update is called once per frame
    void Update()
    {
        if(ButtonHandler.inMenu)
        {
            switch(joycons.Count)
            {
                case 0:
                    transform.GetChild(0).GetComponent<Image>().enabled = false;
                    transform.GetChild(1).GetComponent<Image>().enabled = false;
                    break;
                case 1:
                    transform.GetChild(0).GetComponent<Image>().enabled = true;
                    transform.GetChild(1).GetComponent<Image>().enabled = false;
                    break;
                case 2:
                    transform.GetChild(0).GetComponent<Image>().enabled = true;
                    transform.GetChild(1).GetComponent<Image>().enabled = true;
                    break;
            }
        }
    }
}
