using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMusicHandler : MonoBehaviour
{
    // Start is called before the first frame update
    AudioSource menuMusic;
    void Start()
    {
        menuMusic = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayMusic()
    {
        if (!menuMusic.isPlaying)
            menuMusic.Play();
    }

    public void StopMusic()
    {
        menuMusic.Stop(); 
    }
}
