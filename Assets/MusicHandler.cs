using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicHandler : MonoBehaviour
{
    // Start is called before the first frame update
    AudioSource raceStart;
    AudioSource raceLoop;
    FinishedHandler finishedHandler;
    bool startedMusic;
    bool startedLoop;
    void Start()
    {
        raceStart = gameObject.GetComponents<AudioSource>()[0];
        raceStart.loop = false;
        raceLoop = gameObject.GetComponents<AudioSource>()[1];
        raceLoop.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(startedMusic && !raceStart.isPlaying && !startedLoop)
        {
            raceLoop.Play();
            startedLoop = true;
        }
    }

    public void StartMusic()
    {
        startedMusic = true;
        raceStart.Play();
    }

    public void StopMusic()
    {
        raceLoop.Stop();
    }

    public void ResetState()
    {
        startedLoop = false;
        startedMusic = false;
    }


}
