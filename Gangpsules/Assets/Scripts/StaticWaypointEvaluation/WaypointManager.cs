using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class WaypointManager : MonoBehaviour
{

    [SerializeField] LayerMask obstaclesLayer;
    public List<Waypoint> waypoints = new List<Waypoint>();
    public Graph graph = new Graph();
    public bool toggleBetweenWaypointLinksAndLineOfSightGizmos;

    NSizeBitMatrix lineOfSightMatrix;
    float maxDistanceBetweenTwoWaypoints;
    public WaypointEvaluation waypointEvaluation;

    private void Awake()
    {
        if (waypoints.Count > 0)
        {
            graph.InitializeGraph();
            waypoints.Sort();
            foreach (Waypoint wp in waypoints)
            {
                graph.AddNode(wp);
            }

            foreach (Waypoint wp in waypoints)
            {
                foreach (int id in wp.linksToOtherWaypoints)
                {
                    Waypoint wp2 = waypoints.First(t => t.ID == id);
                    graph.AddEdge(wp, wp2);
                }
            }
            lineOfSightMatrix = new NSizeBitMatrix(waypoints.Count);
        }
        SetUpLinesOfSight();
        SetExposureOfAllWaypoints();
        SetExposureRatioOfAllWaypoints();
        waypointEvaluation.ClearAllLists();
        waypointEvaluation.FindHighestExposureWaypoints(waypoints);
        waypointEvaluation.FindBestWaypoints(waypoints, lineOfSightMatrix);
        waypointEvaluation.FindAllPinchPoints(graph, waypoints);
        waypointEvaluation.FindGoodAmbushLocationsForAllPinchPoints(lineOfSightMatrix,waypoints);
    }

    [ContextMenu("Find max distance")]
    void FindMaxDistance()
    {
        float maxDistanceSqr = graph.MaxDistance();
        maxDistanceBetweenTwoWaypoints = Mathf.Sqrt(maxDistanceSqr);
    }

    [ContextMenu("Set up lines of sight")]
    void SetUpLinesOfSight()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            bool[] linesOfSight = new bool[waypoints.Count];
            for (int j = 0; j < waypoints.Count; j++)
            {
                if (i == j)
                {
                    linesOfSight[j] = true;
                }
                else
                {
                    Vector3 start = new Vector3(waypoints[i].transform.position.x, 1, waypoints[i].transform.position.z);
                    Vector3 end = new Vector3(waypoints[j].transform.position.x, 1, waypoints[j].transform.position.z);
                    Ray ray = new Ray(start, (end - start).normalized);
                    float distance = Vector3.Distance(start, end);
                    linesOfSight[j] = !Physics.Raycast(ray, distance, obstaclesLayer);
                }
            }
            lineOfSightMatrix.SetBitArray(i, linesOfSight);
        }
    }

    void SetExposureOfAllWaypoints()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            waypoints[i].exposure = lineOfSightMatrix.GetNumberOfTrueValues(i);
        }
    }

    void SetExposureRatioOfAllWaypoints()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            int minimumExposure = int.MaxValue;
            for (int j = 0; j < waypoints[i].linksToOtherWaypoints.Length; j++)
            {
                int neighboorExposure = waypoints[waypoints[i].linksToOtherWaypoints[j]].exposure;
                if (neighboorExposure < minimumExposure)
                    minimumExposure = neighboorExposure;
            }
            waypoints[i].exposureRatio = minimumExposure / waypoints[i].exposure;
        }
    }

    [ContextMenu("Debug Print line of sight matrix")]
    void PrintLineOfSightMatrix()
    {
        lineOfSightMatrix.PrintValues();
    }

    private void OnDrawGizmosSelected()
    {
        if (toggleBetweenWaypointLinksAndLineOfSightGizmos)
            graph.DebugDraw();
        else
            DrawLinesOfSight();
    }

    void DrawLinesOfSight()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < waypoints.Count; i++)
        {
            BitArray lineOfSight = lineOfSightMatrix.GetRow(i);
            Vector3 start = waypoints[i].transform.position;
            for (int j = 0; j < lineOfSight.Count; j++)
            {
                if (i == j) continue;
                if (lineOfSight[j] == true)
                {
                    Vector3 end = waypoints[j].transform.position;
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }

    public bool WaypointAHasNeighboorWithoutLineOfSightToWaypointB(int indexA, int indexB)
    {
        foreach (var wpIndex in waypoints[indexA].linksToOtherWaypoints)
        {
            if (lineOfSightMatrix.GetValue(wpIndex, indexB) == false)
                return true;
        }
        return false;
    }
    public bool WaypointAHasNeighboorWithoutLineOfSightToWaypointB(Waypoint wpA, Waypoint wpB)
    {
        return WaypointAHasNeighboorWithoutLineOfSightToWaypointB(wpA.ID, wpB.ID);
    }

    public bool WaypointAHasLineOfSightToWaypointB(int indexA,int indexB)
    {
        return lineOfSightMatrix.GetValue(indexA, indexB);
    }

    public bool WaypointAHasLineOfSightToWaypointB(Waypoint wpA, Waypoint wpB)
    {
        return WaypointAHasLineOfSightToWaypointB(wpA.ID, wpB.ID);
    }

    public List<Waypoint> GetWaypointsWithLineOfSightToAWaypoint(int index)
    {
        List<Waypoint> wps = new List<Waypoint>();
        BitArray los = lineOfSightMatrix.GetCollumn(index);
        for (int i = 0; i < los.Length; i++)
        {
            if (los[i] == true)
                wps.Add(waypoints[i]);
        }

        return wps;
    }

    public List<Waypoint> GetWaypointsWithoutLineOfSightToAWaypoint(Waypoint wp)
    {
        return GetWaypointsWithoutLineOfSightToAWaypoint(wp.ID);
    }

    public List<Waypoint> GetWaypointsWithoutLineOfSightToAWaypoint(int index)
    {
        List<Waypoint> wps = new List<Waypoint>();
        BitArray los = lineOfSightMatrix.GetCollumn(index);
        for (int i = 0; i < los.Length; i++)
        {
            if (los[i] == false)
                wps.Add(waypoints[i]);
        }

        return wps;
    }

    public List<Waypoint> GetWaypointsWithLineOfSightToAWaypoint(Waypoint wp)
    {
        return GetWaypointsWithLineOfSightToAWaypoint(wp.ID);
    }
}



public class NSizeBitMatrix
{
    public int Order { get; private set; }

    BitArray[] matrix;

    public NSizeBitMatrix(int size)
    {
        if (size < 0) size = 0;

        Order = size;
        matrix = new BitArray[Order];
        for (int i = 0; i < matrix.Length; i++)
        {
            matrix[i] = new BitArray(Order);
        }
    }

    public BitArray GetRow(int index)
    {
        if (index < 0 || index >= Order)
            index = 0;
        BitArray row = new BitArray(Order);
        for (int i = 0; i < matrix.Length; i++)
        {
            row[i] = matrix[index][i];
        }
        return row;
    }

    public BitArray GetCollumn(int index)
    {
        if (index < 0 || index >= Order)
            index = 0;
        BitArray collumn = new BitArray(Order);
        for (int i = 0; i < Order; i++)
        {
            collumn[i] = matrix[i][index];
        }
        return collumn;
    }

    public bool GetValue(int rowIndex,int collumnIndex)
    {
        return matrix[rowIndex][collumnIndex];
    }

    public void SetBitArray(int index, bool[] boolArray)
    {
        if (boolArray.Length != Order) return;
        if (index < 0 || index >= Order) return;
        for (int i = 0; i < Order; i++)
        {
            matrix[index][i] = boolArray[i];
        }
    }

    public int GetNumberOfTrueValues(int index)
    {
        int values = 0;
        for (int i = 0; i < matrix[index].Count; i++)
        {
            if (matrix[index][i] == true)
                values++;
        }
        return values;
    }

    public void PrintValues()
    {
        for (int i = 0; i < matrix.Length; i++)
        {
            string line = "";
            line = PrintRow(i);
            Debug.Log(line);
        }
    }

    public string PrintRow(int index)
    {
        string line = "";
        for (int j = 0; j < matrix[index].Count; j++)
        {
            line += matrix[index][j];
            line += " ";
        }
        return line;
    }
}