using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Custom")]
public class TimePassedWithTargetInSight : Conditional
{
    [Tooltip("The object that we are looking for")]
    public SharedGameObject targetObject;
    [Tooltip("Time that needs to pass to return success")]
    public SharedFloat timeThreshold;

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
        if (blackboard.targetsSeen[target].TimeWithinSight >= timeThreshold.Value)
            return TaskStatus.Success;
        else
            return TaskStatus.Failure;
    }
}

