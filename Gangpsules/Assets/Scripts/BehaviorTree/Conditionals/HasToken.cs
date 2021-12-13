using BehaviorDesigner.Runtime.Tasks;

public class HasToken : Conditional
{
    Enemy enemy;
    public override void OnAwake()
    {
        enemy = GetComponent<Enemy>();
    }


    public override TaskStatus OnUpdate()
    {
        if (enemy.HasToken)
            return TaskStatus.Success;
        else
            return TaskStatus.Failure;
    }
}