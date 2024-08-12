using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedMeter : MonoBehaviour
{

    public RectTransform meterUI;

    [Range(0f, 1f)]
    public float smoothFactor = 0.1f;

    public NewMovement speedInfo;
    // Start is called before the first frame update
    void Start()
    {
    }

    private void FixedUpdate()
    {
        float newMeterValue = Mathf.Lerp(meterUI.offsetMax.y, -300 + 300 * (speedInfo.speedPercentage), smoothFactor);
        meterUI.offsetMax = new Vector2(-4, newMeterValue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
