using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackboard : MonoBehaviour
{
    public Dictionary<GameObject, TargetSeenMemory> targetsSeen = new Dictionary<GameObject, TargetSeenMemory>();
    public TargetHeardMemory targetHeardMemory;

    public void SetTarget(GameObject target)
    {
        if (!targetsSeen.ContainsKey(target))
        {
            IMover mover = target.GetComponent<IMover>();
            CellTracker cellTracker = target.GetComponent<CellTracker>();
            TargetSeenMemory targetSpottedMemory = new TargetSeenMemory(target.transform.position, mover.Direction, mover.Speed, cellTracker);
            targetsSeen.Add(target, targetSpottedMemory);
        }
    }

    public void SeenTarget(GameObject target)
    {       
        IMover mover = target.GetComponent<IMover>();
        CellTracker cellTracker = target.GetComponent<CellTracker>();
        if (targetsSeen.ContainsKey(target))
            targetsSeen[target].UpdateMemory(target.transform.position, mover.Direction,mover.Speed,cellTracker);
        else
        {
            TargetSeenMemory targetSpottedMemory = new TargetSeenMemory(target.transform.position, mover.Direction,mover.Speed, cellTracker);
            targetsSeen.Add(target, targetSpottedMemory);
        }
    }

    public void HeardTarget(GameObject target)
    {
        CellTracker cellTracker = target.GetComponent<CellTracker>();
        if(targetHeardMemory==null)
        {
            targetHeardMemory = new TargetHeardMemory(target.transform.position, cellTracker);
        }
        else
        {
            targetHeardMemory.UpdateMemory(target.transform.position, cellTracker);
        }
    }

    public Vector3 GetHeardTargetPosition(out bool investigate)
    {
        if(targetHeardMemory == null)
        {
            investigate = false;
            return Vector3.zero;
        }
        if(targetHeardMemory.InvestigateSound)
        {
            targetHeardMemory.InvestigateSound = false;
            investigate = true;
            return targetHeardMemory.Position;
        }
        investigate = false;
        return Vector3.zero;
    }

    public void LostTarget(GameObject target)
    {
        if (targetsSeen.ContainsKey(target))
            targetsSeen[target].TargetLost();
    }
}

public class TargetSeenMemory
{
    public TargetSeenMemory(Vector3 position, Vector3 direction, float speed,CellTracker cellTracker)
    {
        UpdateMemory(position, direction,speed,cellTracker);
    }

    public void UpdateMemory(Vector3 position, Vector3 direction, float speed, CellTracker cellTracker)
    {
        Position = position;
        Direction = direction;
        Speed = speed;
        CellTracker = cellTracker;
        TimeLastSeen = Time.time;
        TimeWithinSight += Time.fixedDeltaTime;
        InSight = true;
    }

    public void TargetLost()
    {
        TimeWithinSight -= Time.deltaTime;
        if(TimeWithinSight<0)
            TimeWithinSight = 0;
        InSight = false;
    }
    
    public CellTracker CellTracker { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 Direction { get; private set; }
    public float Speed { get; private set; }
    public float TimeLastSeen { get; private set; }
    public float TimeFirstSeen { get; private set; }
    public float TimeWithinSight { get; private set; }

    public bool InSight { get; private set; }
}

public class TargetHeardMemory
{
    public TargetHeardMemory(Vector3 position, CellTracker cellTracker)
    {
        UpdateMemory(position, cellTracker);
    }

    public void UpdateMemory(Vector3 position, CellTracker cellTracker)
    {
        Position = position;
        CellTracker = cellTracker;
        TimeLastHeard = Time.time;
        InvestigateSound = true;
    }

    public CellTracker CellTracker { get; private set; }
    public Vector3 Position { get; private set; }
    public float TimeLastHeard { get; private set; }
    public bool InvestigateSound { get; set; }
}