using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;


public class NewMovement : MonoBehaviour
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
    public float maxGyroSpeed = 80f;

    [Range(5f, 100f)]
    public float maxRunnerSpeed;

    public Transform goalKeeper = null;
    Queue<Vector3> goals = new Queue<Vector3>();
    Vector3 nextGoal;

    float maxSpeedChange;
    public float speedPercentage { get; private set; }

    public float speed { get; private set; }

    [Range(0f, 1f)]
    public float SpeedSmoother = 0.5f;

    public float velocity;

    [Range(0f,2f)]
    public float smoothTime;


    // Start is called before the first frame update
    void Start()
    {
        j = JoyconManager.Instance.j[0];
        
        maxSpeedChange = Mathf.Exp(maxSpeedChangeLog);

        collectGoals();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, nextGoal, speed * Time.deltaTime);
        if(transform.position == nextGoal)
        {
            nextGoal = goals.Dequeue();
        }
    }

    private void FixedUpdate()
    {
        if (currentTime < timeWindow)
        {
            currentTime += Time.fixedDeltaTime;
        }
        else
        {
            currentTime = 0;
            speedPercentage = Mathf.Clamp(Mathf.Lerp(speedPercentage, avg, Mathf.Exp(-maxSpeedChange * Time.deltaTime)) - correctionFactor,
                                        0,
                                        maxGyroSpeed) / maxGyroSpeed;
            speed = Mathf.Lerp(speed, speedPercentage * maxRunnerSpeed, SpeedSmoother);
            if (speedUI != null)
            {
                speedUI.text = speed.ToString();
            }
            avg = 0;
        }
    }

    void GetNewGyroData()
    {
        //transform.position = j.GetGyro();
        avg += j.GetGyro().magnitude;
    }

    void collectGoals()
    {
        goals.Clear();
        foreach (Transform goal in goalKeeper)
        {
            goals.Enqueue(goal.position);
        }
        nextGoal = goals.Dequeue();
    }

    private void OnEnable()
    {
        Joycon.OnNewGyroData += GetNewGyroData;
        j = JoyconManager.Instance.j[0];
        collectGoals();
    }

    private void OnDisable()
    {
        Joycon.OnNewGyroData -= GetNewGyroData;
    }

    private void OnDestroy()
    {
        Joycon.OnNewGyroData -= GetNewGyroData;
    }
}
