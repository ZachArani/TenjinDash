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

    RaceState raceManager;

    public CinemachineDollyCart runningTrack;

    Animator animator;

    public TextAsset playbackFile;
    public TextMeshProUGUI debugText;
    public AnimationCurve speedCurveImproved;

    public bool isAuto;
    public bool isPlayback;

    Joycon joycon;

    /// <summary>
    /// The player's desired speed.
    /// Based on the most current gyro data. Indicates if the player should speed up or slow down from their current in-game speed.
    /// Can be manipulated by other classes. Namely the PhotoFinish state to handle a skipToPhotoFinish mode
    /// </summary>
    [ReadOnly]
    public float desiredSpeed;

    bool isAccelerating;

    Queue<string> playbackData = new Queue<string>();

    Queue<float> joyconInput = new Queue<float>();
    const int AVG_WINDOW = 35;
    const float EST_MAX = 3f;

    [ReadOnly]
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

        delta_t = Time.fixedDeltaTime / TIME_TO_100;
        Debug.Log(delta_t);

        setOpeningSpeed();

    }


    private void FixedUpdate()
    {

        UpdateJoyconEffort();
        UpdateRubberband();

        var estSpeed = raceManager.speedMax * (0.7f + joyconEffort + rubberbandBoost) * Time.fixedDeltaTime;
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
        //finalSpeed = currentSpeed / 100f * raceManager.speedMax * Time.fixedDeltaTime;
        finalSpeed = estSpeed;
        runningTrack.m_Speed = finalSpeed;
        animator.SetFloat("runningSpeed", finalSpeed);
        animator.SetBool("isAccelerating", isAccelerating);
        animator.SetFloat("runnerSpeed", raceManager.speedMax);
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
        joyconEffort = joyconInput.Average() / EST_MAX * 0.3f;
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
        tImproved = 0.5f;
        currentSpeed = speedCurveImproved.Evaluate(tImproved);
    }


}
