using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLogic : MonoBehaviour
{
    GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = gameObject.name.Contains("1") ? GameObject.Find("Player 1") : GameObject.Find("Player 2");
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = new Vector3(player.transform.position.x-5, player.transform.position.y+2, player.transform.position.z);
    }
}
