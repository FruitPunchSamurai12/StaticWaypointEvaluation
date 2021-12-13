using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using System.Collections.Generic;

[TaskDescription("Get a list of the best waypoints relative to the target")]
[TaskCategory("Custom")]
public class GetBestWaypointsRelativeToTarget : Action
{
    [Tooltip("The target")]
    public SharedGameObject targetObject;
    [Tooltip("The returned waypoint")]
    public SharedGameObject returnedObject;

    Enemy enemy;

    public override void OnAwake()
    {
        base.OnAwake();
        enemy = GetComponent<Enemy>();
    }

    public override TaskStatus OnUpdate()
    {
        if (targetObject == null)
            return TaskStatus.Failure;
        GameObject waypoint = enemy.GetBestWaypointToAttackTarget(targetObject.Value);
        returnedObject.Value = waypoint;
        if (waypoint == null)
            return TaskStatus.Failure;
        return TaskStatus.Success;
    }
}
