using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskDescription("Get target's last seen position")]
[TaskCategory("Custom")]
public class SetTargetLastSeenPosition : Action
{
    [Tooltip("The object that we are looking for")]
    public SharedGameObject targetObject;
    [Tooltip("The returned value")]
    public SharedVector3 returnedValue;
    [Tooltip("The returned waypoint")]
    public SharedGameObject returnedWaypoint;


    Blackboard blackboard;
    public override void OnAwake()
    {
        blackboard = GetComponent<Blackboard>();
    }


    public override TaskStatus OnUpdate()
    {
        
        GameObject target = targetObject.Value;
        if (target == null)
            return TaskStatus.Failure;
        if (!blackboard.targetsSeen.ContainsKey(target))
            return TaskStatus.Failure;
        returnedValue.Value = blackboard.targetsSeen[target].Position;
        returnedWaypoint.Value = blackboard.targetsSeen[target].CellTracker.closestWaypoint.gameObject;
        return TaskStatus.Success;
    }
}
