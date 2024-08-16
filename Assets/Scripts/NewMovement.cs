using Cinemachine;
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
   
    //Time variables for gyro measurements
    float avg = 0f;
    List<float> avgRuns;
    [Range(0.05f, 1f)]
    public float timeWindow = 0.1f;
    float currentTime;
    //Gyro measurements
    [Range(0f, 5f)]
    public float maxGyroChangeLog = 3f;
    float maxGyroChange;
    [Range(0f, 10f)]
    public float correctionFactor = 3;
    [Range(50f, 100f)]
    public float maxGyroSpeed = 80f;
    public float gyroPercentage { get; private set; }

    //Player's speed info
    [Range(5f, 30f)]
    public float maxRunnerSpeed;
    public float speed { get; private set; }
    [SerializeField]
    [Range(0f, 25f)]
    float startingSpeed;
    public AnimationCurve speedCurve;
    [Range(0f, 25f)]
    public float desiredSpeed;
    [Range(0f, 0.4f)]
    public float maxTChange;
    float t;
    bool isAccelerating;

    [SerializeField]
    [Range(0f, 10f)]
    float randomFakeRange = 3f;

    [SerializeField]
    [Range(1f, 50f)]
    float randomFakeDistance = 5f;

    [ReadOnly]
    public float losingSpeedBoost = 0f;

    //Running track
    public CinemachineSmoothPath runningTrack;
    CinemachineDollyCart track;

    Animator animator;


    // Start is called before the first frame update
    void Start()
    {
        track = GetComponent<CinemachineDollyCart>();
        animator = GetComponent<Animator>();
        
        maxGyroChange = Mathf.Exp(maxGyroChangeLog);
        speed = startingSpeed;
        t = 0.05f;
        j = JoyconManager.Instance.j[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (desiredSpeed + losingSpeedBoost > speed)
        {
            t += maxTChange * Time.deltaTime;
            isAccelerating = true;
        }
        else if (desiredSpeed + losingSpeedBoost < speed)
        {
            t -= maxTChange * Time.deltaTime;
            isAccelerating = false;
        }
        Mathf.Clamp(t, 0f, 1f);
        speed = speedCurve.Evaluate(t);
        speed = Mathf.Round(speed * 100f) / 100f; 
        if (speedUI != null && speedUI.isActiveAndEnabled)
        {
            speedUI.text = speed.ToString();
        }

        FakeItTillYaMakeIt(randomFakeDistance);

        track.m_Speed = speed;

        animator.SetFloat("runningSpeed", speed);
        animator.SetBool("isAccelerating", isAccelerating);
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
            gyroPercentage = Mathf.Clamp(Mathf.Lerp(gyroPercentage, avg, Mathf.Exp(-maxGyroChange * Time.deltaTime)) - correctionFactor,
                                        0,
                                        maxGyroSpeed) / maxGyroSpeed;
            //speed = Mathf.Lerp(speed, speedPercentage * maxRunnerSpeed, SpeedSmoother);
            //desiredSpeed = speedPercentage * maxRunnerSpeed;
            avg = 0;
        }
    }

    void GetNewGyroData()
    {
        //transform.position = j.GetGyro();
        avg += j.GetGyro().magnitude;
    }

    /// <summary>
    /// Fake movement for videos, etc.
    /// </summary>
    /// <param name="distance">How often to change speeds</param>
    void FakeItTillYaMakeIt(float distance)
    {
        if (GetComponent<CinemachineDollyCart>().m_Position % distance < 0.1)
        {
            float speedPush = UnityEngine.Random.Range(-randomFakeRange, randomFakeRange);
            desiredSpeed += speedPush;
            Mathf.Clamp(desiredSpeed, 8, 15);
            Debug.Log($"{gameObject.name}: {speedPush}, new Speed: {desiredSpeed}");
        }
    }


    private void OnEnable()
    {
        Joycon.OnNewGyroData += GetNewGyroData;
        j = JoyconManager.Instance.j[0];
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
