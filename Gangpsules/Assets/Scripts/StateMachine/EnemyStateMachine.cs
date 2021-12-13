using UnityEngine;
using UnityEngine.AI;
using System;
using BehaviorDesigner.Runtime;

public class EnemyStateMachine : MonoBehaviour
{
    BehaviorTree behaviourTree;
    Player player;
    [SerializeField] ExternalBehavior idleBehaviour;
    [SerializeField] ExternalBehavior alertBehaviour;
    [SerializeField] ExternalBehavior combatBehaviour;
    StateMachine stateMachine;
    NavMeshAgent navMeshAgent;
    Enemy enemy;

    public Type CurrentStateType => stateMachine.CurrentState.GetType();
    public event Action<IState> OnEnemyStateChanged;

    private void Awake()
    {
        behaviourTree = GetComponent<BehaviorTree>();
        stateMachine = new StateMachine();       
        stateMachine.OnStateChanged += state => OnEnemyStateChanged?.Invoke(state);
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();
        player = FindObjectOfType<Player>();

        var idle = new BehaviourState("idle",behaviourTree,idleBehaviour);
        var alert = new BehaviourState("alert",behaviourTree, alertBehaviour);
        //var combat = new BehaviourState("combat",behaviourTree, combatBehaviour);

        stateMachine.AddTransition(idle, alert, () => navMeshAgent.transform.position.FlatVectorDistanceSquared(player.transform.position) < 5f);
        //stateMachine.AddTransition(chasePlayer, attack, () => DistanceFlat(navMeshAgent.transform.position, player.transform.position) < 2f);
        //stateMachine.AddAnyTransition(dead, ()=>_entity.Health <= 0);

        stateMachine.SetState(idle);
    }


    private void Update()
    {
        Debug.Log(player);
        stateMachine.Tick();
    }
}