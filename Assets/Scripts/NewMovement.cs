using Assets.Scripts.FSM.States;
using Cinemachine;
using System;
using System.Collections.Generic;
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

    public CinemachineDollyCart runningTrack;

    Animator animator;

    public TextAsset playbackFile;
    public TextMeshProUGUI debugText;
    public AnimationCurve speedCurve;

    public bool isAuto;
    public bool isPlayback;

    Joycon joycon;


    bool isAccelerating;

    Queue<string> playbackData = new Queue<string>();

    Queue<float> joyconInput = new Queue<float>();
    const int AVG_WINDOW = 35;

    [Range(2f, 4f)]
    public float MAX_AVG = 3f;

    [Range(2f, 100f)]
    public float ZERO_TO_100 = 4f;

    [Range(0.01f, 1f)]
    public float epsilon = 0.1f;

    [ReadOnly]
    public float joyconEffort;

    [ReadOnly]
    public float targetSpeed;
    [ReadOnly]
    public float currentSpeed;

    [ReadOnly]
    public float t;
    
    [ReadOnly]
    public float delta_t;

    [ReadOnly]
    public float dist;

    [ReadOnly]
    public float rubberbandBoost;


    // Start is called before the first frame update
    void Start()
    {
        runningTrack = GetComponent<CinemachineDollyCart>();
        animator = GetComponent<Animator>();
        joycon = JoyconManager.Instance.GetJoyconByPlayer(gameObject);
        raceManager = StateManager.instance.stateDictionary[GAME_STATE.RACE].GetComponent<RaceState>();

        if(isPlayback && playbackFile != null)
        {
            playbackData = new Queue<string>(playbackFile.text.Split(","));
            Debug.Log(playbackData.Count);
        }

        delta_t = Time.fixedDeltaTime / ZERO_TO_100;

        setOpeningSpeed();

    }


    private void FixedUpdate()
    {

        UpdateJoyconEffort();
        UpdateRubberband();

        targetSpeed = raceManager.maxSpeed * (0.7f + joyconEffort + rubberbandBoost);

        if (currentSpeed < targetSpeed)
        {
            t += delta_t;
            isAccelerating = true;
        }
        else if (currentSpeed > targetSpeed)
        {
            t -= delta_t;
            isAccelerating = false;
        }
        t = Mathf.Clamp(t, 0, 1);
        currentSpeed = speedCurve.Evaluate(t) * raceManager.maxSpeed;
        runningTrack.m_Speed = currentSpeed;

        animator.SetFloat("runningSpeed", currentSpeed);
        animator.SetBool("isAccelerating", isAccelerating);
        animator.SetFloat("runnerSpeed", raceManager.maxSpeed);
    }

    void UpdateJoyconEffort()
    {
        var speed = 0f;
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
        rubberbandBoost = (dist / raceManager.maxRubberbandDistance) * raceManager.maxRubberbandBoost / 100f;
    }

    void setOpeningSpeed()
    {
        t = 0.5f;
        currentSpeed = speedCurve.Evaluate(t);
    }


}
