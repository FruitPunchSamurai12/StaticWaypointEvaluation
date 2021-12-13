using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Graph
{
    List<Edge> edges;
    List<Node> nodes;
    List<Node> pathList;
	


	public Graph(){}
	
	public void AddNode(Waypoint wp)
	{
		Node node = new Node(wp);
		nodes.Add(node);
		
	}
	
	public void AddEdge(Waypoint fromNode, Waypoint toNode)
	{
		Node from = FindNode(fromNode.ID);
		Node to = FindNode(toNode.ID);
		
		if(from != null && to != null)
		{
			Edge e = new Edge(from, to);
			edges.Add(e);
			from.edgelist.Add(e);
		}	
	}
	
	Node FindNode(int id)
	{
		foreach (Node n in nodes) 
		{
			if(n.GetId() == id)
				return n;
		}
		return null;
	}
	
	
	public int GetPathLength()
	{
		return pathList.Count;	
	}
	
	public Waypoint GetPathPoint(int index)
	{
		return pathList[index].wp;
	}
	
	public void PrintPath()
	{
		foreach(Node n in pathList)
		{	
			Debug.Log(n.wp.ID);	
		}
	}
	
	public List<Node> GetPath()
    {
        return pathList;
    }

    public void InitializeGraph()
    {
        nodes = new List<Node>();
        edges = new List<Edge>();
        pathList = new List<Node>();
    }

	public bool AStar(Waypoint startWP, Waypoint endWP,List<Waypoint> excludedWaypoints =null)
	{
	  	Node start = FindNode(startWP.ID);
	  	Node end = FindNode(endWP.ID);
        List<Node> excluded;
        if (excludedWaypoints == null)
            excluded = null;
        else
        {
            excluded = new List<Node>();
            foreach (var wp in excludedWaypoints)
            {
                excluded.Add(FindNode(wp.ID));
            }
        }
	  	if(start == null || end == null)
	  	{
	  		return false;	
	  	}
	  	
	  	List<Node>	open = new List<Node>();
	  	List<Node>	closed = new List<Node>();
	  	float tentative_g_score= 0;
	  	bool tentative_is_better;
	  	
	  	start.g = 0;
	  	start.h = Distance(start,end);
	  	start.f = start.h;
	  	open.Add(start);
	  	
	  	while(open.Count > 0)
	  	{
	  		int i = LowestF(open);
			Node thisnode = open[i];
			if(thisnode.GetId() == endWP.ID)  //path found
			{
                ReconstructPath(start, end);
				return true;	
			} 	
			
			open.RemoveAt(i);
			closed.Add(thisnode);
			
			Node neighbour;
			foreach(Edge e in thisnode.edgelist)
			{
                bool excludedWaypoint = false;
                if (excluded != null)
                {
                    foreach (var n in excluded)
                    {
                        if (e.startNode == n || e.endNode == n)
                        {
                            excludedWaypoint = true;
                            break;
                        }
                    }
                }
                if(excludedWaypoint)
                    continue;

                neighbour = e.endNode;
				neighbour.g = thisnode.g + Distance(thisnode,neighbour);
				
				if (closed.IndexOf(neighbour) > -1)
					continue;
				
				tentative_g_score = thisnode.g + Distance(thisnode, neighbour);
				
				if( open.IndexOf(neighbour) == -1 )
				{
					open.Add(neighbour);
					tentative_is_better = true;	
				}
				else if (tentative_g_score < neighbour.g)
				{
					tentative_is_better = true;	
				}
				else
					tentative_is_better = false;
					
				if(tentative_is_better)
				{
					neighbour.cameFrom = thisnode;
					neighbour.g = tentative_g_score;
					neighbour.h = Distance(thisnode,end);
					neighbour.f = neighbour.g + neighbour.h;	
				}
			}
  	
	  	}
		
		return false;	
	}
	
	public void ReconstructPath(Node startId, Node endId)
	{
		pathList.Clear();
		pathList.Add(endId);
		
		var p = endId.cameFrom;
		while(p != startId && p != null)
		{
			pathList.Insert(0,p);
			p = p.cameFrom;	
		}
		pathList.Insert(0,startId);
	}

	
    float Distance(Node a, Node b)
    {
	  float dx = a.xPos - b.xPos;
	  float dy = a.yPos - b.yPos;
	  float dz = a.zPos - b.zPos;
	  float dist = dx*dx + dy*dy + dz*dz;
	  return( dist );
    }

    public float MaxDistance()
    {
        float maxDistance = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;
                float distance = Distance(nodes[i], nodes[j]);
                if (distance > maxDistance)
                    maxDistance = distance;
            }
        }

        return maxDistance;
    }

    int LowestF(List<Node> l)
    {
	  float lowestf = 0;
	  int count = 0;
	  int iteratorCount = 0;
	  	  
	  for (int i = 0; i < l.Count; i++)
	  {
	  	if(i == 0)
	  	{	
	  		lowestf = l[i].f;
	  		iteratorCount = count;
	  	}
	  	else if( l[i].f <= lowestf )
	  	{
	  		lowestf = l[i].f;
	  		iteratorCount = count;	
	  	}
	  	count++;
	  }
	  return iteratorCount;
    }
    
    public int GetRegionWaypointCount(Waypoint start, List<Waypoint> excludedWaypoints = null)
    {
        return GetRegionNodes(start, excludedWaypoints).Count;
    }

    public HashSet<Node> GetRegionNodes(Waypoint start, List<Waypoint> excludedWaypoints = null)
    {
        Node startNode = FindNode(start.ID);
        HashSet<Node> regionNodes = new HashSet<Node>();
        regionNodes.Add(startNode);
        bool allDone = false;
        while (!allDone)
        {
            bool onlyDuplicates = true;
            Node[] regionNodesTemp = regionNodes.ToArray();
            foreach (var node in regionNodesTemp)
            {
                foreach (var edge in node.edgelist)
                {
                    bool excluded = false;
                    foreach (var excludedWP in excludedWaypoints)
                    {
                        if (edge.endNode == FindNode(excludedWP.ID))
                        {
                            excluded = true;
                            break;
                        }
                    }
                    if (excluded)
                        continue;
                    if (regionNodes.Add(edge.endNode))
                        onlyDuplicates = false;
                }
            }
            allDone = onlyDuplicates;
        }
        return regionNodes;

    }

    public void DebugDraw()
    {
        Gizmos.color = Color.green;
        //draw edges
        for (int i = 0; i < edges.Count; i++)
	  	{
            Vector3 start = new Vector3(edges[i].startNode.xPos, edges[i].startNode.yPos, edges[i].startNode.zPos);
            Vector3 end = new Vector3(edges[i].endNode.xPos, edges[i].endNode.yPos, edges[i].endNode.zPos);
            //Debug.DrawLine(start, end, Color.red);
            Gizmos.DrawLine(start, end);
	  	}
    }
	
}
