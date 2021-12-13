using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node
{
	public List<Edge> edgelist = new List<Edge>();
	public Node path = null;
	public Waypoint wp;
	public float xPos;
	public float yPos;
	public float zPos;
	public float f, g, h;
	public Node cameFrom;
	
	public Node(Waypoint wp)
	{
		this.wp = wp;
		xPos = wp.transform.position.x;
		yPos = wp.transform.position.y;
		zPos = wp.transform.position.z;
		path = null;
	}
	
	public int GetId()
	{
		return wp.ID;	
	}

}
