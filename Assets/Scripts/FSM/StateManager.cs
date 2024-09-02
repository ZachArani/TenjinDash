using Cinemachine;
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

    /// <summary>
    /// List of player cameras. Placed in this class because it ends up getting used by several states anyways.
    /// </summary>
    [field: SerializeField] public List<Camera> playerCams;

    /// <summary>
    /// StateMachine singleton.
    /// </summary>
    public static StateManager instance {  get; private set; }

    /// <summary>
    /// Event that triggers whenever the FSM changes state
    /// </summary>
    public static event Action<GAME_STATE, GAME_STATE> onGameStateChanged;

    /// <summary>
    ///  Object containing game options
    /// </summary>
    public Options options; //TODO: Fold into this class

    /// <summary>
    /// Current game state.
    /// </summary>
    public GAME_STATE currentState { get; private set; } //Current game state


    /// <summary>
    /// Set of current game contexts. HashSet is easy to search and only allows unique entries, which is perfect for our use case.
    /// Game contexts can be any arbitrary state information: "SFX muted", "AUTO MODE", "2 PLAYER", etc.
    /// Basically a utility variable to keep track of any extra flags floating around. 
    /// May be removed if everything can be refactored to other classes.
    /// </summary>
    public HashSet<GAME_CONTEXTS> contexts { get; private set; }

    ///for camera enable/disabling. Needed by multiple states so it goes here. 
    //TODO: Move to utils class
    //TODO: Generify to support many players--using a collection of some sort.
    public CinemachineBrain player1Brain;
    public CinemachineBrain player2Brain;
    public CinemachineVirtualCamera player1Cam;
    public CinemachineVirtualCamera player2Cam;


    /// <summary>
    /// Used to ensure singleton is properly loaded. Either creates one instance or kills any other instance.
    /// </summary>
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
        if (SkipMenu) //Checks against the game's options. If skip options are set, skip the game's starting state to selected mode.
        {
            contexts.Add(GAME_CONTEXTS.SKIP_START_MENU);
            currentState = GAME_STATE.PREROLL;
        }
        if(skipPreroll)
        {
            contexts.Add(GAME_CONTEXTS.SKIP_PREROLL);
            if (SkipMenu)
            {
                currentState = GAME_STATE.COUNTDOWN;
            }
        }
        Initialize(currentState);
    }

    /// <summary>
    /// Handles starting up the state machine.
    /// </summary>
    /// <param name="startingState">State the FSM begins with.</param>
    public void Initialize(GAME_STATE startingState)
    {
        currentState = startingState;
        Debug.Log($"STARTING GAME WITH {currentState} STATE.");
        onGameStateChanged.Invoke(GAME_STATE.NONE, currentState);
    }

    /// <summary>
    /// Transitions the state machine to a new state. 
    /// Triggers onGameStateChanged event
    /// </summary>
    /// <param name="nextState">Next state to transition to</param>
    public void TransitionTo(GAME_STATE nextState)
    {
        Debug.Log($"TRANSITION TO {nextState}");
        onGameStateChanged.Invoke(currentState, nextState);
        currentState = nextState;
    }

    /// <summary>
    /// Utility function to enable each player's race camera.
    /// </summary>
    public void EnableRaceCameras()
    {
        Debug.Log("Enabling race cameras.");
        playerCams.ForEach(c => c.enabled = true);
    }

    /// <summary>
    /// Utility function to disable each player's race cameras.
    /// </summary>
    public void DisableRaceCameras()
    {
        Debug.Log("Disabling race cameras.");
        playerCams.ForEach(c => c.enabled = false);
    }

    /// <summary>
    /// Adds a new context flag to the game contexts.
    /// </summary>
    /// <param name="context">context flag to add to current contexts</param>
    public void addContext(GAME_CONTEXTS context)
    {
        contexts.Add(context);
    }

    /// <summary>
    /// Removes context flag from game contexts. 
    /// </summary>
    /// <param name="context"></param>
    public void removeContext(GAME_CONTEXTS context)
    {
        if(!contexts.Remove(context))
        {
            Debug.Log($"{context} not found in current flags!");
        }
    
    }


}



/// <summary>
/// List of Game states
/// </summary>
public enum GAME_STATE
{
    NONE, //Used at start of game, since we "enter" START_MENU from no prior state.
    START_MENU, //Start menu
    PREROLL, //Cutscenes after start but before countdown
    COUNTDOWN, //Countdown before race
    RACE, //Actual race which starts when the countdown concludes
    PHOTO_FINISH, //State that plays when the two runners are neck-and-neck at the finish line 
    FINISH, //Game results screen
}

/// <summary>
/// List of game contexts that might change what happens in certain states.
/// Basically all the different "flags" the game might have at any given time.
/// </summary>
public enum GAME_CONTEXTS
{
    AUTO, //Both players are running automatically. "Demo" mode
    SOLO, //One player runs automatically. 1 Player mode.

    RESTART, //Need to restart the game

    MUSIC_MUTED,
    SFX_MUTED,

    SKIP_START_MENU,
    SKIP_PREROLL,
    SKIP_COUNTDOWN,

}