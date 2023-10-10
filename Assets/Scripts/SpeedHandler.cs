using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

public class SpeedHandler : MonoBehaviour
{

    TextMeshProUGUI text;
    GameObject player;
    GameObject otherPlayer;
    bool isNum;
    FinishedHandler finished;
    // Start is called before the first frame update
    void Start()
    {

        player = (gameObject.transform.parent.name.Equals("P1Pos") || gameObject.transform.parent.parent.name.Equals("P1Pos")) ?
            GameObject.Find("Player1") : GameObject.Find("Player2");
        otherPlayer = (gameObject.transform.parent.name.Equals("P1Pos") || gameObject.transform.parent.parent.name.Equals("P1Pos")) ?
            GameObject.Find("Player2") : GameObject.Find("Player1");
        text = gameObject.GetComponent<TextMeshProUGUI>();
        isNum = gameObject.name.Contains("Num") ? true : false;
        text.text = isNum ? "1" : "st";
        if(gameObject.transform.parent.name.Contains("2") || gameObject.transform.parent.parent.name.Contains("2"))
        {
            text.text = isNum ? "2" : "nd";
        }
        finished = GameObject.Find("UI").transform.Find("Finish").GetComponent<FinishedHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!finished.isFinished)
        {
            if(System.Math.Abs(player.transform.position.x - otherPlayer.transform.position.x) > 0.001f) //Make sure they're not going to switch place every frame if they're really close
            {
                if(isNum)
                {
                    text.text = player.transform.position.x > otherPlayer.transform.position.x ? "1" : "2"; 
                }
                else
                {
                    text.text = player.transform.position.x > otherPlayer.transform.position.x ? "st" : "nd"; 
                    
                }

                float distanceDiff = player.transform.position.x - otherPlayer.transform.position.x;

                if (player.GetComponent<MovementHandler>().isFirstPlace == true && distanceDiff < 0)
                {
                    player.GetComponent<MovementHandler>().isFirstPlace = false;
                    player.GetComponent<MovementHandler>().ResetTimeInPlace();
                    otherPlayer.GetComponent<MovementHandler>().isFirstPlace = true;
                    otherPlayer.GetComponent<MovementHandler>().ResetTimeInPlace();
                }
                else if (player.GetComponent<MovementHandler>().isFirstPlace == false && distanceDiff > 0)
                {
                    player.GetComponent<MovementHandler>().isFirstPlace = true;
                    player.GetComponent<MovementHandler>().ResetTimeInPlace();
                    otherPlayer.GetComponent<MovementHandler>().isFirstPlace = false;
                    otherPlayer.GetComponent<MovementHandler>().ResetTimeInPlace();
                }
            }
        }
    }
}
