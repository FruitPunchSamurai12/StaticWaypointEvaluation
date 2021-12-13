using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialPartition : MonoBehaviour
{
    [SerializeField] WaypointManager waypointManager;
    [SerializeField] float width, height;
    [SerializeField] int rows, collumns;
    public Cell[] cells;

    bool initialized = false;

    public static SpatialPartition Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        initialized = false;
        InitializeCells();
        SetCellNeighboors();
        SetCellWaypoints();
        initialized = true;

    }

    [System.Serializable]
    public class Cell
    {
        public int index { get; private set; }
        public Vector3 position { get; private set; }
        float width, height;
        public List<int> neighboors;
        public List<int> waypointsInCell;
        public Bounds cellBounds { get; private set; }

        public void DrawCell()
        {
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            UnityEditor.Handles.DrawWireCube(position, new Vector3(width, 2, height));
            UnityEditor.Handles.Label(position, index.ToString(),style);
#endif
        }

        public Cell(int index,Vector3 pos, float w,float h)
        {
            this.index = index;
            position = pos;
            width = w;
            height = h;
            cellBounds = new Bounds(position, new Vector3(w, 2, h));
        }
    }

    void OnValidate()
    {
        //initialized = false;       
        //InitializeCells();
        //SetCellNeighboors();
        //SetCellWaypoints();
        //initialized = true;
    }

    void InitializeCells()
    {
        float widthStep = width / collumns;
        float heightStep = height / rows;
        Vector3 bottomLeft = new Vector3(-width / 2, 2, -height / 2);
        cells = new Cell[rows*collumns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < collumns; j++)
            {
                cells[i*collumns + j] = new Cell(i * collumns + j,new Vector3(bottomLeft.x + (widthStep / 2) + widthStep * j, 1, bottomLeft.z + (heightStep / 2) + heightStep * i), widthStep, heightStep);
            }
        }
    }

    void SetCellNeighboors()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < collumns; j++)
            {
                cells[i * collumns + j].neighboors = new List<int>();
                for (int k = i-1; k <= i+1; k++)
                {
                    for (int l = j-1; l <= j+1; l++)
                    {
                        if (k == i && l == j) continue;
                        if (k >= 0 && k < rows && l >= 0 && l < collumns)
                            cells[i * collumns + j].neighboors.Add(k * collumns + l);
                    }
                }
            }
        }
    }

    void SetCellWaypoints()
    {
        foreach (var cell in cells)
        {
            cell.waypointsInCell = new List<int>();
        }
        List<Waypoint> waypoints = waypointManager.waypoints;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 waypointPos = waypoints[i].transform.position;
            foreach (var cell in cells)
            {
                if (cell.cellBounds.Contains(new Vector3(waypointPos.x, 1, waypointPos.z)))
                {
                    cell.waypointsInCell.Add(i);
                    break;
                }
            }

        }
    }

    public List<int> GetCellAndNeighboorWaypoints(int cellIndex)
    {
        List<int> wps = new List<int>();
        wps.AddRange(cells[cellIndex].waypointsInCell);
        foreach (var cell in cells[cellIndex].neighboors)
        {
            wps.AddRange(cells[cell].waypointsInCell);
        }
        return wps;
    }


    public Waypoint GetClosestWaypointToAPosition(Vector3 position)
    {
        int index = 0;
        foreach (var cell in Instance.cells)
        {
            if (cell.cellBounds.Contains(position))
            {
                index = cell.index;
                break;
            }
        }

        float minMagnitudeSqr = float.MaxValue;
        List<Waypoint> waypoints = waypointManager.waypoints;
        Waypoint closest = null;
        foreach (int wpIndex in GetCellAndNeighboorWaypoints(index))
        {
            float magnitudeSqr = Vector3.SqrMagnitude(waypoints[wpIndex].transform.position - position);
            if (magnitudeSqr < minMagnitudeSqr)
            {
                minMagnitudeSqr = magnitudeSqr;
                closest = waypoints[wpIndex];
            }
        }
        return closest;
    }
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!initialized) return;
        UnityEditor.Handles.color = Color.blue;
        foreach (var cell in cells)
        {
            cell.DrawCell();
        }
    }
#endif
}
