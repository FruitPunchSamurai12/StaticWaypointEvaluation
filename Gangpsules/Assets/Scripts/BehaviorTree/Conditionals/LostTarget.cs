using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

public class LostTarget : Conditional
{
    [Tooltip("The object that we are looking for")]
    public SharedGameObject targetObject;

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
        if (blackboard.targetsSeen[target].InSight)
            return TaskStatus.Failure;
        else
            return TaskStatus.Success;
    }
}
