using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedHandler : MonoBehaviour
{

    GameObject player;
    Text text;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        text = gameObject.GetComponent<Text>();
        Debug.Log("Testing speed text");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
