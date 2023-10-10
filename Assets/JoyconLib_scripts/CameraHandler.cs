using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime;
using Math = System.Math;
using System.Linq;
using UnityEngine.AI;
public class CameraHandler : MonoBehaviour
{
    List<GameObject> cameraGoals;
    int currentGoal;
    NavMeshAgent agent;
    public GameObject player1;
    public GameObject player2;
    void Start()
    {
        currentGoal = 0;
        cameraGoals = new List<GameObject>();
        for(int i = 0; i < GameObject.Find("Goals").transform.GetChild(2).transform.childCount; i++) //Get each child in order and add them to the list
        {
            cameraGoals.Add(GameObject.Find("Goals").transform.GetChild(2).transform.GetChild(i).gameObject);
        }
        agent = gameObject.GetComponent<NavMeshAgent>();//GetComponentInParent<NavMeshAgent>();
        agent.SetDestination(cameraGoals[currentGoal].transform.position); //Set intial target to first one
        agent.autoBraking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentGoal == cameraGoals.Count) //if we're at the end
        {
            agent.speed = 0; //stop moving the camera
        }
        else if(!agent.pathPending && agent.remainingDistance < 2) //If we're almost at the goal
        {
            currentGoal += 1;
            agent.SetDestination(cameraGoals[currentGoal].transform.position);
        }
        else //Just find the speed we need to run at
        {
            agent.speed = player1.GetComponent<NavMeshAgent>().speed < player2.GetComponent<NavMeshAgent>().speed ? //Get speed of camera based on whoever's the slowest runner right now
                        player1.GetComponent<NavMeshAgent>().speed : player2.GetComponent<NavMeshAgent>().speed;
        }
        
    }
}
