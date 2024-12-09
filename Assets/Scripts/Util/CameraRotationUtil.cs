using System;
using UnityEngine;
using UnityEngine.Timeline;

/// <summary>
/// Utilities for rotating camera.
/// </summary>
public class CameraRotationUtil : MonoBehaviour, ITimeControl
{

    /// <summary>
    /// Target the camera points at
    /// </summary>
    public GameObject pointTarget; 

    /// <summary>
    /// Radius from <see cref="pointTarget"/> the camera rotates at.
    /// </summary>
    public float rotationRadius;

    /// <summary>
    /// Speed at which the camera rotates
    /// </summary>
    [Range(0f, 100f)]
    public float rotationSpeed;

    bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            transform.RotateAround(pointTarget.transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
            transform.LookAt(pointTarget.transform);
        }
    }



    public void OnControlTimeStart()
    {
        transform.position = new Vector3(pointTarget.transform.position.x + rotationRadius,
                                         transform.position.y,
                                         pointTarget.transform.position.z + rotationRadius);
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
