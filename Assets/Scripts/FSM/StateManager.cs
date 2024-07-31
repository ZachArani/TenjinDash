using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class StateManager : MonoBehaviour
{

    public static StateManager instance {  get; private set; }

    public PlayableDirector menuTimeline;
    public PlayableDirector cutscenesTimeline;
    public PlayableDirector countdownTimeline;

    public static event Action<GAME_STATE, GAME_STATE, HashSet<GAME_CONTEXTS>> gameStateChanged;

    public Options options;

    public GAME_STATE currentState { get; private set; } //Current game state

    public HashSet<GAME_CONTEXTS> contexts; //Set of current game contexts. HashSet is easy to search and only allows unique entries, which is perfect for our use case.

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        currentState = GAME_STATE.START_MENU;
        if (options.SkipMenu)
        {
            contexts.Add(GAME_CONTEXTS.SKIP_START_MENU);
            currentState = GAME_STATE.PREROLL;
        }
        if(options.skipPreroll)
        {
            contexts.Add(GAME_CONTEXTS.SKIP_PREROLL);
            currentState = GAME_STATE.COUNTDOWN;
        }
        if (options.skipCountdown)
        {
            contexts.Add(GAME_CONTEXTS.SKIP_COUNTDOWN);
            currentState = GAME_STATE.RACE;
        }

        gameStateChanged.Invoke(GAME_STATE.NONE, currentState, contexts);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}



/// <summary>
/// List of Game states
/// </summary>
public enum GAME_STATE
{
    NONE, //Used at start of game
    START_MENU,
    FINISH_MENU,
    PREROLL, //Cutscenes after start but before countdown
    COUNTDOWN, //Countdown before race
    RACE, //Actual race which starts when the countdown concludes
    PHOTO_FINISH, //State that plays when the two runners are neck-and-neck at the finish line 
    FINISH, //Game results screen
}

/// <summary>
/// List of game contexts that might change what happens in certain states.
/// </summary>
public enum GAME_CONTEXTS
{
    AUTO,
    ONE_PLAYER,
    TWO_PLAYER,

    RESTART,

    MUSIC_MUTED,
    SFX_MUTED,

    SKIP_START_MENU,
    SKIP_PREROLL,
    SKIP_COUNTDOWN,

}