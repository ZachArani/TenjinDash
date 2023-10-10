using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using Nito.Collections;

public class UpdatePlayerPos : MonoBehaviour
{
    // Start is called before the first frame update
    int lap = 1;
    NavMeshAgent agent;
    public GameObject currentGoal;
    public GameObject lastGoal;
    public GameObject[] goals;
    public int currentGoalPos = 0;
    public Deque<Vector3> points = new Deque<Vector3>();


    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        points = gameObject.GetComponent<NewAgent>().points;
    }

    // Update is called once per frame
    void Update()
    {
        if(points.Count > 2)
        {
            if (agent.remainingDistance < 0.5 && currentGoalPos < goals.Length-1)
            {
                lastGoal = currentGoal;
                currentGoal = goals[currentGoalPos + 1];
                //agent.destination = currentGoal.transform.position;
                currentGoalPos++;

            }
            else if(currentGoalPos == goals.Length - 1 && agent.remainingDistance < 0.5) //If we've reached the final goal, loop to the first one
            {
                lastGoal = currentGoal;
                currentGoal = goals[0];
                //agent.destination = currentGoal.transform.position;
                currentGoalPos++;
            }
            else if(currentGoalPos == goals.Length && agent.remainingDistance < 0.5) //If we've reach the start again
            {
               /// lap++;
                lastGoal = currentGoal;
                currentGoalPos = 1;
                currentGoal = goals[1];
                //agent.destination = currentGoal.transform.position;
               // Debug.Log("LAPPING!");
            }
        }
        else
        {
        }
        

    }
}
