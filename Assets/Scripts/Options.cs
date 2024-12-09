using System.Linq;
using UnityEngine;

/// <summary>
/// Controls game options.
/// Basically just contains a bun of flags that can be referenced elsewhere.
/// </summary>
public class Options : MonoBehaviour
{
    public static Options instance { get; private set; }

    [SerializeField]
    private bool _skipMenu;
    public bool skipMenu { get { return _skipMenu; } set { _skipMenu = value; } }

    [SerializeField]
    private bool _skipPreroll;
    public bool skipPreroll { get { return _skipPreroll; } set { _skipPreroll = value; } }

    [SerializeField]
    private bool _skipCountdown;
    public bool skipCountdown { get { return _skipCountdown; } set { _skipCountdown = value; } }

    [SerializeField]
    private bool _skipRace;
    public bool skipRace { get { return _skipRace; } set { _skipRace = value; } }

    [SerializeField]
    private bool _skipPhotoFinish;
    public bool skipPhotoFinish { get { return _skipPhotoFinish; } set { _skipPhotoFinish = value; } }

    /// <summary>
    /// Skips the finish menu (and returns to the main menu)
    /// </summary>
    [SerializeField]
    private bool _skipFinishMenu;
    public bool skipFinishMenu { get { return _skipFinishMenu; } set { _skipFinishMenu = value; } }

    /// <summary>
    /// Decides if all characters are running automatically.
    /// </summary>
    [SerializeField]
    private bool _isAuto;
    public bool isAuto { get { return _isAuto; } set { _isAuto = value; } }

    /// <summary>
    /// Mute game SFX
    /// </summary>
    [SerializeField]
    private bool _muteSFX;
    public bool muteSFX { get { return _muteSFX; } set { _muteSFX = value; } }

    /// <summary>
    /// Mute game music
    /// </summary>
    [SerializeField]
    private bool _muteMusic;
    public bool muteMusic { get { return _muteMusic; } set { _muteMusic = value; } }

    /// <summary>
    /// Records player data in .csv format using the InputRecorders attached to the various players.
    /// </summary>
    [SerializeField]
    private bool _recordPlayerData;
    public bool recordPlayerData { get { return _recordPlayerData; } set { _recordPlayerData = value; } }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this);
    }


    private void OnValidate()
    {
    }

    /// <summary>
    /// Calculates the starting state for the game based on the selected options.
    /// </summary>
    /// <returns>The correct game starting based on options selected</returns>
    public GAME_STATE GetStartingState()
    {
        bool[] skipOptions = { skipMenu, skipPreroll, skipCountdown, skipRace, skipPhotoFinish, skipFinishMenu };
        foreach (var skip in skipOptions.Select((value, i) => new { i, value }))
        {
            if (!skip.value)
            {
                return (GAME_STATE)skip.i + 1;
            }
        }
        return GAME_STATE.START_MENU; //Return start menu by default
    }
}

