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
public class Improvement : MonoBehaviour
{

    Joycon j;

    Queue<float> speedAvg = new Queue<float>();
    const int AVG_WINDOW = 75;
    float max;
    const float EST_MAX = 2.5f;

    public TextMeshProUGUI debugText;
    public AnimationCurve speedCurve;

    public float speedPercent;

    [Range(5f,100f)]
    public float runnerSpeed;

    [Range(0f, 100f)]
    public float targetSpeed;
    [ReadOnly]
    public float currentSpeed;

    [ReadOnly]
    public float t;

    const float TIME_TO_100 = 4f;
    [ReadOnly]
    public float delta_t;

    public CinemachineDollyCart runnerTrack;

    [ReadOnly]
    public float dist;
    
    
    private void Start()
    {
        delta_t = Time.fixedDeltaTime / TIME_TO_100;
    }
    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        var speed = Mathf.Abs(JoyconManager.Instance.j[0].GetAccel().y);
        if(speedAvg.Count > AVG_WINDOW)
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

        Debug.Log(Time.fixedDeltaTime);

        if(currentSpeed < targetSpeed)
        {
            t += delta_t;
        }
        else if (currentSpeed > targetSpeed)
        {
            t -= delta_t;
        }
        currentSpeed = speedCurve.Evaluate(t) * 100f;
        t = Mathf.Clamp(t, 0, 1);
        dist = dist + t * runnerSpeed * Time.fixedDeltaTime;
        Debug.Log(dist);
    }


}
