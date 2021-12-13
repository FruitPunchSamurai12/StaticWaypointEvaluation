using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskDescription("Look at target")]
[TaskCategory("Custom")]
public class LootAtTarget : Action
{
    [Tooltip("The target we are looking at")]
    public SharedGameObject targetObject;
    [Tooltip("The position we are looking at")]
    public SharedVector3 targetPosition;
    [Tooltip("Rotation speed")]
    public SharedFloat smoothRotation;
    [Tooltip("The agent's weapon")]
    public SharedWeapon equippedWeapon;

    public override TaskStatus OnUpdate()
    {
        var lookAtPosition = targetObject==null||targetObject.Value==null?targetPosition.Value: targetObject.Value.transform.position;
        if (lookAtPosition == null)
            return TaskStatus.Failure;
        Vector3 lookDirection = (lookAtPosition - transform.position).normalized;
        transform.rotation = CalculateRotation(lookDirection);
        Weapon w = equippedWeapon.Value;
        if (w != null)
        {
            lookDirection = (lookAtPosition - w.transform.position).normalized;
            w.transform.rotation = CalculateRotation(lookDirection);
        }
        return TaskStatus.Success;
    }

    Quaternion CalculateRotation(Vector3 lookDirection)
    {
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, smoothRotation.Value * Time.deltaTime);
        rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, 0);
        return rotation;
    }
}
