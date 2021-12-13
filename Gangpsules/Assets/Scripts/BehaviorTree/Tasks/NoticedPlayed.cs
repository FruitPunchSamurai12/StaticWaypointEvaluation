using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("Set the noticed player variables")]
[TaskCategory("Custom")]
public class NoticedPlayed : Action
{
    BehaviorTree bt;

    public override void OnAwake()
    {
        base.OnAwake();
        bt = GetComponent<BehaviorTree>();
    }

    public override TaskStatus OnUpdate()
    {
        var playerLastPos = (SharedVector3)bt.GetVariable("PlayerLastKnownPosition");
        var playerGO = (SharedGameObject)GlobalVariables.Instance.GetVariable("Player");
        playerLastPos.Value = playerGO.Value.transform.position;
        var hasSeenPlayer = (SharedBool)bt.GetVariable("HasSeenPlayer");
        hasSeenPlayer.Value = true;
        return TaskStatus.Success;
    }
}
