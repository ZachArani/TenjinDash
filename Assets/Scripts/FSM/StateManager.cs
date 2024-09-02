using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class StateManager : MonoBehaviour
{
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
    [NonSerialized]
    public Options options;

    /// <summary>
    /// Current game state.
    /// </summary>
    public GAME_STATE currentState { get; private set; } //Current game state



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
        options = GetComponent<Options>();
        currentState = GAME_STATE.START_MENU;

        chooseStartingState();

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

    private void chooseStartingState()
    {
        if (options.skipMenu) //Checks against the game's options. If skip options are set, skip the game's starting state to selected mode.
        {
            currentState = GAME_STATE.PREROLL;
        }
        if (options.skipPreroll)
        {
            if (options.skipMenu) //If we're skipping menu AND preroll
            {
                if(options.skipCountdown) //If we're skipping menu AND preroll AND countdown
                {
                    currentState = GAME_STATE.RACE; //head right to the race
                }
                else
                {
                    currentState = GAME_STATE.COUNTDOWN; //Or to the countdown
                }
            }
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
