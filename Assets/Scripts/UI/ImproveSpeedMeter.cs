using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImproveSpeedMeter : MonoBehaviour
{

    /// <summary>
    /// WIP Asset that visually shows the current speed. 
    /// Basically just a rectangle that raises and lowers to fill the speed meter. 
    /// </summary>
    public RectTransform meterUI;

    /// <summary>
    /// Factor that affects how smooth the meter transitions from one speed to another. 
    /// Higher values: more smooth/more delay.
    /// Lower values: less smooth/more reactive.
    /// </summary>
    [Range(0f, 1f)]
    public float smoothFactor = 0.1f;

    /// <summary>
    /// Gets the current speed from the relevant player
    /// TODO: Encapsulate this better!
    /// </summary>
    public Improvement speedInfo;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //Lerp to new speed based on smoothing factor
        float newMeterValue = Mathf.Lerp(meterUI.offsetMax.y, -300 + 300 * (speedInfo.speedPercent), smoothFactor);
        meterUI.offsetMax = new Vector2(meterUI.offsetMax.x, newMeterValue); //Update UI
    }
}
