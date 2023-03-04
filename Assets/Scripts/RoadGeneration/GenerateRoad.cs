using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GenerateRoad : NetworkBehaviour
{

    struct GeneratedPointsStruct : INetworkSerializable
    {
        public Vector3[] points;

        // INetworkSerializable
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // Length
            int length = 0;
            if (!serializer.IsReader)
            {
                length = points.Length;
            }

            serializer.SerializeValue(ref length);

            // Array
            if (serializer.IsReader)
            {
                points = new Vector3[length];
            }

            for (int n = 0; n < length; ++n)
            {
                serializer.SerializeValue(ref points[n]);
            }
        }
        // ~INetworkSerializable
    }

    public GameObject MeshObject;

    [SerializeField]
    private GameObject RoadMesh;

    [SerializeField]
    private GameObject Goal;

    [SerializeField]
    private GameObject Gate;

    [SerializeField]
    private bool trackExists = false;

    // Reference to another Script (TrackInfo.cs)
    private TrackInfo TrackManager;

    private NetworkVariable<GeneratedPointsStruct> GeneratedPoints = new NetworkVariable<GeneratedPointsStruct>();
    public List<Vector3> convexHull;

    private void Awake()
    {
        MeshObject = GameObject.Find("MeshBounds");

        TrackManager = GameObject.Find("TrackManager").GetComponent<TrackInfo>();
    }

    // Was start method
    public void InitializeTrack()
    {

        if (!IsOwner)
        {
            RequestPointsServerRPC();
            return;
        }

        // if (GeneratedPoints.Value.points != null) generateTrack();
        Debug.Log("Generating points");
        Vector3[] points = GeneratePoints(12, MeshObject);
        GeneratePointsServerRPC(points);

    }

    void generateTrack()
    {
        // Remove Duplicates!
        Debug.Log("Generating track");
        convexHull = GenerateConvexHull(GeneratedPoints.Value.points).Distinct().ToList();

        UnityEngine.Debug.Log("Length: " + convexHull.Count);

        TrackManager.goal = Instantiate(Goal, convexHull.First(), Quaternion.identity);

        double y = 0;
        while (y < 1)
        {
            for (int x = 0; x < convexHull.Count; x++)
            {

                if (convexHull[x] != convexHull.Last())
                {
                    Instantiate(RoadMesh, Vector3.Lerp(convexHull[x], convexHull[x + 1], (float)y), Quaternion.identity);
                    TrackManager.triggers.Add(Instantiate(Gate, Vector3.Lerp(convexHull[x], convexHull[x + 1], (float)y), Quaternion.identity));
                    //Gizmos.DrawCube(Vector3.Lerp(convexHull[x], convexHull[x + 1], (float)y), new Vector3((float)0.25, (float)0.25, (float)0.25));
                }
                else
                {
                    Instantiate(RoadMesh, Vector3.Lerp(convexHull.Last(), convexHull.First(), (float)y), Quaternion.identity);
                    TrackManager.triggers.Add(Instantiate(Gate, Vector3.Lerp(convexHull.Last(), convexHull.First(), (float)y), Quaternion.identity));
                    //Gizmos.DrawCube(Vector3.Lerp(convexHull.Last(), convexHull.First(), (float)y), new Vector3((float)0.25, (float)0.25, (float)0.25));
                }
            }

            y += .01;
        }
        trackExists = true;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        foreach (Vector3 point in GeneratedPoints.Value.points)
        {
            Gizmos.color = UnityEngine.Color.blue;
            Gizmos.DrawCube(point, new Vector3((float)0.25, (float)0.25, (float)0.25));
        }

        foreach (Vector3 p in convexHull)
        {
            Gizmos.color = UnityEngine.Color.red;
            Gizmos.DrawCube(p, new Vector3((float)0.35, (float)0.35, (float)0.35));
        }

        for (int x = 0; x < convexHull.Count(); x++)
        {
            Gizmos.color = UnityEngine.Color.green;
            if (x < convexHull.Count - 1)
            {
                Gizmos.DrawLine(convexHull[x], convexHull[x + 1]);
            }
            else
            {
                Gizmos.color = UnityEngine.Color.yellow;
                Gizmos.DrawLine(convexHull[convexHull.Count - 1], convexHull[0]);
            }
        }

        Gizmos.color = UnityEngine.Color.yellow;
        double y = 0;
        while (y < 1)
        {

            for (int x = 0; x < convexHull.Count; x++)
            {

                if (convexHull[x] != convexHull.Last())
                {
                    Gizmos.DrawCube(Vector3.Lerp(convexHull[x], convexHull[x + 1], (float)y), new Vector3((float)0.25, (float)0.25, (float)0.25));
                }
                else
                {
                    Gizmos.DrawCube(Vector3.Lerp(convexHull.Last(), convexHull.First(), (float)y), new Vector3((float)0.25, (float)0.25, (float)0.25));
                }
            }

            y += .05;
        }

    }

    [ServerRpc]
    public void GeneratePointsServerRPC(Vector3[] points)
    {
        Debug.Log("Server points");
        // Generate Points given the 2D Plane's bounds
        GeneratedPoints.Value = new GeneratedPointsStruct { points = points };
        SetPointsClientRPC(points);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPointsServerRPC()
    {
        Debug.Log("Request points");
        SetPointsClientRPC(GeneratedPoints.Value.points);
    }

    [ClientRpc]
    public void SetPointsClientRPC(Vector3[] points)
    {
        Debug.Log("Client points");
        // Generate Points given the 2D Plane's bounds
        GeneratedPoints.Value = new GeneratedPointsStruct { points = points };
        if(!trackExists) generateTrack();
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
            pointList[x] = new Vector3(UnityEngine.Random.Range(minimum.x, maximum.x), 0, UnityEngine.Random.Range(minimum.z, maximum.z));

            // Modify to accomdate any potential transfoms
            pointList[x] += MeshObject.transform.position;
            pointList[x].Scale(MeshObject.transform.localScale);
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

        //UnityEngine.Debug.Log($"z2:{z2}*x1:{x1}-z1:{z1}*x2:{x2}={(z2 * x1) - (z1 * x2)}");
        return (z2 * x1) - (z1 * x2);
    }

    /// <summary>
    /// Calculates the Convex Hull given a list of Points
    /// </summary>
    /// <param name="generatedPoints"></param>
    List<Vector3> GenerateConvexHull(Vector3[] generatedPoints)
    {
        // Build this list to return
        List<Vector3> convexHullList = new List<Vector3>();
        Vector3 leftmostPoint = generatedPoints[0];

        // Look for the left most point to start from
        foreach (Vector3 p in generatedPoints)
        {
            // Checking to see if there's a vertex that is left of "leftmostPoint"
            if (leftmostPoint.x > p.x)
            {
                leftmostPoint = p;
            }
        }

        // Save startPoint to check later.
        //startPoint = leftmostPoint;

        // Found the first point, can start doing Jarvis March.
        convexHullList.Add(leftmostPoint);

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
                float crossProduct = CalculateCrossProduct(convexHullList.Last(), targetPoint, generatedPoints[y]);

                if (crossProduct > 0)
                {
                    // Found a poiunt that is on the left side of the line.
                    targetPoint = generatedPoints[y];
                }

            }

            if (targetPoint.Equals(convexHullList.Last()))
            {
                // We have found the entirety of the Convex Hull.
                break;
            }
            else
            {
                // Found the next point to be added. 
                convexHullList.Add(targetPoint);
            }

        }

        return convexHullList;

    }

    Vector3 Render4PTBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float time)
    {
        // Linearly interpolate between all the points
        Vector3 v1 = Vector3.Lerp(p0, p1, time);
        Vector3 v2 = Vector3.Lerp(p1, p2, time);
        Vector3 v3 = Vector3.Lerp(p2, p3, time);

        // Linerly interpolate between these three points
        Vector3 v4 = Vector3.Lerp(v1, v2, time);
        Vector3 v5 = Vector3.Lerp(v2, v3, time);

        // Linearly interpolate between these two points
        Vector3 finalVector = Vector3.Lerp(v4, v5, time);

        return finalVector;

    }

    Vector3 Render3PTBezier(Vector3 p0, Vector3 p1, Vector3 p2, float time)
    {
        // Linearly interpolate between all the points
        Vector3 v1 = Vector3.Lerp(p0, p1, time);
        Vector3 v2 = Vector3.Lerp(p1, p2, time);

        // Linerly interpolate between these two points
        Vector3 finalVector = Vector3.Lerp(v1, v2, time);

        return finalVector;

    }

    void draw4PTCurve(int x, float y)
    {
        Gizmos.DrawCube(Render4PTBezier(convexHull[x], convexHull[x + 1], convexHull[x + 2], convexHull[x + 3], (float)y), new Vector3((float)0.25, (float)0.25, (float)0.25));
    }

    void draw3PTCurve(int x, float y)
    {
        Gizmos.DrawCube(Render3PTBezier(convexHull[x], convexHull[x + 1], convexHull[x + 2], (float)y), new Vector3((float)0.25, (float)0.25, (float)0.25));
    }

    void draw3PTLastCurve(int x, float y)
    {
        Gizmos.DrawCube(Render3PTBezier(convexHull[x], convexHull[x + 1], convexHull.Last(), (float)y), new Vector3((float)0.25, (float)0.25, (float)0.25));
    }

    float calcAngle(Vector3 previousVector, Vector3 nextVector)
    {
        float rise = nextVector.z - previousVector.z;
        float run = nextVector.x - previousVector.x;

        UnityEngine.Debug.Log(UnityEngine.Mathf.Tan(rise / run));

        return UnityEngine.Mathf.Tan(rise / run);
    }

}
