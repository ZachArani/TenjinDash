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
    public event Action<int> OnNewGyroData;

    public int playerNum;

    public bool isAuto;
    public bool isPlayback;

    Joycon joycon;


    //Time variables for gyro measurements
    /// <summary>
    /// Keeps track of the current sum of gyro data based on the time window
    /// </summary>
    float currentGyroAverage = 0f;
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

    /// <summary>
    /// The t value is used in calculating the player's current in-game speed. 
    /// The value decides the player's speed by referencing the acceleration curve defined. 
    /// So a t value of 0.7 means it will pick the Y value of the acceleration curve at the x==0.7 mark.
    /// The value is clamped between 0 and 1--i.e., 0% and 100%
    /// </summary>
    float t;
    bool isAccelerating;

    [SerializeField]
    [Range(0f, 10f)]
    float randomFakeSpeedRange = 3f;

    /// <summary>
    /// Used in creating fake data for demo/auto modes. Defines the distance traveled before a new random speed is calculated.
    /// </summary>
    [SerializeField]
    [Range(1f, 50f)]
    float distanceToNextRand = 5f;


    /// <summary>
    /// The running track this player races on.
    /// Defines exactly where they are running during the race.
    /// </summary>
    public CinemachineSmoothPath runningTrack;
    CinemachineDollyCart track;

    Animator animator;

    public TextAsset playbackFile;

    Queue<string> playbackData = new Queue<string>();




    /// <summary>
    /// Improvement
    /// </summary>

    Queue<float> speedAvg = new Queue<float>();
    const int AVG_WINDOW = 75;
    float max;
    const float EST_MAX = 2.5f;

    public TextMeshProUGUI debugText;
    public AnimationCurve speedCurveImproved;

    public float speedPercent;

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


    // Start is called before the first frame update
    void Start()
    {
        track = GetComponent<CinemachineDollyCart>();
        animator = GetComponent<Animator>();

        //joycon = JoyconManager.Instance.GetJoycon(gameObject);

        maxGyroChange = Mathf.Exp(maxGyroChangeLog); //Calculate the maximum gyro change value based on the logarithmic value we defined earlier.
        speed = startingSpeed;
        t = 0.05f; //Set at 0.05 right now to fix a bug. TODO: Allow t-value to run at 0.00

        if(Options.instance.isPlayback && playbackFile != null)
        {
            playbackData = new Queue<string>(playbackFile.text.Split(","));
            Debug.Log(playbackData.Count);
        }

        delta_t = Time.fixedDeltaTime / TIME_TO_100;
        Debug.Log(delta_t);

        setOpeningSpeed();

    }

    // Update is called once per frame
    void OldUpdate()
    {
        if (desiredSpeed > 0.1f && desiredSpeed + StateManager.instance.losingSpeedBoost > speed) //If the new gyro readings are faster then the current in-game speed
        {
            t += maxTChange * Time.deltaTime; //Increase t (speed up)
            isAccelerating = true;
        }
        else if (desiredSpeed + StateManager.instance.losingSpeedBoost < speed) //If the new gyro readings are slower than current in-game speed
        {
            t -= maxTChange * Time.deltaTime; //decrease t (slow down)
            isAccelerating = false;
        }
        Mathf.Clamp(t, 0f, 1f); //Clamp t between 0% and 100%
        speed = speedCurve.Evaluate(t); //Get new speed by evaluating t value on curve.
        speed = Mathf.Round(speed * 100f) / 100f;  //Truncate speed to 2 decimal places (X.XXXXXXXXX -> X.XX)
        track.m_Speed = speed; //Updates our actual movement along the running track. Tells the track the speed to advance at.

        //Update animation info
        animator.SetFloat("runningSpeed", speed);
        animator.SetBool("isAccelerating", isAccelerating);
    }

    private void FixedUpdate()
    {
        var speed = JoyconManager.Instance.j.Count > 0 ? Mathf.Abs(JoyconManager.Instance.j[0].GetAccel().y) : 0;
        if (speedAvg.Count > AVG_WINDOW)
        {
            speedAvg.Dequeue();
        }
        speedAvg.Enqueue(speed);
        speedPercent = speedAvg.Average() / EST_MAX;
        if (speedPercent > max) max = speedPercent;

        if (debugText != null)
        {
            debugText.text = speedPercent.ToString();
        }


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
        finalSpeed = currentSpeed / 100f * runnerSpeed * Time.fixedDeltaTime;
        track.m_Speed = finalSpeed;
        animator.SetFloat("runningSpeed", finalSpeed);
        animator.SetBool("isAccelerating", isAccelerating);
        animator.SetFloat("runnerSpeed", runnerSpeed);
    }

    /// <summary>
    /// time and gyro calculations done in FixedUpdate for reliable timings.
    /// </summary>
    private void OldFixedUpdate()
    {
        if (isAuto)
        {
            AutoSpeed(distanceToNextRand);
        }
        else if(Options.instance.isPlayback && playbackData.Count > 0)
        {
            Debug.Log(playbackData.Peek());
            currentGyroAverage += float.Parse(playbackData.Dequeue());
            OldCalcGyroData();
        }
        else
        {
            currentGyroAverage += joycon.GetGyro().magnitude;
            OldCalcGyroData();
        }
    }

    /// <summary>
    /// Measures Gyro data from Joycon.
    /// Uses this data to decide the speed the player should move at next step.
    /// </summary>
    private void OldCalcGyroData()
    {
        //Check if our current gyro reading period is within the timeWindow
        if (currentTimeInWindow < timeWindow)
        {
            currentTimeInWindow += Time.fixedDeltaTime;
        }
        else //If we're past the current window
        {
            currentTimeInWindow = 0;
            //Calculate the gyro percentage (current value vs max possible value) based on:
            //1) The current average of gyro samples taken during the window
            //2) The maximum gyro change as defined by the developer (meaning the new percentage can't change *too* much compared to the last one)
            //3) The correction factor as defined by the developer (subtracts a constant from the max gyro change)
            //4) The max gyro speed, which is the max possible value for gyro readings.
            gyroPercentage = Mathf.Clamp(Mathf.Lerp(gyroPercentage, currentGyroAverage, Mathf.Exp(-maxGyroChange * Time.deltaTime)) - correctionFactor,
                                        0,
                                        maxGyroSpeed) / maxGyroSpeed;
            desiredSpeed = gyroPercentage * maxRunnerSpeed; //New desired speed is based on % gyro readings (player running IRL) relative to player character's max speed
            currentGyroAverage = 0;
        }
    }

    void GetNewGyroData(Vector3 data)
    {
        currentGyroAverage += data.magnitude;
    }

    /// <summary>
    /// Fake movement for videos, etc.
    /// </summary>
    /// <param name="distanceToSpeedChange">How often to change speeds</param>
    void AutoSpeed(float distanceToSpeedChange)
    {
        if (GetComponent<CinemachineDollyCart>().m_Position % distanceToSpeedChange < 0.1) //If we've approached the required distance
        {
            float speedPush = UnityEngine.Random.Range(-randomFakeSpeedRange, randomFakeSpeedRange);
            desiredSpeed += speedPush;
            Debug.Log(desiredSpeed);
            desiredSpeed = Mathf.Clamp(desiredSpeed, 12, 14);
            //Debug.Log($"{gameObject.name}: {speedPush}, new Speed: {_desiredSpeed}");
        }
    }

    void setOpeningSpeed()
    {
        tImproved = 0.5f;
        currentSpeed = speedCurveImproved.Evaluate(tImproved);
    }


}
