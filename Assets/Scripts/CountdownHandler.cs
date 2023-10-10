using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Time = UnityEngine.Time;

public class CountdownHandler : MonoBehaviour
{
    // Start is called before the first frame update

    TextMeshProUGUI shadow;
    TextMeshProUGUI text;
    [System.NonSerialized] public bool isCounting;
    int frameStart;
    public int updateCount = 150;
    bool postCountdown;
    MusicHandler musicHandler;
    AudioSource countdownSFX;
    AudioSource countdownFinishSFX;
    
    void Start()
    {
        musicHandler = GameObject.Find("Music").transform.Find("Race").GetComponent<MusicHandler>();
        text = gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        shadow = gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        countdownSFX = GameObject.Find("Music").transform.Find("Countdown").GetComponents<AudioSource>()[0];
        countdownFinishSFX = GameObject.Find("Music").transform.Find("Countdown").GetComponents<AudioSource>()[1];
        //isCounting = true;
        text.text = "";
        shadow.text = "";
        updateCount = 150;
        postCountdown = false;
        if(!isCounting)
            frameStart = int.MaxValue;
    }

    private void FixedUpdate()
    {
        if (isCounting)
        {
            if (updateCount == 0)
            {
                text.text = shadow.text = "GO";
                postCountdown = true;
                isCounting = false;
                countdownFinishSFX.Play();
                musicHandler.StartMusic();
            }
            else if (updateCount % 50 == 0)
            {
                text.text = shadow.text = (updateCount / 50).ToString();
                countdownSFX.Play();
            }
            updateCount--;
        }
        else if(postCountdown)
        {
            updateCount--;
            if (updateCount < -50)
            {
                text.text = shadow.text = "";
                postCountdown = false;
            }
        }
    }

    public void StartCounting()
    {
        isCounting = true;
        frameStart = UnityEngine.Time.frameCount;
    }

    public void ResetCount()
    {
        updateCount = 150;
    }

}
