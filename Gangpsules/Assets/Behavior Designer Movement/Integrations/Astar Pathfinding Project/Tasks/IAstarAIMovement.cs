using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject
{
    // Abstract class for any task that uses an IAstarAI cmponent
    public abstract class IAstarAIMovement : Movement
    {
        [Tooltip("The speed of the agent")]
        public SharedFloat speed = 10;
        [Tooltip("The agent has arrived when the destination is less than the specified amount")]
        public SharedFloat arriveDistance = 0.2f;
        [Tooltip("Should the NavMeshAgent be stopped when the task ends?")]
        public SharedBool stopOnTaskEnd = true;
        [Tooltip("Is the agent being used for 2D movement?")]
        public bool use2DMovement;

        protected IAstarAI agent;
        protected Seeker seeker;
        protected bool pathLegit=true;
        protected List<Vector3> nodes;
        public override void OnAwake()
        {
            agent = gameObject.GetComponent<IAstarAI>();
            seeker = GetComponent<Seeker>();
        }

        private void onPathComplete(Path p)
        {

            if (!SamplePosition(Target().FlatVector()))
            {
                nodes = p.vectorPath;

                pathLegit = false;
            }

        }

        public override void OnStart()
        {
            agent.maxSpeed = speed.Value;
            seeker.pathCallback += onPathComplete;
        }

        protected override bool SetDestination(Vector3 target)
        {
            agent.canSearch = true;
            agent.canMove = true;
            agent.destination = target;
            Debug.DrawRay(target, Vector3.up, Color.red, 3);
            return true;
        }

        protected virtual Vector3 Target()
        {
            return new Vector3(0, 0, 0);
        }

        protected override Vector3 Velocity()
        {
            return agent.velocity;
        }

        protected override void UpdateRotation(bool update)
        {
            // Intentionally left blank
        }

        protected bool SamplePosition(Vector3 position)
        {
            var direction = transform.InverseTransformDirection(AstarPath.active.GetNearest(position).position - position);
            direction.y = 0;
            return direction.sqrMagnitude < arriveDistance.Value;
        }

        protected override bool HasPath()
        {

            return agent.hasPath;
        }

        protected override void Stop()
        {
            agent.destination = transform.position;
            agent.canMove = false;
            agent.canSearch = false;
        }

        protected override bool HasArrived()
        {
            var direction = transform.InverseTransformPoint(agent.destination);
            if (use2DMovement) {
                direction.z = 0;
            } else {
                direction.y = 0;
            }
            return direction.magnitude < arriveDistance.Value;
        }

        public override void OnEnd()
        {
            if (stopOnTaskEnd.Value) {
                Stop();
            }
            seeker.pathCallback -= onPathComplete;
        }

        public override void OnReset()
        {
            speed = 10;
            arriveDistance = 1;
            stopOnTaskEnd = true;
            pathLegit = true;
        }

        
    }
}
