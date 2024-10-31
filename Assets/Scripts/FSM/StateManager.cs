using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Playables;
using Debug = UnityEngine.Debug;


/// <summary>
/// Singleton that manages the FSM overseeing the game.
/// </summary>
[Serializable]
public class StateManager : MonoBehaviour
{
    /// <summary>
    /// StateMachine singleton.
    /// </summary>
    public static StateManager instance { get; private set; }

    /// <summary>
    /// List of player cameras. Placed in this class because it ends up getting used by several states anyways.
    /// </summary>
    [field: SerializeField] public List<Camera> playerCams;

    /// <summary>
    /// Event that triggers whenever the FSM changes state
    /// </summary>
    public static event Action<GAME_STATE, GAME_STATE> onGameStateChanged;

    /// <summary>
    /// Current game state (As an enum value).
    /// </summary>
    [ReadOnly, SerializeField]
    private GAME_STATE _currentState; //Current game state
    public GAME_STATE currentState { get { return _currentState; } set { _currentState = value; } }


    /// <summary>
    /// List containing all players (through their NewMovement Component) in the race.
    /// </summary>
    [SerializeField]
    private List<NewMovement> _players;

    /// <summary>
    /// Public facing list of players. 
    /// Ordered by current standing (or player order if not in race)
    /// </summary>
    public List<NewMovement> players { get { return _players; } private set { _players = value; } }

    /// <summary>
    /// Number of players in race.
    /// </summary>
    public int numPlayers => _players.Count;

    /// <summary>
    /// Determines where the finish point is that ends the race and starts the photoFinish/Finish modes
    /// Stored as the distance from the starting point (e.g. 541.5 units)
    /// </summary>
    [SerializeField]
    private float _finishLinePos;
    public float finishLinePos { get { return _finishLinePos; } private set { _finishLinePos = value; } }

    [SerializeField]
    private float _trackFinishPos;
    public float trackFinishPos { get { return _trackFinishPos; } private set { _trackFinishPos = value; } }

    /// <summary>
    /// Decides how much of a speed boost to give to the losing runners
    /// </summary>
    [SerializeField]
    [Range(0f, 2f)]
    float _losingSpeedBoost = 0.3f;
    public float losingSpeedBoost { get { return _losingSpeedBoost; } set { _losingSpeedBoost = value; } }

    public Stopwatch stateStopwatch;

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
        currentState = Options.instance.GetStartingState();
        stateStopwatch = new Stopwatch();

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
        stateStopwatch.Start();
        onGameStateChanged.Invoke(GAME_STATE.NONE, currentState);
    }

    /// <summary>
    /// Transitions the state machine to a new state. 
    /// Triggers onGameStateChanged event, which allows a subscribed state object the handle the transition.
    /// Obviously this requires states to be on good behavior, but we assume the best of our programming here...
    /// </summary>
    /// <param name="nextState">Next state to transition to</param>
    public void TransitionTo(GAME_STATE nextState)
    {
        Debug.Log($"TRANSITION TO {nextState}");
        onGameStateChanged.Invoke(currentState, nextState);
        currentState = nextState;
        stateStopwatch.Restart();
    }

    /// <summary>
    /// Utility function to doEnable each player's race camera.
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

    public void EnableRaceComponents(bool doEnable)
    {
        players.ForEach(p => {
            p.enabled = doEnable;
            p.GetComponent<CinemachineDollyCart>().enabled = doEnable;
        });
    }

    public void EnableRecorders(bool doEnable)
    {
        players.ForEach(p => {
            if (Options.instance.recordPlayerData)
            {
                p.GetComponent<InputRecorder>().enabled = doEnable;
            }
        });
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
