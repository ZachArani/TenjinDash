using AYellowpaper.SerializedCollections;
using Cinemachine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
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
    /// Event that triggers whenever the FSM changes state.
    /// Event handled by each state.
    /// </summary>
    public static event Action<GAME_STATE, GAME_STATE> onGameStateChanged;

    [ReadOnly]
    public GAME_STATE currentState;

    [ReadOnly]
    public GameObject currentStateObject;

    /// <summary>
    /// Translates <see cref="GAME_STATE"/> enum values to real gameObjects.
    /// Can be referenced by other classes as needed.
    /// </summary>
    [SerializedDictionary("State Enum", "GameObject")]
    public SerializedDictionary<GAME_STATE, GameObject> stateDictionary;

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

    void Start()
    {
        currentState = Options.instance.GetStartingState();

        Initialize(currentState);
    }

    /// <summary>
    /// Handles starting up the state machine.
    /// </summary>
    /// <param name="startingState">State the FSM begins with.</param>
    public void Initialize(GAME_STATE startingState)
    {
        currentState = startingState;
        currentStateObject = stateDictionary[currentState];
        Debug.Log($"STARTING GAME WITH {currentState} STATE.");
        onGameStateChanged.Invoke(GAME_STATE.NONE, currentState);
    }

    /// <summary>
    /// Transitions the state machine to a new state. 
    /// Triggers <see cref="onGameStateChanged"/> event, which allows a subscribed state object to handle the transition.
    /// Obviously this requires states to be on good behavior, but we assume the best of our programming here...
    /// </summary>
    /// <param name="nextState">Next state to transition to</param>
    public void TransitionTo(GAME_STATE nextState)
    {
        Debug.Log($"TRANSITION TO {nextState}");
        onGameStateChanged.Invoke(currentState, nextState);
        currentState = nextState;
        currentStateObject = stateDictionary[currentState];
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

    /// <summary>
    /// Enable all player components used in racing 
    /// </summary>
    /// <param name="doEnable"></param>
    public void EnableRaceComponents(bool doEnable)
    {
        players.ForEach(p =>
        {
            p.enabled = doEnable;
            p.GetComponent<CinemachineDollyCart>().enabled = doEnable;
        });
    }

    /// <summary>
    /// Enables any components used in recording player data.
    /// </summary>
    /// <param name="doEnable"></param>
    public void EnableRecorders(bool doEnable)
    {
        players.ForEach(p =>
        {
            if (Options.instance.recordPlayerData)
            {
                p.GetComponent<InputRecorder>().enabled = doEnable;
            }
        });
    }

    public void OnApplicationQuit()
    {
        StopAllCoroutines();
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
