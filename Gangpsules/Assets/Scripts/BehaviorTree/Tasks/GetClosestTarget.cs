using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskDescription("Get the closest target")]
[TaskCategory("Custom")]
public class GetClosestTarget : Action
{
    [Tooltip("The list of all available targets")]
    public SharedGameObjectList targetList;
    [Tooltip("The returned object")]
    public SharedGameObject returnedObject;

    public override TaskStatus OnUpdate()
    {
        var targets = targetList.Value;
        if (targets.Count == 0)
            return TaskStatus.Failure;
        float minDistance = Mathf.Infinity;
        foreach (var target in targets)
        {
            float distanceSqr = Vector3.SqrMagnitude(target.transform.position - transform.position);
            if (distanceSqr < minDistance)
            {
                minDistance = distanceSqr;
                returnedObject.Value = target;
            }
        }
        return TaskStatus.Success;
    }
}
