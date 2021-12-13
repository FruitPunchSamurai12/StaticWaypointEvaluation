using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using System.Collections.Generic;

[TaskDescription("Get all targets within sight")]
[TaskCategory("Custom")]
public class GetTargetsWithinSight : Action
{
    [Tooltip("The object variable that will be set when a object is found what the object is")]
    public SharedGameObjectList returnedObjects;
    Blackboard blackboard;
    List<GameObject> targetsInSight = new List<GameObject>();
    public override void OnAwake()
    {
        blackboard = GetComponent<Blackboard>();
    }

    public override void OnStart()
    {
        base.OnStart();
        targetsInSight.Clear();
    }

    public override TaskStatus OnUpdate()
    {
        TaskStatus status = TaskStatus.Failure;
        foreach (var t in blackboard.targetsSeen)
        {
            if (t.Value.InSight)
            {
                targetsInSight.Add(t.Key);
                status = TaskStatus.Success;
            }
        }
        returnedObjects.Value = targetsInSight;
        return status;
    }
}
