using Assets.Scripts.FSM.States;
using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Controls player movement during race
/// The process is basically:
/// 1) Wait for new data from joycon
/// 2) Add new gyro data to queue of recent readings, removing the oldest data point if the queue has reached maximum capacity. 
/// 3) Once a specific period of time has elapsed, average all the joycon data into one average data point for calculations
/// 4) Update the player's 'desired' <see cref="targetSpeed"/> based on this new data
/// 5) Smoothly push the player's 'real' <see cref="currentSpeed"/> up or down based on their <see cref="targetSpeed"/>.
/// Handles speed changes based on a pre-defined <see cref="accCurve"/> and <see cref="dccCurve"/>.
/// <remarks>
/// In essence, we use a sliding window to average joycon input data over time. 
/// We take this averaged data and decide to move faster or slower based on it.
/// If faster, use <see cref="accCurve"/>; if slower use <see cref="dccCurve"/> to move smoothly. 
/// </remarks>
/// </summary>
public class NewMovement : MonoBehaviour
{
    /// <summary>
    /// Real world player number (e.g. p1, p2)
    /// </summary>
    public int playerNum;

    RaceState raceManager;

    CinemachineDollyCart runningTrack;

    Animator animator;

    /// <summary>
    /// Acceleration curve that defines the player speed when moving faster.
    /// </summary>
    public AnimationCurve accCurve;
    /// <summary>
    /// De-acceleration curve that defines the player's speed when moving slower.
    /// </summary>
    public AnimationCurve dccCurve;

    /// <summary>
    /// Joycon connected to this player.
    /// </summary>
    Joycon joycon;


    bool isAccelerating;

    /// <summary>
    /// Playback data used when reading from file
    /// </summary>
    Queue<float> playbackData = new Queue<float>();

    /// <summary>
    /// Joy-Con data stored in a queue.
    /// When queue is full, the oldest data point is removed.
    /// </summary>
    Queue<float> joyconInput = new Queue<float>();
    
    /// <summary>
    /// How many readings (<see cref="joyconInput"/> elements) we take for our data average. 
    /// Also serves as the size limit of <see cref="joyconInput"/>.
    /// </summary>
    const int AVG_WINDOW = 35;

    /// <summary>
    /// Maximum accepted value for our average reading.
    /// </summary>
    [Range(2f, 4f)]
    public float MAX_AVG = 3f;

    /// <summary>
    /// Minimum average value considered as 'movement.'
    /// Anything lower is written off as random noise and 'non movement.'
    /// </summary>
    [Range(0f, 0.2f)]
    public float effortFloor = 0.1f;

    /// <summary>
    /// Measure of how much 'effort' our player is putting into running.
    /// Basically, how much running the player is doing IRL.
    /// </summary>
    [ReadOnly]
    public float joyconEffort;

    /// <summary>
    /// The speed the player *wants* to go at. 
    /// Based on the newest data readings
    /// This value will shift sharply and rapidly with each new data average.
    /// So, we can't use this as our real speed value--that would cause the player to jitter around the screen.
    /// Instead, we use this value as a *goal* that <see cref="currentSpeed"/> attempts to reach.
    /// <example>
    /// Say, for instance, that <c>targetSpeed</c> suddenly goes from <c>0</c> (no movement) to values around <c>10</c>, like <c>9.1235, 11.235, 10.2135, 8.123, .../c>. 
    /// Then <see cref="currentSpeed"/> will smoothly ramp up from <c>0</c> to <c>10</c> GRADUALLY. 
    /// Even if the <c>targetSpeed</c> value jitters around wildly, <see cref="currentSpeed"/> will always approach that value smoothly.
    /// </example>
    /// </summary>
    [ReadOnly]
    public float targetSpeed;
    
    /// <summary>
    /// Current in-game speed. 
    /// The actual speed used to calculate physics/animations.
    /// I.e., the speed the player "sees."
    /// <seealso cref="targetSpeed"/>
    /// </summary>
    [ReadOnly]
    public float currentSpeed;

    /// <summary>
    /// Indicates our current place on the <see cref="accCurve"/> and <see cref="dccCurve"/> curves.
    /// <c>t=0</c> means no speed. <c>t=1.0</c> means max speed.
    /// <seealso href="https://en.wikipedia.org/wiki/Parametric_equation"/> for more details/
    /// </summary>
    [ReadOnly]
    public float t;

    /// <summary>
    /// How fast <see cref="t"/> is allowed to change in one frame.
    /// Lower values equal gradual shifts in speed. High values equal jittery, but fast shifts.
    /// </summary>
    [Range(0f, 1f)]
    public float delta_t;

    /// <summary>
    /// Extra speed boost given to the player based on how bad they're currently losing.
    /// Helps 'even the odds' for a comeback.
    /// <seealso cref="UpdateRubberband"/>
    /// </summary>
    [ReadOnly]
    public float rubberbandBoost;

    /// <summary>
    /// Extra speed boost given to a losing player based on how long they've been behind the winning player.
    /// Increases and decreases (when a losing player becomes a winning player) gradually.
    /// <seealso cref="RaceState.AddLoserBoost"/>, <seealso cref="RaceState.RemoveWinnerBoost"/>
    /// </summary>
    [ReadOnly]
    public float slipstream;

    /// <summary>
    /// Are we currently in a <see cref="PhotoFinishState"/>.
    /// </summary>
    public bool isPhotoFinish = false;

    /// <summary>
    /// Stops reading player data instantly. 
    /// Causes speed to drop to zero.
    /// Used for debugging purposes.
    /// </summary>
    public bool killswitch = false;

    /// <summary>
    /// Flag for if the player is currently slowing down to a stop.
    /// </summary>
    [ReadOnly]
    public bool isStopping = false;

    /// <summary>
    /// Flag for if the race has just started. If so, player data is temporarily ignored to create a 'smooth' start to the race.
    /// <seealso cref="RaceState.OpenRace"/>
    /// </summary>
    [ReadOnly]
    public bool isOpening = false;

    [ReadOnly]
    public float currentEffort = 0f;

    /// <summary>
    /// targetSpeed used in opening race
    /// </summary>
    public float openingTargetSpeed = 85f;


    // Start is called before the first frame update
    void Start()
    {
        runningTrack = GetComponent<CinemachineDollyCart>();
        animator = GetComponent<Animator>();
        joycon = JoyconManager.Instance.GetJoyconByPlayer(gameObject);
        raceManager = StateManager.instance.stateDictionary[GAME_STATE.RACE].GetComponent<RaceState>();

        if (Options.instance.isAuto)
        {
            var autoFile = raceManager.PickAutoFile();
            Debug.Log($"AUTO MODE ENABLED. {gameObject.name} READING DATA FROM {autoFile.name}");
            playbackData = new Queue<float>(Array.ConvertAll(autoFile.text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries), float.Parse));
        }

    }

    private void Update()
    {
        double curveValue = 0f;

        //Get newest data
        UpdateJoyconEffort();
        UpdateRubberband();

        //The primary speed equation.
        //Basically:
        //If the player is really running, they get 70% of the current maximum speed by default--which keeps players feeling 'fast' even if they're going slow
        //On top of that, add their calculated efforts and the rubberband boost to that 70%.
        //JoyConEffort is worth 30%, the rubberBand is basically 'extra credit.'
        //So a player running at 100% effort and is far behind, they will get 70% base + 30% 'effort' + 10% 'bonus' = 110% of the max speed
        //Meanwhile a winning player running at 50% effort will get 70% base + (30% possible * 50% current = ) 15% 'effort' + 0% bonus = 85% of the max speed.
        if (!isOpening)
            targetSpeed = joyconEffort < effortFloor ? 0 : raceManager.maxSpeed * (0.7f + joyconEffort + rubberbandBoost);
        
        //We need to speed up to match our target speed.
        if (currentSpeed < targetSpeed)
        {
            //Increase our t value based on delta_t. Cap at 1
            t = Mathf.Clamp(t + Time.deltaTime * delta_t, 0, 1);
            curveValue = Math.Round((double)accCurve.Evaluate(t), 4); 
            if (curveValue > 0.95)
            {
                curveValue += 0.3 * rubberbandBoost; //Edge case for some smoother speeds at higher ends.
            }
            isAccelerating = true;
            if (t > 0.2f)
            {
                isStopping = false;
            }
        }
        //We need to slow down!
        else if (currentSpeed > targetSpeed)
        {
            t = Mathf.Clamp(t - Time.deltaTime * delta_t, 0, 1); //Cap off value at 0.
            isAccelerating = false;
            if (t >= 0.5f)
            {
                curveValue = Math.Round((double)accCurve.Evaluate(t), 4);
            }
            else
            {
                isStopping = true;
                curveValue = Math.Round((double)dccCurve.Evaluate(t), 4);
            }
        }
        currentSpeed = (float)(curveValue) * raceManager.maxSpeed; //Set relative to current Max speed.
        runningTrack.m_Speed = currentSpeed; //This controls actual movement (via dolly track)

        animator.SetFloat("runningSpeed", currentSpeed);
        animator.SetBool("isAccelerating", isAccelerating);
        animator.SetFloat("runnerSpeed", raceManager.maxSpeed);
    }

    /// <summary>
    /// Get newest Joy-Con data. Update <see cref="joyconInput"/> and <see cref="joyconEffort"/>.
    /// Data reading is based on <c>Mathf.Abs(joycon.GetAccel().y)</c>
    /// </summary>
    void UpdateJoyconEffort()
    {
        var speed = 0f;
        if (Options.instance.isAuto)
        {
            speed = playbackData.Dequeue();
            playbackData.Enqueue(speed); //Shitty circular queue interpretation since we need to loop data points. Just add the old points back to the start.
        }
        else if (JoyconManager.Instance.GetJoyconByPlayer(gameObject) != null)
        {
            joycon = JoyconManager.Instance.GetJoyconByPlayer(gameObject);
            speed = Mathf.Abs(joycon.GetAccel().y);
            currentEffort = speed;
        }
        else return;

        if (joyconInput.Count > AVG_WINDOW)
        {
            joyconInput.Dequeue();
        }
        if (killswitch)
            speed = 0;
        joyconInput.Enqueue(speed);
        joyconEffort = joyconInput.Average() / MAX_AVG * 0.3f;
    }

    /// <summary>
    /// Calculates the player's current Rubberband bonus values.
    /// </summary>
    void UpdateRubberband()
    {
        if (raceManager.firstPlace == this)
        {
            rubberbandBoost = 0;
            return;
        }
        var dist = Mathf.Abs(runningTrack.m_Position - raceManager.firstPlace.runningTrack.m_Position);
        rubberbandBoost = (dist / raceManager.maxRubberbandDistance) * raceManager.rubberbandBoostMax / 100f + slipstream;
    }

    /// <summary>
    /// Util method other objects use when <see cref="isOpening"/> is true.
    /// Other classes may need to do math based on <see cref="targetSpeed"/> even when its not being updated accurately here.
    /// </summary>
    /// <returns></returns>
    public float CalcTargetSpeed()
    {
        return joyconEffort < effortFloor ? 0 : raceManager.maxSpeed * (0.7f + joyconEffort + rubberbandBoost);
    }

    public void CleanUp()
    {
        currentSpeed = 0;
        targetSpeed = openingTargetSpeed;
        joyconEffort = 0;
        t = 0;
        isPhotoFinish = false;
        rubberbandBoost = 0;
        slipstream = 0;
        isOpening = true;

        var dolly = GetComponent<CinemachineDollyCart>();
        transform.position = dolly.m_Path.transform.position;
        dolly.m_Position = 0;
        dolly.m_Speed = 0;
    }



}
