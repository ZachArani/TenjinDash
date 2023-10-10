using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using Nito.Collections;
using UnityEngine.EventSystems;

public class NewAgent : MonoBehaviour
{
    //TODO: Calculate and store all points at startup.
    //TODO: Restructure goals. Create regular goal AND curve goal objects. Tag with proper P1/P2 tags or consider other methods of 'marking' P1 vs P2 goals more dynamically
    protected Deque<GameObject> goals = new Deque<GameObject>();

    public NavMeshAgent agent;
    GameObject currentGoal;
    NavMeshPath curPath;
    public float faultTolerance;
    Vector3 lapPoint;
    public int MAX_LAPS = 3;

    public float weight = 1f;
    public int numberOfPoints = 20;

    public float[] ratios = new float[] {1,1,1,1};

    protected int nextCurvePoint = -1;

    Vector3[] bezierPoints;
    public Deque<Vector3> points = new Deque<Vector3>();
    public List<Vector3> pointsList = new List<Vector3>();
    public int currentListPoint = 0;

    public Vector3 currentPoint;
    public int currentLap = 1;


    // Start is called before the first frame update
    void Awake()
    {
        curPath = new NavMeshPath();
        faultTolerance = 1;
        //Collect all the goals
        GameObject[] unsortedGoals = gameObject.name.Contains("1") ? GameObject.FindGameObjectsWithTag("P1Goal") : GameObject.FindGameObjectsWithTag("P2Goal");
        foreach (GameObject goal in unsortedGoals)
        {
            goals.AddToBack(goal);
        }
        //Sort by order and coverts back into a Queue (yes, we have to do it this way)
        goals = new Deque<GameObject>(goals.OrderBy(i => i.transform.GetSiblingIndex()).ToList());


        RecalcPoints();
        lapPoint = points.ElementAt(points.Count - 1);

        agent = gameObject.GetComponent<NavMeshAgent>();
        //  currentGoal = goals.RemoveFromFront();
        //agent.destination = currentGoal.transform.position;
        //curPath = agent.path;

        pointsList = points.ToList();

        currentPoint = points.RemoveFromFront();

        foreach (GameObject goal in GameObject.FindGameObjectsWithTag("P1Goal"))
        {
            goal.GetComponent<Renderer>().enabled = false;

        }

        foreach (GameObject goal in GameObject.FindGameObjectsWithTag("P2Goal"))
        {
            goal.GetComponent<Renderer>().enabled = false;

        }

        foreach (GameObject goal in GameObject.FindGameObjectsWithTag("MapGoal"))
        {
            goal.GetComponent<Renderer>().enabled = false;

        }


    }

    // Update is called once per frame
    void Update()
    {

        if (agent.remainingDistance < faultTolerance && currentPoint != lapPoint)
        {
            points.AddToBack(currentPoint);
            currentPoint = points.RemoveFromFront();
            agent.CalculatePath(currentPoint, curPath);
            agent.path = curPath;
            currentListPoint++;
        }
        else if (agent.remainingDistance < faultTolerance && currentLap == MAX_LAPS && currentPoint == lapPoint)
        {
            ExecuteEvents.Execute<IFinishMessages>(GameObject.Find("Finish"), null, (x, y) => x.finishedRace());
            MAX_LAPS = 0; //Arbitrary sign that the race is over
            
        }
        else if(currentPoint == lapPoint && agent.remainingDistance < faultTolerance && MAX_LAPS != 0)
        {
            currentLap++;
            Debug.Log("LAPPED!");

            points.AddToBack(currentPoint);
            currentPoint = points.RemoveFromFront();
            agent.CalculatePath(currentPoint, curPath);
            agent.path = curPath;

            GameObject.Find(agent.gameObject.name.Contains("1") ? "P1Lap" : "P2Lap").GetComponent<LapHandler>().Lapped();
            currentListPoint = 0;

        }


       //RecalcPoints();

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Draw the bezier curve if we're turning corners

        Gizmos.DrawLine(currentPoint, points.ElementAt(0));

        foreach(GameObject goal in goals)
        {
            //Gizmos.DrawSphere(goal.transform.position, 1);
        }
        for(int i = 0; i<points.Count-1; i++)
        {
            Gizmos.DrawLine(points.ElementAt(i), points.ElementAt(i+1));
        }
        Gizmos.DrawLine(points.ElementAt(points.Count - 1), points.ElementAt(0));

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(currentPoint, 1);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(points.ElementAt(0), 1);

    }

    //Given a time reference t, and a series of points, and ratios for how weighted each point should be, this function calculates the point at time 't' on the cubic bezier curve.
    //So t=0.50 would return a point that's at the halfway point on the curve
    //If the ratios are [1,2,1], then the middle point will be weighted more towards how the curve turns out (the curve will trend towards the middle point)
    private Vector3 Bezier(float t, Vector3[] points, float[] ratios) {
        float t2 = t * t;
        float t3 = t2 * t;
        float mt = 1 - t;
        float mt2 = mt * mt;
        float mt3 = mt2 * mt;
        float[] f = points.Length == 4 ? new float[] //Assign weights depending on if we're doing a 3 point or 4 point bezier
        {
            ratios[0] * mt3,
            3 * ratios[1] * mt2 * t,
            3 * ratios[2] * mt * t2,
            ratios[3] * t3
        } : new float[] {
            ratios[0] * mt2,
            2 * ratios[1] * mt * t,
            ratios[2] * t2
        };
        float basis = f.Sum();
        if (points.Length == 4)
            return new Vector3(
                (f[0] * points[0].x + f[1] * points[1].x + f[2] * points[2].x + f[3] * points[3].x) / basis,
                (f[0] * points[0].y + f[1] * points[1].y + f[2] * points[2].y + f[3] * points[3].y) / basis,
                (f[0] * points[0].z + f[1] * points[1].z + f[2] * points[2].z + f[3] * points[3].z) / basis
                );
        else
            return new Vector3(
                (f[0] * points[0].x + f[1] * points[1].x + f[2] * points[2].x) / basis,
                (f[0] * points[0].y + f[1] * points[1].y + f[2] * points[2].y) / basis,
                (f[0] * points[0].z + f[1] * points[1].z + f[2] * points[2].z) / basis
                );
    }
    //Generates and returns a series of points of a bezier curve. Useful for making a clear line
    private Vector3[] generateBezierPoints(Vector3[] points, int numPoints)
    {
        //The points on the bezier curve we're returning
        Vector3[] bezierPoints = new Vector3[numPoints+1];
        
        //How much we need to increase 't' between each point value
        float increment = 1f / numPoints;
        //Where we are currently between 0 and 1 in our incrementation of t
        float currentInc = 0.0f;

        //Iterate for each point we want to generate, calculate the bezier curve value at that 't' and then increment our current t value until we reach t=1
        for(int i = 0; i<=numPoints; i++)
        {
            bezierPoints[i] = Bezier(currentInc, points, ratios);
            currentInc += increment;
        }

        return bezierPoints;
    }

    private void RecalcPoints()
    {
        points.Clear();
        for (int i = 0; i < goals.Count; i++)
        {
            GameObject iterGoal = goals.ElementAt(i);
            if (iterGoal.name.Contains("C"))
            {
                Vector3[] curveGeneratingPoints = new Vector3[]
                {
                            iterGoal.transform.position,
                            goals.ElementAt(i + 1).transform.position,
                            goals.ElementAt(i + 2).transform.position,
                            goals.ElementAt(i + 3).transform.position
                };
                Vector3[] calculatedCurve = generateBezierPoints(curveGeneratingPoints, numberOfPoints);
                foreach (Vector3 point in calculatedCurve)
                {
                    points.AddToBack(point);
                }
            }
            else if ((iterGoal.name.Contains("L") || iterGoal.name.Contains("R")) && !iterGoal.name.Contains("Lap"))
            {
                i++; //Just iterate, the curve point above will figure the actual path points
            }
            else
            {
                points.AddToBack(iterGoal.transform.position);
            }
        }
    }
}


