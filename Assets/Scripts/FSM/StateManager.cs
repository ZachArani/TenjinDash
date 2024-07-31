using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class StateManager : MonoBehaviour
{
    public bool SkipMenu;
    public bool skipPreroll;

    [field: SerializeField] public List<Camera> playerCams; //Util for race used by multiple states

    public static StateManager instance {  get; private set; }

    public static event Action<GAME_STATE, GAME_STATE> onGameStateChanged;

    public Options options; //TODO: Fold into this class

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
        contexts = new HashSet<GAME_CONTEXTS>();
        currentState = GAME_STATE.START_MENU;
        if (SkipMenu)
        {
            contexts.Add(GAME_CONTEXTS.SKIP_START_MENU);
            currentState = GAME_STATE.PREROLL;
        }
        if(skipPreroll)
        {
            contexts.Add(GAME_CONTEXTS.SKIP_PREROLL);
            currentState = GAME_STATE.COUNTDOWN;
        }
        Initialize(currentState);
    }

    public void Initialize(GAME_STATE startingState)
    {
        currentState = startingState;
        onGameStateChanged.Invoke(GAME_STATE.NONE, currentState);
    }

    public void TransitionTo(GAME_STATE nextState)
    {
        onGameStateChanged.Invoke(currentState, nextState);
        currentState = nextState;
    }

    public void EnableRaceCameras()
    {
        playerCams.ForEach(c => c.enabled = true);
    }

    public void DisableRaceCameras()
    {
        Debug.Log("Disabling Race Cameras.");
        playerCams.ForEach(c => c.enabled = false);
    }

}



/// <summary>
/// List of Game states
/// </summary>
public enum GAME_STATE
{
    NONE, //Used at start of game
    START_MENU,
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
    SOLO,

    RESTART,

    MUSIC_MUTED,
    SFX_MUTED,

    SKIP_START_MENU,
    SKIP_PREROLL,
    SKIP_COUNTDOWN,

}