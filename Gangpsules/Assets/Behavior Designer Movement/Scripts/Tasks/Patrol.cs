using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Movement
{
    [TaskDescription("Patrol around the specified waypoints using the Unity NavMesh.")]
    [TaskCategory("Movement")]
    [HelpURL("https://www.opsive.com/support/documentation/behavior-designer-movement-pack/")]
    [TaskIcon("Assets/Behavior Designer Movement/Editor/Icons/{SkinColor}PatrolIcon.png")]
    public class Patrol : NavMeshMovement
    {
        [Tooltip("Should the agent patrol the waypoints randomly?")]
        public SharedBool randomPatrol = false;
        [Tooltip("The length of time that the agent should pause when arriving at a waypoint")]
        public SharedFloat waypointPauseDuration = 0;
        [Tooltip("The waypoints to move to")]
        public SharedGameObjectList waypoints;
        [Tooltip("Should the agent sweep when pausing at waypoint")]
        public SharedBool sweepAtPause = false;
        [Tooltip("The agent's sweeping angle")]
        public SharedFloat sweepAngle = 0;
        [Tooltip("Sweep clockwise or anticlockwise")]
        public SharedBool sweepClockwise = true;
        [Tooltip("Choose clockwise or anticlockwise at random. If this is true the sweepClockwise variable is ignored")]
        public SharedBool randomSweepDirection = false;


        // The current index that we are heading towards within the waypoints array
        private int waypointIndex;
        private float waypointReachedTime;

        //sweep stuff
        Quaternion startRotation;
        Quaternion endRotation;
        float timeLerpStarted;

        public override void OnStart()
        {
            base.OnStart();

            // initially move towards the closest waypoint
            float distance = Mathf.Infinity;
            float localDistance;
            for (int i = 0; i < waypoints.Value.Count; ++i) {
                if ((localDistance = Vector3.Magnitude(transform.position - waypoints.Value[i].transform.position)) < distance) {
                    distance = localDistance;
                    waypointIndex = i;
                }
            }
            waypointReachedTime = -1;
            SetDestination(Target());
        }

        // Patrol around the different waypoints specified in the waypoint array. Always return a task status of running. 
        public override TaskStatus OnUpdate()
        {
            if (waypoints.Value.Count == 0) {
                return TaskStatus.Failure;
            }
            if (HasArrived()) {
                if (waypointReachedTime == -1) {
                    waypointReachedTime = Time.time;
                    if (sweepAtPause.Value)
                        SetUpSweep();
                }
                // wait the required duration before switching waypoints.
                if (waypointReachedTime + waypointPauseDuration.Value <= Time.time) {
                    if (randomPatrol.Value) {
                        if (waypoints.Value.Count == 1) {
                            waypointIndex = 0;
                        } else {
                            // prevent the same waypoint from being selected
                            var newWaypointIndex = waypointIndex;
                            while (newWaypointIndex == waypointIndex) {
                                newWaypointIndex = Random.Range(0, waypoints.Value.Count);
                            }
                            waypointIndex = newWaypointIndex;
                        }
                    } else {
                        waypointIndex = (waypointIndex + 1) % waypoints.Value.Count;
                    }
                    SetDestination(Target());
                    waypointReachedTime = -1;
                }
                else
                {
                    if (sweepAtPause.Value)
                        Sweep();
                }
            }

            return TaskStatus.Running;
        }

        void SetUpSweep()
        {
            startRotation = transform.rotation;
            float angle = sweepAngle.Value;
            if (randomSweepDirection.Value)
            {
                if (Random.Range(0, 100) < 50)
                    angle = -angle;
            }
            else
            {
                if (sweepClockwise.Value)
                    angle = -angle;
            }
            endRotation = startRotation * Quaternion.Euler(Vector3.up * angle);
            timeLerpStarted = Time.time;
        }

        void Sweep()
        {
            float timeSinceStarted = Time.time - timeLerpStarted;
            float percentageComplete = timeSinceStarted / waypointPauseDuration.Value;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, percentageComplete);
        }

        // Return the current waypoint index position
        private Vector3 Target()
        {
            if (waypointIndex >= waypoints.Value.Count) {
                return transform.position;
            }
            return waypoints.Value[waypointIndex].transform.position;
        }

        // Reset the public variables
        public override void OnReset()
        {
            base.OnReset();

            randomPatrol = false;
            waypointPauseDuration = 0;
            waypoints = null;
        }

        // Draw a gizmo indicating a patrol 
        public override void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (waypoints == null || waypoints.Value == null) {
                return;
            }
            var oldColor = UnityEditor.Handles.color;
            UnityEditor.Handles.color = Color.yellow;
            for (int i = 0; i < waypoints.Value.Count; ++i) {
                if (waypoints.Value[i] != null) {
                    UnityEditor.Handles.SphereHandleCap(0, waypoints.Value[i].transform.position, waypoints.Value[i].transform.rotation, 1, EventType.Repaint);
                }
            }
            UnityEditor.Handles.color = oldColor;
#endif
        }
    }
}