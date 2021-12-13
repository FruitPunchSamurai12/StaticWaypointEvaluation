using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskDescription("Get a waypoint without line of sight to the target")]
[TaskCategory("Custom")]
public class GetCoverWaypointRelativeToTarget : Action
{
    [Tooltip("The target")]
    public SharedGameObject targetObject;
    [Tooltip("The returned waypoint")]
    public SharedGameObject returnedObject;

    Enemy enemy;

    public override void OnAwake()
    {
        base.OnAwake();
        enemy = GetComponent<Enemy>();
    }

    public override TaskStatus OnUpdate()
    {
        if (targetObject == null || targetObject.Value == null)
            return TaskStatus.Failure;
        Waypoint wp = enemy.GetCoverWaypoint(targetObject.Value);
        returnedObject.Value = wp == null ? null:wp.gameObject;
        return wp==null?TaskStatus.Failure:TaskStatus.Success;
    }
}