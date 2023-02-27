using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class GenerateRoad : MonoBehaviour
{

    private GameObject MeshObject;
    private Vector3[] GeneratedPoints;
    private List<Vector3> convexHull;

    private void Awake()
    {
        MeshObject = GameObject.Find("MeshBounds");

        // Generate Points given the 2D Plane's bounds
        GeneratedPoints = GeneratePoints(10, MeshObject);

        convexHull = GenerateConvexHull(GeneratedPoints);
        foreach (Vector3 p in convexHull)
        {
            UnityEngine.Debug.Log(p);
        }

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        foreach(Vector3 point in GeneratedPoints)
        {
            Gizmos.color = UnityEngine.Color.blue;
            Gizmos.DrawCube(point, new Vector3((float)0.25, (float)0.25, (float)0.25));
        }

        foreach (Vector3 p in convexHull)
        {
            Gizmos.color = UnityEngine.Color.red;
            Gizmos.DrawCube(p, new Vector3((float)0.35, (float)0.35, (float)0.35));
        }

    }


    /// <summary>
    /// Provided a 2D Plane, will generate and return a list of Random points on this 2D Plane.
    /// </summary>
    /// <param name="numPoints">Number of points to generate</param>
    /// <param name="mesh">2D Plane to define the bounds</param>
    /// <returns>List of Generated Points bounded by Mesh</returns>
    Vector3[] GeneratePoints(int numPoints, GameObject mesh)
    {
        // Define the Bounds
        Vector3 minimum = mesh.GetComponent<MeshFilter>().sharedMesh.bounds.min;
        Vector3 maximum = mesh.GetComponent<MeshFilter>().sharedMesh.bounds.max;
        Vector3[] pointList = new Vector3[numPoints];


        for (int x = 0; x < numPoints; x++)
        {
            // Generate Point
            pointList[x] = new Vector3(UnityEngine.Random.Range(minimum.x, maximum.x), mesh.transform.position.y, UnityEngine.Random.Range(minimum.z, maximum.z));

            // Modify to accomdate any potential transfoms
            pointList[x] += MeshObject.transform.position;
        }

        return pointList;

    }
    /// <summary>
    /// Calculates the cross product
    /// </summary>
    /// <param name="currentPoint"></param>
    /// <param name="targetPoint"></param>
    /// <param name="checkedPoint"></param>
    /// <returns></returns>
    float CalculateCrossProduct(Vector3 currentPoint, Vector3 targetPoint, Vector3 checkedPoint) 
    {
        /*
         * Return Values:
         * Positive: Left of Line currentPoint->targetPoint
         * Negative: Right of Line currentPoint->targetPoint
         * Zero: Collinear
         */

        float z1 = currentPoint.z - targetPoint.z;
        float z2 = currentPoint.z - checkedPoint.z;
        float x1 = currentPoint.x - targetPoint.x;
        float x2 = currentPoint.x - checkedPoint.x;

        UnityEngine.Debug.Log($"z2:{z2}*x1:{x1}-z1:{z1}*x2:{x2}={(z2 * x1) - (z1 * x2)}");
        return (z2 * x1) - (z1 * x2);
    }

    /// <summary>
    /// Calculates the Convex Hull given a list of Points
    /// </summary>
    /// <param name="generatedPoints"></param>
    List<Vector3> GenerateConvexHull(Vector3[] generatedPoints)
    {
        // Build this list to return
        List<Vector3> convexHull = new List<Vector3>();
        Vector3 leftmostPoint = generatedPoints[0];

        // Look for the left most point to start from
        foreach (Vector3 p in generatedPoints) {
            // Checking to see if there's a vertex that is left of "leftmostPoint"
            if (leftmostPoint.x > p.x)
            {
                leftmostPoint = p;
            }
        }

        // Save startPoint to check later.
        //startPoint = leftmostPoint;

        // Found the first point, can start doing Jarvis March.
        convexHull.Add(leftmostPoint);

        // Loop until we finish building the Convex Hull, then break.
        for (int x = 0; x < generatedPoints.Length; x++)
        {
            /* Starting from the leftmostPoint, target the next vector in the list, calculate the CrossProduct with the next vector after that. If the Cross Product is positive,
             * then assign THAT vector as the targetVector.
             * Repeat for all other vectors until CrossProduct is negative for all
             */

            Vector3 targetPoint = generatedPoints[x];
            
            for (int y = 0; y < generatedPoints.Length; y++)
            {
                float crossProduct = CalculateCrossProduct(convexHull[convexHull.Count()-1], targetPoint, generatedPoints[y]);

                if (crossProduct > 0)
                {
                    // Found a poiunt that is on the left side of the line.
                    targetPoint = generatedPoints[y];
                }

            }

            if (targetPoint.Equals(convexHull.Last()))
            {
                // We have found the entirety of the Convex Hull.
                break;
            } else
            {
                // Found the next point to be added. 
                convexHull.Add(targetPoint);
            }
            
        }

        return convexHull;

    }

}
