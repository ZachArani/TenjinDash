using Assets.Scripts.FSM.States;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using static UtilFunctions;


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

    RaceState raceManager;

    CinemachineDollyCart runningTrack;

    Animator animator;

    public AnimationCurve accCurve;
    public AnimationCurve dccCurve;

    Joycon joycon;


    bool isAccelerating;

    Queue<float> playbackData = new Queue<float>();

    Queue<float> joyconInput = new Queue<float>();
    const int AVG_WINDOW = 35;

    [Range(2f, 4f)]
    public float MAX_AVG = 3f;

    [Range(0f, 0.2f)]
    public float effortFloor = 0.1f;

    [ReadOnly]
    public float joyconEffort;

    [ReadOnly]
    public float targetSpeed;
    [ReadOnly]
    public float currentSpeed;

    [ReadOnly]
    public float t;

    [Range(0f, 1f)]
    public float delta_t;

    [ReadOnly]
    public float dist;

    [ReadOnly]
    public float rubberbandBoost;

    public bool isPhotoFinish = false;

    public bool killswitch = false;

    [ReadOnly]
    public bool isStopping = false;

    [ReadOnly]
    public bool isPreMove = false;

    public float preMoveSpeed = 7f;


    // Start is called before the first frame update
    void Start()
    {
        runningTrack = GetComponent<CinemachineDollyCart>();
        animator = GetComponent<Animator>();
        joycon = JoyconManager.Instance.GetJoyconByPlayer(gameObject);
        raceManager = StateManager.instance.stateDictionary[GAME_STATE.RACE].GetComponent<RaceState>();

        if(Options.instance.isAuto)
        {
            var autoFile = raceManager.PickAutoFile();
            Debug.Log($"AUTO MODE ENABLED. READING DATA FROM {autoFile.name}");
            playbackData = new Queue<float>(Array.ConvertAll(autoFile.text.Split(",", StringSplitOptions.RemoveEmptyEntries), float.Parse));
        }

        SetOpeningSpeed();

    }

    private void Update()
    {
        double curveValue = 0f;

        UpdateJoyconEffort();
        UpdateRubberband();

        if (!isPhotoFinish && !isPreMove)
            targetSpeed = joyconEffort < effortFloor ? 0 : raceManager.maxSpeed * (0.7f + joyconEffort + rubberbandBoost);

        if (currentSpeed < targetSpeed)
        {
            t += (t + Time.deltaTime * delta_t) <= 1 ? Time.deltaTime * delta_t : 0;
            curveValue = Math.Round((double)accCurve.Evaluate(t), 4);
            if(curveValue > 0.95)
            {
                curveValue += 0.3 * rubberbandBoost;
            }
            isAccelerating = true;
            if (t > 0.2f)
            {
                isStopping = false;
            }
        }
        else if (currentSpeed > targetSpeed)
        {
            t -= (t - Time.deltaTime * delta_t >= 0) ? Time.deltaTime * delta_t : 0;
            isAccelerating = false;
            if(t >= 0.5f)
            {
                curveValue = Math.Round((double)accCurve.Evaluate(t), 4);
            }
            else
            {
                isStopping = true;
                curveValue = Math.Round((double)dccCurve.Evaluate(t), 4);
            }
        }
        currentSpeed = (float)(curveValue) * raceManager.maxSpeed;
        runningTrack.m_Speed = currentSpeed;

        animator.SetFloat("runningSpeed", currentSpeed);
        animator.SetBool("isAccelerating", isAccelerating);
        animator.SetFloat("runnerSpeed", raceManager.maxSpeed);
    }

    void UpdateJoyconEffort()
    {
        var speed = 0f;
        if (Options.instance.isAuto)
        {
            speed = playbackData.Dequeue();
            playbackData.Enqueue(speed); //Shitty circular queue interpretation. Need to loop data points.
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
        if (killswitch)
            speed = 0;
        joyconInput.Enqueue(speed);
        joyconEffort = joyconInput.Average() / MAX_AVG * 0.3f;
    }

    void UpdateRubberband()
    {
        if (raceManager.firstPlace == this)
        {
            rubberbandBoost = 0;
            return;
        }
        var dist = Mathf.Abs(runningTrack.m_Position - raceManager.firstPlace.runningTrack.m_Position);
        rubberbandBoost = (dist / raceManager.maxRubberbandDistance) * raceManager.rubberbandBoostMax / 100f + raceManager.rubberbandBonusCur;        
    }


    void SetOpeningSpeed()
    {
        t = 0.5f;
        currentSpeed = accCurve.Evaluate(t);
    }

    void StartPreMove()
    {
        targetSpeed = preMoveSpeed;
    }


}
