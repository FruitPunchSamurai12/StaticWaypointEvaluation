using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject;

[TaskDescription("Look at target")]
[TaskCategory("Custom")]
public class RotateRelativeToMovement : IAstarAIMovement
{
    [Tooltip("Rotation speed")]
    public SharedFloat smoothRotation;
    [Tooltip("Look Vector relative to velocity")]
    public SharedVector3 lookVector;

    Vector3 previousNonZeroVelocity;

    public override void OnAwake()
    {
        base.OnAwake();
        previousNonZeroVelocity = transform.forward;
    }

    public override TaskStatus OnUpdate()
    {
        Vector3 velocity = Velocity();
        if (velocity != Vector3.zero)
            previousNonZeroVelocity = velocity;
        Quaternion movementDirectionRotation = Quaternion.LookRotation(previousNonZeroVelocity);
        Vector3 actualMoveDirection = movementDirectionRotation * lookVector.Value;
        transform.rotation = CalculateRotation(actualMoveDirection);

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