using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("Ask the enemy manager for a token")]
[TaskCategory("Custom")]
public class AskForToken : Action
{
    Enemy enemy;

    public override void OnAwake()
    {
        base.OnAwake();
        enemy = GetComponent<Enemy>();
    }

    public override TaskStatus OnUpdate()
    {
        EnemyManager.Instance.AllocateToken(enemy);
        return TaskStatus.Success;
    }
}
