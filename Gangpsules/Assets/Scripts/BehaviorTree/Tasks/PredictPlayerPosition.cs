using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskDescription("Predict the player's position")]
[TaskCategory("Custom")]
public class PredictPlayerPosition : Action
{
    BehaviorTree bt;
    [Tooltip("The object that we are looking for")]
    public SharedGameObject targetObject;
    [Tooltip("The returned predicted position")]
    public SharedVector3 predictedPosition;
    [Tooltip("The returned waypoint")]
    public SharedGameObject predictedWaypoint;

    Blackboard blackboard;
    public override void OnAwake()
    {
        blackboard = GetComponent<Blackboard>();
    }


    public override TaskStatus OnUpdate()
    {
        GameObject target = targetObject.Value;
        if (target == null)
            return TaskStatus.Failure;
        if (!blackboard.targetsSeen.ContainsKey(target))
            return TaskStatus.Failure;
        Vector3 lastSeenPosition = blackboard.targetsSeen[target].Position;
        Vector3 direction = blackboard.targetsSeen[target].Direction;
        float speed = blackboard.targetsSeen[target].Speed;
        float timePassed = Time.time - blackboard.targetsSeen[target].TimeLastSeen;

        Vector3 predict = lastSeenPosition + direction * speed * timePassed;
        var closestNode = AstarPath.active.GetNearest(predict, Pathfinding.NNConstraint.Default);
        predictedPosition.Value = closestNode.position;
        predictedWaypoint.Value = SpatialPartition.Instance.GetClosestWaypointToAPosition(predict).gameObject;
        return TaskStatus.Success;
    }

}