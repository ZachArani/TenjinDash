using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UILineRenderer = UnityEngine.UI.Extensions.UILineRenderer;
using System.Linq;
using Nito.Collections;

public class DrawMap : MonoBehaviour
{
    // Start is called before the first frame update

    NavMeshAgent agent;
    GameObject lineObject;
    LineRenderer line;
    public Canvas parentCanvas;
    public UILineRenderer lineUI;

    protected Deque<GameObject> goals = new Deque<GameObject>();
    Vector3[] bezierPoints;
    public Deque<Vector3> points = new Deque<Vector3>();

    public float lineThickness;

    public int numberOfPoints = 20;


    public GameObject[] mapPoints;
    public int currentGoalPos = 0;
    public int currentPathPoint = 0;
    NavMeshPath currentPath;
    const float X_OFFSET = -388.7f;
    const float Y_OFFSET = -90f;

    public float[] ratios = new float[] { 1, 1, 1, 1 };



    void Awake()
    {
        lineUI = gameObject.GetComponent<UILineRenderer>();

        GameObject[] unsortedGoals = GameObject.FindGameObjectsWithTag("MapGoal");
        
        foreach (GameObject goal in unsortedGoals)
        {
            goals.AddToBack(goal);
        }
        //Sort by order and coverts back into a Queue (yes, we have to do it this way)
        goals = new Deque<GameObject>(goals.OrderBy(i => i.transform.GetSiblingIndex()).ToList());


        RecalcPoints();
        

        lineUI.Points = new List<Vector2>().ToArray();

        if(mapPoints.Length == 2)
        {
            var pointList = new List<Vector2>(lineUI.Points);
            pointList.Add(new Vector2(mapPoints[0].transform.position.x, mapPoints[0].transform.position.z));
            pointList.Add(new Vector2(mapPoints[1].transform.position.x, mapPoints[0].transform.position.z));
            lineUI.Points = pointList.ToArray();
        }
        else
        {
           //Draw Map
        for(int i = 0; i < points.Count; i++)
        {
            var pointList = new List<Vector2>(lineUI.Points);
            pointList.Add(new Vector2(points.ElementAt(i).x, points.ElementAt(i).z));
            lineUI.Points = pointList.ToArray();
        }
        //Loop back around
        var finalList = new List<Vector2>(lineUI.Points);
        finalList.Add(finalList[0]);
        lineUI.Points = finalList.ToArray(); 
        }
        
        
        

        //gameObject.GetComponent<RectTransform>().localScale = new Vector3(2f, 2f, 1);
    }

    // Update is called once per frame
    void Update()
    {   

    }


    private Vector3 Bezier(float t, Vector3[] points, float[] ratios)
    {
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
    //Generates and returns a series of mapPoints of a bezier curve. Useful for making a clear line
    private Vector3[] generateBezierPoints(Vector3[] points, int numPoints)
    {
        //The mapPoints on the bezier curve we're returning
        Vector3[] bezierPoints = new Vector3[numPoints + 1];

        //How much we need to increase 't' between each point value
        float increment = 1f / numPoints;
        //Where we are currently between 0 and 1 in our incrementation of t
        float currentInc = 0.0f;

        //Iterate for each point we want to generate, calculate the bezier curve value at that 't' and then increment our current t value until we reach t=1
        for (int i = 0; i <= numPoints; i++)
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
            else if (iterGoal.name.Contains("L") || iterGoal.name.Contains("R"))
            {
                i++; //Just iterate, the curve point above will figure the actual path mapPoints
            }
            else
            {
                points.AddToBack(iterGoal.transform.position);
            }
        }
    }

}
