using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject
{
    [TaskDescription("Find a place to hide and move to it.")]
    [TaskCategory("Movement/A* Pathfinding Project")]
    [HelpURL("https://www.opsive.com/support/documentation/behavior-designer-movement-pack/")]
    [TaskIcon("Assets/Behavior Designer Movement/Editor/Icons/{SkinColor}CoverIcon.png")]
    public class Cover : IAstarAIMovement
    {
        [Tooltip("The distance to search for cover")]
        public SharedFloat maxCoverDistance = 1000;
        [Tooltip("The layermask of the available cover positions")]
        public LayerMask availableLayerCovers;
        [Tooltip("The maximum number of raycasts that should be fired before the agent gives up looking for an agent to find cover behind")]
        public SharedInt maxRaycasts = 100;
        [Tooltip("How large the step should be between raycasts")]
        public SharedFloat rayStep = 1;
        [Tooltip("Once a cover point has been found, multiply this offset by the normal to prevent the agent from hugging the wall")]
        public SharedFloat coverOffset = 2;
        [Tooltip("Should the agent look at the cover point after it has arrived?")]
        public SharedBool lookAtCoverPoint = false;
        [Tooltip("Should the agent look at the object it is taking cover from after it has arrived?")]
        public SharedBool lookAtTarget = false;
        [Tooltip("Should the agent attempt to find a cover spot where he can fire at the target while being partialy covered?")]
        public SharedBool coverFire = false;
        [Tooltip("The agent is done rotating to the cover point when the square magnitude is less than this value")]
        public SharedFloat rotationEpsilon = 0.5f;
        [Tooltip("Max rotation delta if lookAtCoverPoint")]
        public SharedFloat maxLookAtRotationDelta;
        [Tooltip("The object that we are taking cover from")]
        public SharedGameObject targetObject;
        // The cover position
        private Vector3 coverPoint;
        private bool foundCover;
        List<Vector3> directions = new List<Vector3>();
        AIPath aiPath;

        public override void OnAwake()
        {
            base.OnAwake();
            aiPath = GetComponent<AIPath>();
        }

        public override void OnStart()
        {
            aiPath.enableRotation = coverFire.Value;
            RaycastHit hit;
            int raycastCount = 0;
            Vector3 startDirection = (transform.position-targetObject.Value.transform.position).normalized;
            startDirection.y = 0;
            var direction = startDirection;
            directions.Add(direction);
            float step = 0;
            bool toggleSide = false;
            var coverTarget = transform.position;
            int hits = 0;
            // Keep firing a ray until too many rays have been fired
            while (raycastCount < maxRaycasts.Value) {
                var ray = new Ray(transform.position+transform.up, direction);
                if (Physics.Raycast(ray, out hit, maxCoverDistance.Value, availableLayerCovers.value)) {
                    hits++;
                    // A suitable agent has been found. Find the opposite side of that agent by shooting a ray in the opposite direction from a point far away
                    if (hit.collider.Raycast(new Ray(hit.point - hit.normal * maxCoverDistance.Value, hit.normal), out hit, Mathf.Infinity)) {
                        coverPoint = hit.point;
                        coverTarget = hit.point + hit.normal * coverOffset.Value;
                        foundCover = true;
                        break;
                    }
                }

                
                step = toggleSide?-step:Mathf.Abs(step)+ rayStep.Value;
                toggleSide = !toggleSide;
                direction = Quaternion.Euler(0, transform.eulerAngles.y + step, 0) * startDirection;
                directions.Add(direction);
                raycastCount++;
            }
            Debug.Log(hits);
            if (foundCover) {
                Debug.Log("found cover at " + coverTarget); 
                SetDestination(coverTarget);
            }

            base.OnStart();
        }

        // Seek to the cover point. Return success as soon as the location is reached or the agent is looking at the cover point
        public override TaskStatus OnUpdate()
        {
            if (!foundCover) {
                return TaskStatus.Failure;
            }
            if(coverFire.Value)
            {
                var lookAtDirection = targetObject.Value.transform.position - transform.position;
                lookAtDirection.y = 0;
                var rotation = Quaternion.LookRotation(lookAtDirection);
                if(Quaternion.Angle(transform.rotation, rotation) > rotationEpsilon.Value)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime * maxLookAtRotationDelta.Value);


            }
            else if (HasArrived()) {
                var lookAtDirection = (lookAtTarget.Value?targetObject.Value.transform.position: coverPoint) - transform.position;
                lookAtDirection.y = 0;
                var rotation = Quaternion.LookRotation(lookAtDirection);
                // Return success if the agent isn't going to look at the cover point or it has completely rotated to look at the cover point
                if ((!lookAtCoverPoint.Value &&!lookAtTarget.Value) || Quaternion.Angle(transform.rotation, rotation) < rotationEpsilon.Value) {
                    return TaskStatus.Success;
                } else {
                    // Still needs to rotate towards the target
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime* maxLookAtRotationDelta.Value);
                }
            }

            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            aiPath.enableRotation = true;
            base.OnEnd();
        }

        // Reset the public variables
        public override void OnReset()
        {
            base.OnReset();

            maxCoverDistance = 1000;
            maxRaycasts = 100;
            rayStep = 1;
            coverOffset = 2;
            lookAtCoverPoint = false;
            rotationEpsilon = 0.5f;
        }

        public override void OnDrawGizmos()
        {
#if UNITY_EDITOR
     
                var oldColor = UnityEditor.Handles.color;
                UnityEditor.Handles.color = Color.green;
            if (foundCover)
            {
                UnityEditor.Handles.SphereHandleCap(0, coverPoint, Quaternion.identity, 1, EventType.Repaint);
            }
            for (int i = 0; i < directions.Count; i++)
                {
                    UnityEditor.Handles.DrawLine(transform.position, transform.position + directions[i] * (i + 1));
                }

                UnityEditor.Handles.color = oldColor;
            
#endif
        }
    }
}