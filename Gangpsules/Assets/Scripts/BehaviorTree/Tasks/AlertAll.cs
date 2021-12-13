using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskDescription("Alert all enemies to the target's position")]
[TaskCategory("Custom")]
public class AlertAll : Action
{
    [Tooltip("The target")]
    public SharedGameObject target;

    public override TaskStatus OnUpdate()
    {
        if (target==null|| target.Value==null)
            return TaskStatus.Failure;
        EnemyManager.Instance.AlertAllEnemies(target.Value);
        return TaskStatus.Success;
    }
}
