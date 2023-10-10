using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SkipHandler : MonoBehaviour
{
    // Start is called before the first frame update

    PlayableDirector director;
    bool pressed;
    void Start()
    {
        director = GameObject.Find("Timelines").transform.GetChild(1).GetComponent<PlayableDirector>();
    }

    // Update is called once per frame
    void Update()
    {
        if(director.state == PlayState.Playing)
        {
            if(Input.GetKeyDown(KeyCode.S) && !pressed)
            {
                Debug.Log("SKIP");
                director.time = 910;
                pressed = true;
            }
        }
        else if(director.state != PlayState.Playing && pressed)
        {
            pressed = false;
        }
    }
}
