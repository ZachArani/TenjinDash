using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

public class LapHandler : MonoBehaviour
{

    TextMeshProUGUI text;
    NavMeshAgent tracker;
    public GameObject player;
    FinishedHandler finished;
    int lap = 1;
    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<TextMeshProUGUI>();
        text.text = "1/3";
        tracker = player.GetComponent<NavMeshAgent>();
        finished = GameObject.Find("Countdown").GetComponent<FinishedHandler>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Lapped()
    {
        lap++;
        text.text = $"{lap}/3";
    }
}
