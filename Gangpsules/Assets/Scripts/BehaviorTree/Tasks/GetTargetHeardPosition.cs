using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using UnityEngine;

[TaskDescription("Get all targets within sight")]
[TaskCategory("Custom")]
public class GetTargetHeardPosition : Action
{
    [Tooltip("The position a sound was heard")]
    public SharedVector3 returnedPosition;
    Blackboard blackboard;
    public override void OnAwake()
    {
        blackboard = GetComponent<Blackboard>();
    }


    public override TaskStatus OnUpdate()
    {
        TaskStatus status = TaskStatus.Failure;
        bool investigate = false;
        Vector3 soundPosition = blackboard.GetHeardTargetPosition(out investigate);
        if (investigate == true)
            returnedPosition.Value = soundPosition;
        status = investigate ? TaskStatus.Success : TaskStatus.Failure;
        return status;
    }
}