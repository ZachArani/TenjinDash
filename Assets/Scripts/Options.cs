using Assets.Scripts.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

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
    public bool skipCountdown { get { return _skipCountdown; } set { _skipCountdown = value;  } }

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
    /// Decides if only player one is running.
    /// </summary>
    [SerializeField]
    private bool _isSolo;
    public bool isSolo { get { return _isSolo; } set { _isSolo = value; } }

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

    public GAME_STATE GetStartingState()
    {
        bool[] skipOptions = { skipMenu, skipPreroll, skipCountdown, skipRace, skipPhotoFinish, skipFinishMenu};
        foreach (var skip in skipOptions.Select((value, i) => new { i, value}))
        {
            if(!skip.value)
            {
                return (GAME_STATE)skip.i + 1;
            }
        }
        return GAME_STATE.START_MENU; //Return start menu by default
    }
}

