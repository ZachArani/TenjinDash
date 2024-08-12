using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;


public class GyroTest : MonoBehaviour
{
    Joycon j;
    public TextMeshProUGUI speedUI;
    float avg = 0f;
    List<float> avgRuns;

    [Range(0.05f, 1f)]
    public float timeWindow = 0.1f;
    
    float currentTime;
    [Range(0f, 5f)]
    public float maxSpeedChangeLog = 3f;

    [Range(0f, 10f)]
    public float correctionFactor = 3;

    [Range(50f, 150f)]
    public float maxInputSpeed = 80f;

    [Range(5f, 100f)]
    public float maxRunnerVelocity = 10f;

    float highestRecordedSpeed;

    float maxSpeedChange;
    public float currentSpeed { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        j = JoyconManager.Instance.j[0];
        maxSpeedChange = Mathf.Exp(maxSpeedChangeLog);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (currentTime < timeWindow)
        {
            currentTime += Time.fixedDeltaTime;
        }
        else
        {
            if(avg > highestRecordedSpeed)
            {
                highestRecordedSpeed = avg;
                Debug.Log($"New Highest Speed: {highestRecordedSpeed}");
            }
            currentTime = 0;
            currentSpeed = Mathf.Clamp(Mathf.Lerp(currentSpeed, avg, Mathf.Exp(-maxSpeedChange * Time.deltaTime))-correctionFactor, 
                                        0, 
                                        maxInputSpeed) / maxInputSpeed * 100f;
            speedUI.text = currentSpeed.ToString();
            avg = 0;
        }
    }

    void GetNewGyroData()
    {
        //transform.position = j.GetGyro();
        avg += j.GetGyro().magnitude;
    }

    private void OnEnable()
    {
        Joycon.OnNewGyroData += GetNewGyroData;
    }

    private void OnDisable()
    {
        Joycon.OnNewGyroData -= GetNewGyroData;
    }
}
