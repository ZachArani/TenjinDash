using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpeed : MonoBehaviour
{
    // Start is called before the first frame update

    public AnimationCurve speedCurve;

    [Range(0f, 80f)]
    public float desiredSpeed = 0f;

    public float currentSpeed = 0f;

    [Range(0f, 0.25f)]
    public float maxTChange = 0.02f;

    public float t = 0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(desiredSpeed > currentSpeed)
        {
            t += maxTChange * Time.deltaTime;
        }
        else if(desiredSpeed < currentSpeed)
        {
            t -= maxTChange * Time.deltaTime;
        }
        Mathf.Clamp(t, 0, 1);
        currentSpeed = speedCurve.Evaluate(t);
    }
}
