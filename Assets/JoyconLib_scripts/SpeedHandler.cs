using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedHandler : MonoBehaviour
{

    GameObject player;
    TextMeshProUGUI text;
    JoyconDemo tracker;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        text = gameObject.GetComponent<TextMeshProUGUI>();
        text.text = "TEST";
        tracker = player.GetComponent<JoyconDemo>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = (tracker.rollingSumSpeed * 40).ToString("0");
    }
}
