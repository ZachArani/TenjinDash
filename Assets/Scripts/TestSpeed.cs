using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpeed : MonoBehaviour
{

    /// <summary>
    /// Defines the player's acceleration curve.
    /// Different curves will affect how fast/slow the player speeds up/slows down.
    /// </summary>
    public AnimationCurve speedCurve;

    /// <summary>
    /// Speed the player *wants* to reach
    /// Based on newest data from JoyCon readings
    /// </summary>
    [Range(0f, 80f)]
    public float desiredSpeed = 0f;

    /// <summary>
    /// The speed the player character is *actually* running at
    /// Desired speed will shift this value over time to produce a smooth movement.
    /// </summary>
    public float currentSpeed = 0f;


    /// <summary>
    /// Maximum amount that the t value (used in the lerping equations for speed) can change
    /// Basically, defines how fast the character can accelerate
    /// If there wasn't a max cap to the t value changes, then a character might go from 0 to 100 in an instant.
    /// </summary>
    [Range(0f, 0.25f)]
    public float maxTChange = 0.02f;

    /// <summary>
    /// t value in lerp equations
    /// Controls exactly where we are along the animation curve that defines our acceleration.
    /// t = 0: start of curve
    /// t = 1: end of curve
    /// t = 0.5: middle of curve
    /// </summary>
    public float t = 0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(desiredSpeed > currentSpeed) //If we want to go faster
        {
            t += maxTChange * Time.deltaTime; //Add to our t value
        }
        else if(desiredSpeed < currentSpeed) //if we want to go slower
        {
            t -= maxTChange * Time.deltaTime; //Lower our t value 
        }
        Mathf.Clamp(t, 0, 1); //Force t to be between 0 and 1
        currentSpeed = speedCurve.Evaluate(t); //Convert t value into speed based on animation curve. 
    }
}
