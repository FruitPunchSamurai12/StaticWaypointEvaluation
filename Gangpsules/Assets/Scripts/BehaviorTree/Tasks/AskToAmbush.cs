using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskDescription("Ask the enemy manager if setting an ambush is possible")]
[TaskCategory("Custom")]
public class AskToAmbush : Action
{
    [Tooltip("The target we are ambushing")]
    public SharedGameObject targetObject;

    Enemy enemy;

    public override void OnAwake()
    {
        base.OnAwake();
        enemy = GetComponent<Enemy>();
    }

    public override TaskStatus OnUpdate()
    {
        var target = targetObject.Value;
        if (target == null)
            return TaskStatus.Failure;

        enemy.TryAmbush(target);
        return TaskStatus.Success;
    }
}
