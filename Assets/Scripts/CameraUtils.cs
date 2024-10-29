using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class CameraUtils : MonoBehaviour, ITimeControl
{

    public GameObject target;

    public float radius;

    [Range(0f, 5f)]
    public float speed;

    float circleTimer;

    bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isActive)
        {
            circleTimer = circleTimer > 6.283 ?
                0 : circleTimer + Time.deltaTime * speed;
            transform.position = new Vector3(Mathf.Sin(circleTimer) * radius,
                                            transform.position.y,
                                            Mathf.Cos(circleTimer) * radius);
            transform.LookAt(target.transform);
        }
    }



    public void OnControlTimeStart()
    {
        isActive = true;
    }



    public void OnControlTimeStop()
    {
        isActive = false;
    }

    public void SetTime(Double time)
    {

    }



}
