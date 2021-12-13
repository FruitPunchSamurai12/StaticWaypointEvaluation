using UnityEngine;
using Pathfinding;
using System;

namespace BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject
{
    [TaskDescription("Move towards the specified position. The position can either be specified by a transform or position. If the transform " +
                     "is used then the position will not be used.")]
    [TaskIcon("Assets/Behavior Designer Movement/Editor/Icons/{SkinColor}MoveTowardsIcon.png")]
    public class MoveTowardsAStar : IAstarAIMovement
    {
        [Tooltip("The GameObject that the agent is moving towards")]
        public SharedGameObject target;
        [Tooltip("If target is null then use the target position")]
        public SharedVector3 targetPosition;
        [Tooltip("Should the agent look towards the target")]
        public SharedBool lookAtTarget;

        Vector3 destination;

        public override void OnAwake()
        {
            base.OnAwake();
        }

        public override void OnStart()
        {
            base.OnStart();
            destination = Target();
        }

        public override TaskStatus OnUpdate()
        {
            //destination = Target();
            //SetDestination(destination);
            Vector3 imAt = transform.position.FlatVector();
            Vector3 iWanttogo = destination.FlatVector();
            float distance = Vector3.Distance(imAt, iWanttogo);

            // Return a task status of success once we've reached the target
            if (distance < arriveDistance.Value)
            {
                return TaskStatus.Success;
            }
            //Debug.Log($"im at {imAt}, i want to go to {iWanttogo} and my distance from it is {distance}");

            //Debug.Log($"still running. trying to get to ");
            return TaskStatus.Running;
        }

        // Return targetPosition if targetTransform is null
        protected override Vector3 Target()
        {
            if (target == null || target.Value == null)
            {
                return targetPosition.Value;
            }
            return target.Value.transform.position;
        }
    }
}