using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    public string targetsTag;
    public float minUpdateFrames = 2f;
    public float maxUpdateFrames = 5f;
    public int maxNumberOfTokens = 1;
    int tokensRemaining;
    List<CellTracker> targets;
    List<GameObject> targetGameObjects;
    List<GameObject> customCoordinateSystem;
    List<CellTracker> enemyTrackers;
    List<Enemy> enemies;
    List<List<Waypoint>> linesOfSightToTarget;
    Dictionary<GameObject, List<int>> bestWaypointsForEachTarget;
    Dictionary<CellTracker,Waypoint> assignedWaypoints;
    float updateTime;
    float timer;
    int currentEnemyIndex = 0;
    WaypointManager waypointManager;
    List<Enemy> enemiesToDelete;

    public event Action onEnemyKilled;
    public event Action onNoEnemiesLeft;
    public event Action onEnemySpawned;
    public event Action onUnallocatedToken;
    public static EnemyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        minUpdateFrames = minUpdateFrames * Time.deltaTime;
        maxUpdateFrames = maxUpdateFrames * Time.deltaTime;
        tokensRemaining = maxNumberOfTokens;
        targets = new List<CellTracker>();
        enemiesToDelete = new List<Enemy>();
        targetGameObjects = new List<GameObject>();
        customCoordinateSystem = new List<GameObject>();
        linesOfSightToTarget = new List<List<Waypoint>>();
        waypointManager = FindObjectOfType<WaypointManager>();
        assignedWaypoints = new Dictionary<CellTracker, Waypoint>();
        bestWaypointsForEachTarget = new Dictionary<GameObject, List<int>>();
       
        enemyTrackers = new List<CellTracker>();
        enemies = new List<Enemy>();
    }

    private void Start()
    {
        foreach (var go in GameObject.FindGameObjectsWithTag(targetsTag))
        {           
            CellTracker ct = go.GetComponent<CellTracker>();
            if (ct != null)
            {
                targetGameObjects.Add(go);
                bestWaypointsForEachTarget.Add(go, new List<int>());
                targets.Add(ct);
                GameObject empty = new GameObject();
                customCoordinateSystem.Add(empty);
            }
        }
        timer = 0;
        updateTime = Random.Range(minUpdateFrames, maxUpdateFrames);
        foreach (var ct in targets)
        {
            linesOfSightToTarget.Add(waypointManager.GetWaypointsWithLineOfSightToAWaypoint(ct.closestWaypointIndex));
        }
    }

    public void AskToAmbush(Enemy enemy, GameObject target)
    {
        CellTracker cellTracker = target.GetComponent<CellTracker>();
        int targetWp = cellTracker!=null?cellTracker.closestWaypointIndex:target.GetComponent<Waypoint>().ID;
        PinchPoint pinchPoint = waypointManager.waypointEvaluation.IsWaypointAPinchPoint(targetWp);
        if(pinchPoint!=null)
        {
            SetUpAmbushPositions(pinchPoint,0,1);
        }
        else
        {
            PinchPointPair pinchPointPair = waypointManager.waypointEvaluation.IsWaypointAPinchPointPair(targetWp);
            if(pinchPointPair!=null)
            {
                if(enemies.Count>=2)
                {
                    SetUpAmbushPositions(pinchPointPair.n1, 0, 2);
                    SetUpAmbushPositions(pinchPointPair.n2, 1, 2);
                }
            }
        }
    }

    private void SetUpAmbushPositions(PinchPoint pinchPoint,int enemiesStartIndex,int enemiesStep)
    {
        List<AmbushIDsWithAngleToAmbushPoint> angles = new List<AmbushIDsWithAngleToAmbushPoint>();
        for (int i = 0; i < pinchPoint.anglesToOutsideIDFromAmbushPoints.Count; i++)
        {
            AmbushIDsWithAngleToAmbushPoint angle = pinchPoint.anglesToOutsideIDFromAmbushPoints[i];
            //angle.ambushPointIDs = angle.ambushPointIDs.Where(t => Vector3.SqrMagnitude(waypointManager.waypoints[t].transform.position - waypointManager.waypoints[pinchPoint.ID].transform.position) < enemies[0].attackRange * enemies[0].attackRange).ToList();
            if(angle.ambushPointIDs.Count>0)
                angles.Add(angle);
        }
        for (int i = enemiesStartIndex; i < Mathf.Min(enemies.Count,angles.Count); i += enemiesStep)
        {
            if (i > enemies.Count) return;
            int randomIndex = Random.Range(0, angles.Count);
            var angle = angles[randomIndex];
            List<int> goodAmbushWaypoints = angle.ambushPointIDs;
            goodAmbushWaypoints = goodAmbushWaypoints.OrderBy(t => Vector3.SqrMagnitude(waypointManager.waypoints[t].transform.position - enemies[i].transform.position)).ToList();
            if (goodAmbushWaypoints.Count > 0)
            {
                enemies[i].SetUpAmbush(waypointManager.waypoints[goodAmbushWaypoints.First()], waypointManager.waypoints[pinchPoint.OutsideID]);
                angles.Remove(angle);
            }
        }
    }

    public void AssignWaypoint(CellTracker cellTracker, Waypoint waypoint)
    {
        assignedWaypoints[cellTracker] = waypoint;
    }

    public void AlertAllEnemies(GameObject target)
    {
        foreach (var enemy in enemies)
        {
            enemy.GetAlerted(target);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer>updateTime)
        {
            if(currentEnemyIndex<enemies.Count)
                UpdateAvailableWaypoints(currentEnemyIndex);
            timer = 0;
            updateTime = Random.Range(minUpdateFrames, maxUpdateFrames);
            currentEnemyIndex++;
            if (currentEnemyIndex >= enemyTrackers.Count)
                currentEnemyIndex = 0;
        }

    }

    private void UpdateAvailableWaypoints(int enemyIndex)
    {
        for (int i = 0; i < targetGameObjects.Count; i++)
        {
            var bestAvailable = GetBestAttackWaypoints(targetGameObjects[i], enemyTrackers[enemyIndex]);
            var bestAvailableWithinRange = bestAvailable.Where
                (t => Vector3.SqrMagnitude(t.transform.position - targetGameObjects[i].transform.position) <
                enemies[enemyIndex].attackRange * enemies[enemyIndex].attackRange).ToList();

            bestAvailableWithinRange = bestAvailableWithinRange.Where(t => !IsWaypointOccupiedOrAssigned(t, enemyTrackers[enemyIndex])).ToList();
            customCoordinateSystem[i].transform.position = targetGameObjects[i].transform.position;
            Transform origin = customCoordinateSystem[i].transform;
            Vector3 direction = origin.position - enemies[enemyIndex].transform.position;
            Quaternion lookrotation = Quaternion.LookRotation(direction, Vector3.up);
            origin.rotation = lookrotation;
            bestAvailableWithinRange = bestAvailableWithinRange.Where(t => origin.InverseTransformPoint(t.transform.position).z < 0).ToList();
            Waypoint bestWaypoint = null;
            bestAvailableWithinRange = bestAvailableWithinRange.OrderBy(t => Vector3.SqrMagnitude(t.transform.position - enemies[enemyIndex].transform.position)).ToList();
            for (int j = 0; j < bestAvailableWithinRange.Count; j++)
            {
                RaycastHit info;
                Ray ray = new Ray(bestAvailableWithinRange[j].transform.position, targetGameObjects[i].transform.position - bestAvailableWithinRange[j].transform.position);
                if (Physics.Raycast(ray, out info))
                {
                    if (info.collider.CompareTag(targetsTag))
                    {
                        bestWaypoint = bestAvailableWithinRange[j];
                        break;
                    }
                }
            }
            if (bestWaypoint != null)
            {
                enemies[enemyIndex].SetBestWaypointToAttackTarget(targetGameObjects[i], bestWaypoint.gameObject);
                assignedWaypoints[enemyTrackers[enemyIndex]] = bestWaypoint;
            }
        }
    }

    public List<Waypoint> GetBestAttackWaypoints(GameObject target, CellTracker enemy)
    {
        var bestWaypoints = bestWaypointsForEachTarget[target];
        List<Waypoint> wps = new List<Waypoint>();
        foreach (var wpIndex in bestWaypoints)
        {
            wps.Add(waypointManager.waypoints[wpIndex]);
        }
        return wps;
    }

    public Waypoint GetClosestWaypointToWaypointAWithoutLineOfSightToWaypointB(Waypoint wpA, Waypoint wpB,CellTracker ct)
    {
        Waypoint coverWaypoint = null;
        foreach (int wpIndex in wpA.linksToOtherWaypoints)
        {
            if (!waypointManager.WaypointAHasLineOfSightToWaypointB(wpIndex, wpB.ID))
            {
                coverWaypoint = waypointManager.waypoints[wpIndex];
                break;
            }
        }
        if (coverWaypoint != null) return coverWaypoint;
        List<Waypoint> coversAvailable = waypointManager.GetWaypointsWithoutLineOfSightToAWaypoint(wpB);
        coversAvailable = coversAvailable.Where(t => !IsWaypointOccupiedOrAssigned(t, null)).ToList();
        customCoordinateSystem[0].transform.position = wpB.transform.position;
        Transform origin = customCoordinateSystem[0].transform;
        Vector3 direction = origin.position - wpA.transform.position;
        Quaternion lookrotation = Quaternion.LookRotation(direction, Vector3.up);
        origin.rotation = lookrotation;

        coversAvailable = coversAvailable.Where(t => origin.InverseTransformPoint(t.transform.position).z < 0 && 
                            Vector3.SignedAngle(Vector3.zero,origin.InverseTransformPoint(t.transform.position), origin.right)>45).ToList();
        coversAvailable = coversAvailable.OrderBy(t => Vector3.SqrMagnitude(t.transform.position - wpA.transform.position)).ToList();
        coverWaypoint = coversAvailable.Count>0?coversAvailable.First():null;
        assignedWaypoints[ct] = coverWaypoint;
        return coverWaypoint;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            linesOfSightToTarget[i] = waypointManager.GetWaypointsWithLineOfSightToAWaypoint(targets[i].closestWaypointIndex);
            bestWaypointsForEachTarget[targetGameObjects[i]].Clear();
            foreach (var wp in linesOfSightToTarget[i])
            {
                if (waypointManager.WaypointAHasNeighboorWithoutLineOfSightToWaypointB(wp.ID, targets[i].closestWaypointIndex))
                    bestWaypointsForEachTarget[targetGameObjects[i]].Add(wp.ID);
            }
        }
    }

    private void LateUpdate()
    {
        foreach (var enemy in enemiesToDelete)
        {
            DeleteEnemy(enemy);
        }
        enemiesToDelete.Clear();
        if (enemies.Count == 0)
            onNoEnemiesLeft?.Invoke();
    }

    public void NewEnemy(Enemy enemy)
    {
        enemies.Add(enemy);
        var bestWaypoints = waypointManager.waypointEvaluation.bestWaypoints;
        List<GameObject> patrolWaypoints = new List<GameObject>();
        for (int i = 0; i < bestWaypoints.Length; i++)
        {
            patrolWaypoints.Add(waypointManager.waypoints[bestWaypoints[i]].gameObject);
        }
        enemy.SetPatrolWaypoints(patrolWaypoints);
        CellTracker ct = enemy.GetComponent<CellTracker>();
        enemyTrackers.Add(ct);
        assignedWaypoints.Add(ct,waypointManager.waypoints[0]);
        onEnemySpawned?.Invoke();
    }

    public void EnemyToDelete(Enemy enemy) => enemiesToDelete.Add(enemy);

    void DeleteEnemy(Enemy enemy)
    {
        UnAllocateToken(enemy);
        enemies.Remove(enemy);
        var ct = enemy.GetComponent<CellTracker>();
        enemyTrackers.Remove(ct);
        assignedWaypoints.Remove(ct);
        UIManager.Instance.EnemyKilled();
        onEnemyKilled?.Invoke();
    }

    public void Reset()
    {
        foreach (var enemy in enemies)
        {
            EnemyToDelete(enemy);

        }
        foreach (var enemy in enemiesToDelete)
        {
            DeleteEnemy(enemy);
            Destroy(enemy.gameObject);
        }
        enemiesToDelete.Clear();
    }

    public bool IsWaypointOccupiedOrAssigned(Waypoint wp,CellTracker exludedCellTracker)
    {
        foreach (var aw in assignedWaypoints)
        {
            if (aw.Key!= exludedCellTracker && aw.Value == wp)
                return true;
        }
        foreach (var cellTracker in enemyTrackers)
        {
            if(cellTracker!=exludedCellTracker)
            {
                if (wp == cellTracker.closestWaypoint)
                    return true;
            }
        }
        return false;
    }

    [ContextMenu("occupied waypoints")]
    public void PrintOccupiedWaypoints()
    {
        foreach (var wp in waypointManager.waypoints)
        {
            if (IsWaypointOccupiedOrAssigned(wp, null))
                Debug.Log($"wp {wp} is occupied");
        }
    }

    public bool AllocateToken(Enemy enemy)
    {
        if (enemy.HasToken)
            return false;
        if (tokensRemaining == 0)
            return false;
        enemy.GiveToken();
        tokensRemaining--;
        return true;
    }

    public void UnAllocateToken(Enemy enemy)
    {
        if (enemy.HasToken)
        {
            enemy.RemoveToken();
            tokensRemaining++;
            if (tokensRemaining > maxNumberOfTokens)
                tokensRemaining = maxNumberOfTokens;
            onUnallocatedToken?.Invoke();
        }
    }

    public bool IsWaypointOccupiedOrAssigned(int wpIndex, CellTracker exludedCellTracker)
    {
        return IsWaypointOccupiedOrAssigned(waypointManager.waypoints[wpIndex], exludedCellTracker);
    }
}
