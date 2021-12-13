using UnityEngine;
using Pathfinding;

namespace BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject
{
    [TaskDescription("Seek the target specified using the A* Pathfinding Project.")]
    [TaskCategory("Movement/A* Pathfinding Project")]
    [HelpURL("https://www.opsive.com/support/documentation/behavior-designer-movement-pack/")]
    [TaskIcon("Assets/Behavior Designer Movement/Editor/Icons/{SkinColor}SeekIcon.png")]
    public class Seek : IAstarAIMovement
    {
        [Tooltip("The GameObject that the agent is moving towards")]
        public SharedGameObject target;
        [Tooltip("If target is null then use the target position")]
        public SharedVector3 targetPosition;

        CellTracker cellTracker;
        Waypoint waypointToGo;
        public override void OnAwake()
        {
            base.OnAwake();
            cellTracker = GetComponent<CellTracker>();
        }

        public override void OnStart()
        {
            base.OnStart();
            waypointToGo = null;
            if (target != null && target.Value != null)
                waypointToGo = target.Value.GetComponent<Waypoint>();
            SetDestination(Target());
        }

        // Seek the destination. Return success once the agent has reached the destination.
        // Return running if the agent hasn't reached the destination yet
        public override TaskStatus OnUpdate()
        {
            if (HasArrived()) {
                return TaskStatus.Success;
            }
            SetDestination(Target());
            if(waypointToGo!= null)
            {
                if (EnemyManager.Instance.IsWaypointOccupiedOrAssigned(waypointToGo, cellTracker))
                    return TaskStatus.Success;
            }
            else if (EnemyManager.Instance.IsWaypointOccupiedOrAssigned(cellTracker.closestWaypoint, cellTracker))
                return TaskStatus.Success;

            return TaskStatus.Running;
        }

        // Return targetPosition if target is null
        protected override Vector3 Target()
        {
            if (target.Value != null) {
                return target.Value.transform.position;
            }
            return targetPosition.Value;
        }

        public override void OnReset()
        {
            base.OnReset();
            target = null;
            targetPosition = Vector3.zero;
        }
    }
}