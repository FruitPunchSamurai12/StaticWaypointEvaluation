using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class WaypointEvaluation
{
    public int highestExposureLevels = 3;
    public int bestWaypointsNumber = 5;
    public int multiplePinchPoints = 3;

    public ListOfWaypointsWithCertainExposure[] highestExposureWaypoints;
    public int[] bestWaypoints;

    public List<PinchPoint> pinchPoints;
    public List<PinchPointPair> pinchPointPairs;

    struct WaypointNeighboor
    {
        public int ID;
        public int previousID;

        public WaypointNeighboor(int iD, int previousID)
        {
            ID = iD;
            this.previousID = previousID;
        }
    };

    public void ClearAllLists()
    {
        pinchPoints.Clear();
        pinchPointPairs.Clear();
    }

    public void FindHighestExposureWaypoints(List<Waypoint> waypoints)
    {
        int[] highestExposures = new int[highestExposureLevels];
        for (int i = 0; i < waypoints.Count; i++)
        {
            int exposuresNumber = waypoints[i].exposure;

            for (int j = 0; j < highestExposures.Length; j++)
            {
                if(exposuresNumber>=highestExposures[j])
                {
                    highestExposures[j] = exposuresNumber;
                    break;
                }
            }
        }
        highestExposureWaypoints = new ListOfWaypointsWithCertainExposure[highestExposureLevels];
        for (int i = 0; i < highestExposureWaypoints.Length; i++)
        {
            highestExposureWaypoints[i].exposure = highestExposures[i];
            highestExposureWaypoints[i].IDs = new List<int>();
            for (int j = 0; j < waypoints.Count; j++)
            {
                if (waypoints[j].exposure == highestExposureWaypoints[i].exposure)
                    highestExposureWaypoints[i].IDs.Add(waypoints[j].ID);
            }
        }
    }

  

    public void FindBestWaypoints(List<Waypoint> waypoints, NSizeBitMatrix lineOfSightMatrix)
    {
        //FIND ALL NEIGHBOORING WAYPOINTS TO THE HIGHEST EXPOSURE WAYPOINTS
        int numberOfHighestExposureWaypoints = 0;

        List<WaypointNeighboor> neighbooringWaypoints = new List<WaypointNeighboor>();
        for (int i = 0; i < highestExposureWaypoints.Length; i++)
        {
            numberOfHighestExposureWaypoints += highestExposureWaypoints[i].IDs.Count;
            for (int j = 0; j < highestExposureWaypoints[i].IDs.Count; j++)
            {
                Waypoint wp = waypoints[highestExposureWaypoints[i].IDs[j]];
                for (int k = 0; k < wp.linksToOtherWaypoints.Length; k++)
                {
                    int neighboorID = wp.linksToOtherWaypoints[k];
                    WaypointNeighboor neighboor = new WaypointNeighboor(neighboorID, wp.ID);
                    neighbooringWaypoints.Add(neighboor);
                }
            }
        }

        //SORT THE NEIGHBOORS BY THE EXPOSURE RATIO WHICH IS NEIGHBOOREXPOSURE/EXPOSURE
        int sortIndex = 0;
        while(sortIndex<neighbooringWaypoints.Count)
        {
            float minExposureRatio = float.MaxValue;
            int minExposureRatioIndex = 0;
            for (int i = sortIndex; i < neighbooringWaypoints.Count; i++)
            {
                int exposure = lineOfSightMatrix.GetNumberOfTrueValues(neighbooringWaypoints[i].previousID);
                int neighboorExposure = lineOfSightMatrix.GetNumberOfTrueValues(neighbooringWaypoints[i].ID);
                float exposureRatio = (float)neighboorExposure / (float)exposure;
                if(exposureRatio< minExposureRatio)
                {
                    minExposureRatio = exposureRatio;
                    minExposureRatioIndex = i;
                }
            }
            var temp = neighbooringWaypoints[sortIndex];
            neighbooringWaypoints[sortIndex] = neighbooringWaypoints[minExposureRatioIndex];
            neighbooringWaypoints[minExposureRatioIndex] = temp;
            sortIndex++;
        }       

        //ASSIGN THE FIRST UNIQUE IDS AS THE BEST WAYPOINTS
        bestWaypoints = new int[Mathf.Min(numberOfHighestExposureWaypoints, bestWaypointsNumber)];

        int n = 0;
        int m = 0;
        while (n < bestWaypoints.Length)
        {
            bool alreadyExists = false;
            int ID = neighbooringWaypoints[m].previousID;
            foreach (var wp in bestWaypoints)
            {
                if (wp == ID)
                {
                    alreadyExists = true;
                    break;
                }
            }
            if(!alreadyExists)
            {
                bestWaypoints[n] = ID;
                n++;
            }
            m++;
        }

    }

    public void FindAllPinchPoints(Graph graph, List<Waypoint> waypoints)
    {
        pinchPoints = new List<PinchPoint>();
        pinchPointPairs = new List<PinchPointPair>();
        foreach (var wp in waypoints)
        {
            if (wp.linksToOtherWaypoints.Length == 1)
            {
                if (waypoints[wp.linksToOtherWaypoints[0]].linksToOtherWaypoints.Length == 2)
                    continue;
                int outsideID = wp.linksToOtherWaypoints[0];
                if (waypoints[outsideID].linksToOtherWaypoints.Length > 2)
                {
                    List<int> insideId = new List<int>();
                    insideId.Add(wp.ID);
                    PinchPoint pinchPoint = new PinchPoint(wp.ID, insideId, outsideID);
                    AddPinchPointToListAfterCheckingForDuplicates(pinchPoint);
                }
            }
            else if (wp.linksToOtherWaypoints.Length == 2)
            {
                Waypoint A = waypoints[wp.linksToOtherWaypoints[0]];
                Waypoint B = waypoints[wp.linksToOtherWaypoints[1]];
                List<Waypoint> excluded = new List<Waypoint>();
                excluded.Add(wp);
                if (!graph.AStar(A, B, excluded))
                {
                    var pinchPoint = CreatePinchPoint(graph, wp, A, B, excluded, waypoints);
                    if (pinchPoint != null)
                        AddPinchPointToListAfterCheckingForDuplicates(pinchPoint);
                }
                else
                {
                    foreach (var node in graph.GetPath())
                    {
                        if (node.edgelist.Count == 2)
                        {
                            excluded.Add(node.wp);
                            if (!graph.AStar(A, B, excluded))
                            {
                                var n1 = CreatePinchPoint(graph, wp, A, B, excluded, waypoints);
                                var n2 = CreatePinchPoint(graph, node.wp, node.edgelist[0].endNode.wp, node.edgelist[1].endNode.wp, excluded, waypoints);
                                if (n1 != null && n2 != null)
                                {
                                    if (n1.ID != n2.InsideIDs.First() && n1.ID != n2.OutsideID && n2.ID != n1.InsideIDs.First() && n2.ID != n1.OutsideID)
                                    {
                                        if (!CheckForDuplicatePinchPointPairs(n1, n2))
                                        {
                                            var pinchPointPair = new PinchPointPair(n1, n2);
                                            List<Waypoint> newExcluded = new List<Waypoint> { waypoints[n1.ID], waypoints[n2.ID] };
                                            var insideWaypoints = graph.GetRegionNodes(waypoints[n1.InsideIDs.First()], newExcluded);
                                            pinchPointPair.insideWaypoints = new List<int>();
                                            foreach (var n in insideWaypoints)
                                            {
                                                pinchPointPair.insideWaypoints.Add(n.wp.ID);
                                            }
                                            pinchPointPairs.Add(pinchPointPair);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
    }

    private PinchPoint CreatePinchPoint(Graph graph, Waypoint wp, Waypoint A, Waypoint B, List<Waypoint> excluded,List<Waypoint> allWaypoints,bool findEndOfHallway = true)
    {
        int ARegion = graph.GetRegionWaypointCount(A, excluded);
        int BRegion = graph.GetRegionWaypointCount(B, excluded);
        if (ARegion == 0 || BRegion == 0)
            return null;
        int insideID = ARegion > BRegion ? B.ID : A.ID;
        int outsideID = ARegion > BRegion ? A.ID : B.ID;
        int pinchPointID = wp.ID;
        List<int> insideIDs = new List<int>();
        if (findEndOfHallway)
        {
            while (allWaypoints[outsideID].linksToOtherWaypoints.Length == 2)
            {
                int temp = allWaypoints[outsideID].linksToOtherWaypoints.First(t => t != pinchPointID);
                pinchPointID = outsideID;
                outsideID = temp;
            }

            foreach (var node in graph.GetRegionNodes(allWaypoints[insideID], new List<Waypoint> { allWaypoints[pinchPointID] }))
            {
                insideIDs.Add(node.wp.ID);
            }
        }
        else
        {
            insideIDs.Add(insideID);
        }
        PinchPoint pinchPoint = new PinchPoint(pinchPointID, insideIDs, outsideID);
        return pinchPoint;
    }

    void AddPinchPointToListAfterCheckingForDuplicates(PinchPoint N)
    {
        foreach (var pinchPoint in pinchPoints)
        {
            if (N.OutsideID == pinchPoint.OutsideID)
                return;
        }
        pinchPoints.Add(N);
    }

    bool CheckForDuplicatePinchPointPairs(PinchPoint n1,PinchPoint n2)
    {
        if (n1.ID == n2.ID)
            return true;
        foreach (var pair in pinchPointPairs)
        {
            if ((pair.n1.ID == n1.ID && pair.n2.ID == n2.ID) || (pair.n1.ID == n2.ID && pair.n2.ID == n1.ID))
                return true;
            foreach (var id in pair.insideWaypoints)
            {
                if (id == n1.ID || id == n2.ID)
                    return true;
            }
        }
        return false;
    }

    public void FindGoodAmbushLocationsForAllPinchPoints(NSizeBitMatrix lineOfSightMatrix,List<Waypoint> waypoints)
    {
        for (int i = 0; i < pinchPoints.Count; i++)
        {
            pinchPoints[i].anglesToOutsideIDFromAmbushPoints = new List<AmbushIDsWithAngleToAmbushPoint>();
            BitArray linesOfSightToOutside = lineOfSightMatrix.GetCollumn(pinchPoints[i].OutsideID);
            BitArray linesOfSightOfPinchPoint = lineOfSightMatrix.GetRow(pinchPoints[i].ID);
            BitArray ambushLocations = linesOfSightToOutside.And(linesOfSightOfPinchPoint.Not());
            for (int j = 0; j < ambushLocations.Count; j++)
            {
                if (ambushLocations[j] == true)
                {
                    SetUpAmbushLocationWithAngleToOutside(waypoints,pinchPoints[i] , j);
                }
            }
        }
        for (int i = 0; i < pinchPointPairs.Count; i++)
        {
            pinchPointPairs[i].n1.anglesToOutsideIDFromAmbushPoints = new List<AmbushIDsWithAngleToAmbushPoint>();
            pinchPointPairs[i].n2.anglesToOutsideIDFromAmbushPoints = new List<AmbushIDsWithAngleToAmbushPoint>();
            BitArray linesOfSightToOutside1 = lineOfSightMatrix.GetCollumn(pinchPointPairs[i].n1.OutsideID);
            BitArray linesOfSightToOutside2 = lineOfSightMatrix.GetCollumn(pinchPointPairs[i].n2.OutsideID);
            BitArray noLinesOfSightOfPinchPoint1 = lineOfSightMatrix.GetRow(pinchPointPairs[i].n1.ID).Not();
            BitArray noLinesOfSightOfPinchPoint2 = lineOfSightMatrix.GetRow(pinchPointPairs[i].n2.ID).Not();
            BitArray ambushLocations1 = linesOfSightToOutside1.And(noLinesOfSightOfPinchPoint1.And(noLinesOfSightOfPinchPoint2));
            BitArray ambushLocations2 = linesOfSightToOutside2.And(noLinesOfSightOfPinchPoint1.And(noLinesOfSightOfPinchPoint2));
            for (int j = 0; j < ambushLocations1.Count; j++)
            {
                if (ambushLocations1[j] == true)
                    SetUpAmbushLocationWithAngleToOutside(waypoints, pinchPointPairs[i].n1, j);
            }
            for (int j = 0; j < ambushLocations2.Count; j++)
            {
                if (ambushLocations2[j] == true)
                    SetUpAmbushLocationWithAngleToOutside(waypoints, pinchPointPairs[i].n2, j);
            }
        }
    }

    private void SetUpAmbushLocationWithAngleToOutside(List<Waypoint> waypoints, PinchPoint pinchPoint, int ambushLocationID)
    {
        float angle = Vector3.SignedAngle(waypoints[pinchPoint.OutsideID].transform.position - waypoints[ambushLocationID].transform.position, Vector3.forward, Vector3.up);
        bool duble = false;
        foreach (var thing in pinchPoint.anglesToOutsideIDFromAmbushPoints)
        {
            float ang = thing.angle;
            if (ang * angle > 0)
            {
                if (Mathf.Abs(ang - angle) < 10)
                {
                    duble = true;
                    thing.ambushPointIDs.Add(ambushLocationID);
                    break;
                }
            }

        }
        if (!duble)
        {
            List<int> ambushID = new List<int>();
            ambushID.Add(ambushLocationID);
            var newAngle = new AmbushIDsWithAngleToAmbushPoint(angle);
            newAngle.ambushPointIDs = ambushID;
            pinchPoint.anglesToOutsideIDFromAmbushPoints.Add(newAngle);
        }
    }

    public List<Waypoint> OrderWaypointsByExposureRatio(List<Waypoint> waypoints)
    {
        return waypoints.OrderBy(t => t.exposureRatio).ToList();
    }


    public PinchPoint IsWaypointAPinchPoint(int waypoint)
    {
        foreach (var pinchPoint in pinchPoints)
        {
            if(waypoint == pinchPoint.ID)
            {
                return pinchPoint;
            }
            foreach (var insidePoint in pinchPoint.InsideIDs)
            {
                if (waypoint == insidePoint)
                    return pinchPoint;
            }
        }

        return null;
    }

    public PinchPointPair IsWaypointAPinchPointPair(int waypoint)
    {
        foreach (var ppPair in pinchPointPairs)
        {
            if (waypoint == ppPair.n1.ID || waypoint == ppPair.n2.ID)
                return ppPair;
            foreach (var insidePoint in ppPair.insideWaypoints)
            {
                if (waypoint == insidePoint)
                    return ppPair;
            }
        }
        return null;
    }
}

[System.Serializable]
public struct ListOfWaypointsWithCertainExposure:IComparable<ListOfWaypointsWithCertainExposure>
{
    public List<int> IDs;
    public int exposure;

    public int CompareTo(ListOfWaypointsWithCertainExposure other)
    {
        return exposure.CompareTo(other.exposure);
    }
}

[System.Serializable]
public class PinchPoint
{
    public int ID;
    public List<int> InsideIDs;
    public int OutsideID;
    public List<AmbushIDsWithAngleToAmbushPoint> anglesToOutsideIDFromAmbushPoints;
    
    public PinchPoint(int iD, List<int> insideIDs, int outsideID)
    {
        ID = iD;
        InsideIDs = insideIDs;
        OutsideID = outsideID;
    }
}

[System.Serializable]
public class PinchPointPair
{
    public PinchPoint n1;
    public PinchPoint n2;
    public List<int> insideWaypoints;
    public PinchPointPair(PinchPoint n1, PinchPoint n2)
    {
        this.n1 = n1;
        this.n2 = n2;
    }
}

[System.Serializable]
public class AmbushIDsWithAngleToAmbushPoint
{
    public float angle;
    public List<int> ambushPointIDs;

    public AmbushIDsWithAngleToAmbushPoint(float angle)
    {
        this.angle = angle;
    }
}