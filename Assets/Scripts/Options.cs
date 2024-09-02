using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls game options.
/// Basically just contains a bun of flags that can be referenced elsewhere.
/// </summary>
public class Options : MonoBehaviour
{
    /// <summary>
    /// Skips main menu. Debug option for speeding up testing.
    /// </summary>
    [SerializeField]
    private bool _skipMenu;
    public bool skipMenu { get { return _skipMenu; } set { _skipMenu = value; } }

    /// <summary>
    /// Skips preroll cutscenes before race.
    /// </summary>
    [SerializeField]
    private bool _skipPreroll;
    public bool skipPreroll { get { return _skipPreroll; } set { _skipPreroll = value; } }

    /// <summary>
    /// Skips countdown before race. For debug purposes.
    /// </summary>
    [SerializeField]
    private bool _skipCoutndown;
    public bool skipCountdown { get { return _skipCoutndown; } set { _skipCoutndown = value; } }

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
        
    }
}
