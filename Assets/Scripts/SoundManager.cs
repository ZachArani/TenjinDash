using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public AudioClip countDownFinishSFX;
    public AudioClip raceMusic;

    /// <summary>
    /// SoundManager singleton instance.
    /// </summary>
    public static SoundManager instance { get; private set; }

    AudioSource player;


    /// <summary>
    /// Used to ensure singleton is properly loaded. Either creates one instance or kills any other instance.
    /// </summary>
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return; //Don't accidentally destroy your references, ya dummy! 
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this);
    }


    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayCountDownFinish()
    {
        player.PlayOneShot(countDownFinishSFX);
    }

    public void PlayRaceMusic()
    {
        player.Play();
    }

    public void StopRaceMusic()
    {
        Debug.Log("Stopping player"!);
        player.Stop();
    }

}
