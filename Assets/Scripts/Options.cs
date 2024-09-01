using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls game options.
/// Basically just contains a bun of flags that can be referenced elsewhere.
/// </summary>
public class Options : MonoBehaviour
{

    public bool SkipMenu { get; private set; }
    public bool skipPreroll { get; private set; }
    public bool skipCountdown { get; private set; }

    public bool isAuto;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
