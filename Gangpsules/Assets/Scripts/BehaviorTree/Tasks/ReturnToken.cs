using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("Return the token to the enemy manager")]
[TaskCategory("Custom")]
public class ReturnToken : Action
{
    Enemy enemy;

    public override void OnAwake()
    {
        base.OnAwake();
        enemy = GetComponent<Enemy>();
    }

    public override TaskStatus OnUpdate()
    {
        EnemyManager.Instance.UnAllocateToken(enemy);
        return TaskStatus.Success;
    }
}