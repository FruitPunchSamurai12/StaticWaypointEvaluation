using BehaviorDesigner.Runtime;
using UnityEngine;

public class BehaviourState : IState
{
    BehaviorTree behaviorTree;
    ExternalBehavior behavior;
    string name;
    public BehaviourState(string name, BehaviorTree behaviorTree, ExternalBehavior behavior)
    {
        this.name = name;
        this.behaviorTree = behaviorTree;
        this.behavior = behavior;
    }

    public void OnEnter()
    {
        behaviorTree.ExternalBehavior = behavior;
        behaviorTree.EnableBehavior();
    }

    public void OnExit()
    {
        behaviorTree.DisableBehavior();
    }

    public void Tick()
    {
        Debug.Log($"{name} ticking");
    }
}
