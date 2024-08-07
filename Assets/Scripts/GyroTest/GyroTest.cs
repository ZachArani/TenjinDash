using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GyroTest : MonoBehaviour
{
    Joycon j;
    public TextMeshProUGUI Meter;
    public TextMeshProUGUI RunsUI;
    string uiText;

    float avg;
    List<float> avgRuns;

    public float timeWindow;
    float currentTime;
    // Start is called before the first frame update
    void Start()
    {
        j = JoyconManager.Instance.j[0];
        avgRuns = new List<float>();
        uiText = "Runs:\n";
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
            
            currentTime = 0;
            avgRuns.Add(avg);
            uiText += $"{avg}\n";
            RunsUI.text = uiText;
            avg = 0;
        }
    }

    void GetNewGyroData()
    {
        //transform.position = j.GetGyro();
        avg += j.GetGyro().magnitude;
        Meter.text = avg.ToString();
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
