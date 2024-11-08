using Assets.Scripts.FSM.States;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;


/// <summary>
/// Controls player movement during race
/// The process is basically:
/// 1) Wait for new data from joycon
/// 2) Add new gyro data to current "sum"
/// 3) Once a specific period of time has elapsed, average all the joycon data into one data point for speed calculations
/// 4)Update the player's desired speed based on this new data
/// 5) Smoothly move player speed up or down based on desired speed and acceleration curve
/// 
/// </summary>
public class NewMovement : MonoBehaviour
{

    public int playerNum;

    public bool isAuto;
    public bool isPlayback;

    Joycon joycon;

    /// <summary>
    /// Defines the time window of gyro readings
    /// Once we reach the end of a time window, we average the summed data to calculate the next speed data point.
    /// </summary>
    [Range(0.05f, 1f)]
    public float timeWindow = 0.1f;
    /// <summary>
    /// Keeps track of the current time WITHIN the time window
    /// Used to check if we've past the time window
    /// </summary>
    float currentTimeInWindow;


    //Gyro measurements
    /// <summary>
    /// Factor used to control smoothing the gyro data.
    /// Calculated in logarithm format for easier modification
    /// </summary>
    [Range(0f, 5f)]
    public float maxGyroChangeLog = 3f;
    /// <summary>
    /// Defines how rapidly the gyro data readings change
    /// Basically, this helps smooth data in case of bad readings or sudden stops.
    /// </summary>
    float maxGyroChange;
    /// <summary>
    /// A basic correction factor that subtracts from the gyro readings
    /// Used to put "base level" readings close to 0
    /// Otherwise, "no movement" might read as something like 3 or 5 or 15 instead of 0
    /// </summary>
    [Range(0f, 10f)]
    public float correctionFactor = 3;
    /// <summary>
    /// Defines how much speed can be generated from the gyro readings itself.
    /// Basically defines the maximum cap of effort a player can put into physically running
    /// NOTE: this is different from the max player speed. That controls the actual in-game speed of the player. 
    /// This controls the data read from the gyro sensor.
    /// </summary>
    [Range(50f, 100f)]
    public float maxGyroSpeed = 80f;

    /// <summary>
    /// Utility variable that defines what "percentage" the current gyro data is at.
    /// i.e., if the current gyro value is at 40, and the current maxGyroSpeed is at 80, then the gyroPercentage is 50%
    /// Variable is used to understand how fast the player is running relative to the possible maximum effort.
    /// </summary>
    public float gyroPercentage { get; private set; }


    [Range(5f, 30f)]
    public float maxRunnerSpeed;

    public float speed { get; private set; }
    /// <summary>
    /// The speed the player starts when the race begins.
    /// This variable is required because the characters "leap" into a sprint at the race's start.
    /// Meanwhile, the people *playing* the game usually don't start running until *after* the countdown begins
    /// So we define a starting speed for each player character, which will be their speed for the first few seconds of the game
    /// Eventually is replaced by actual gyro data from players. 
    /// </summary>
    [SerializeField]
    [Range(0f, 25f)]
    float startingSpeed;

    public AnimationCurve speedCurve;

    /// <summary>
    /// The player's desired speed.
    /// Based on the most current gyro data. Indicates if the player should speed up or slow down from their current in-game speed.
    /// Can be manipulated by other classes. Namely the PhotoFinish state to handle a skipToPhotoFinish mode
    /// </summary>
    [ReadOnly]
    public float desiredSpeed;

    /// <summary>
    /// Defines the maximum change of the t value, which is used in lerping the player's speed up or down.
    /// Basically, this forces the player to smoothly accelerate.
    /// If there was no max cap to the t value change, then the player could go from 0 to 100 in an instant.
    /// </summary>
    [Range(0f, 0.4f)]
    public float maxTChange;

    bool isAccelerating;


    /// <summary>
    /// The running track this player races on.
    /// Defines exactly where they are running during the race.
    /// </summary>
    public CinemachineDollyCart track;

    Animator animator;

    public TextAsset playbackFile;

    Queue<string> playbackData = new Queue<string>();




    /// <summary>
    /// Improvement
    /// </summary>

    Queue<float> joyconInput = new Queue<float>();
    const int AVG_WINDOW = 35;
    float max;
    const float EST_MAX = 3f;

    public TextMeshProUGUI debugText;
    public AnimationCurve speedCurveImproved;

    public float joyconEffort;

    [Range(0f, 1000f)]
    public float runnerSpeed;

    [Range(0f, 100f)]
    public float targetSpeed;
    [ReadOnly]
    public float currentSpeed;
    [ReadOnly]
    public float finalSpeed;

    [ReadOnly]
    public float tImproved;

    const float TIME_TO_100 = 4f;
    [ReadOnly]
    public float delta_t;

    [ReadOnly]
    public float dist;

    RaceState raceManager;

    [ReadOnly]
    public float rubberbandBoost;


    // Start is called before the first frame update
    void Start()
    {
        track = GetComponent<CinemachineDollyCart>();
        animator = GetComponent<Animator>();

        joycon = JoyconManager.Instance.GetJoyconByPlayer(gameObject);

        maxGyroChange = Mathf.Exp(maxGyroChangeLog); //Calculate the maximum gyro change value based on the logarithmic value we defined earlier.
        speed = startingSpeed;

        if(isPlayback && playbackFile != null)
        {
            playbackData = new Queue<string>(playbackFile.text.Split(","));
            Debug.Log(playbackData.Count);
        }

        delta_t = Time.fixedDeltaTime / TIME_TO_100;
        Debug.Log(delta_t);

        setOpeningSpeed();

        raceManager = StateManager.instance.currentStateObject.GetComponent<RaceState>();

    }


    private void FixedUpdate()
    {

        UpdateJoyconEffort();
        UpdateRubberband();

        var estSpeed = raceManager.speedMax * (0.7f + joyconEffort + rubberbandBoost) * Time.fixedDeltaTime;

        Debug.Log($"{estSpeed}");

        if (currentSpeed < targetSpeed)
        {
            tImproved += delta_t;
            isAccelerating = true;
        }
        else if (currentSpeed > targetSpeed)
        {
            tImproved -= delta_t;
            isAccelerating = false;
        }
        tImproved = Mathf.Clamp(tImproved, 0, 1);
        currentSpeed = speedCurveImproved.Evaluate(tImproved) * 100f;
        finalSpeed = currentSpeed / 100f * raceManager.speedMax * Time.fixedDeltaTime;
        track.m_Speed = finalSpeed;
        animator.SetFloat("runningSpeed", finalSpeed);
        animator.SetBool("isAccelerating", isAccelerating);
        animator.SetFloat("runnerSpeed", raceManager.speedMax);
    }

    void UpdateJoyconEffort()
    {
        var speed = 0f;
        var tick = 0;
        if (isPlayback)
        {
            speed = float.Parse(playbackData.Dequeue());
        }
        else if (JoyconManager.Instance.GetJoyconByPlayer(gameObject) != null)
        {
            joycon = JoyconManager.Instance.GetJoyconByPlayer(gameObject);
            speed = Mathf.Abs(joycon.GetAccel().y);
        }
        else return;

        if (joyconInput.Count > AVG_WINDOW)
        {
            joyconInput.Dequeue();
        }
        joyconInput.Enqueue(speed);
        joyconEffort = joyconInput.Average() / EST_MAX * 0.3f;
    }

    void UpdateRubberband()
    {
        if (raceManager.firstPlace == this)
        {
            rubberbandBoost = 0;
            return;
        }

        var dist = Mathf.Abs(track.m_Position - raceManager.firstPlace.track.m_Position);
        rubberbandBoost = (dist / raceManager.maxRubberbandDistance) * raceManager.maxRubberbandBoost / 100f;
    }

    void setOpeningSpeed()
    {
        tImproved = 0.5f;
        currentSpeed = speedCurveImproved.Evaluate(tImproved);
    }


}
