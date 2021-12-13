using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject;
using Pathfinding;

[TaskDescription("Stop the agents movement")]
[TaskCategory("Custom")]
public class HaltMovement : IAstarAIMovement
{


    public override void OnStart()
    {
        base.OnStart();
        Stop();
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }

}