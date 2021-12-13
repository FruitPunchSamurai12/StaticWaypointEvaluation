using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Movement;

public class CanAttack : Conditional
{
    [Tooltip("The object that we are attacking")]
    public SharedGameObject targetObject;
    [Tooltip("The agent's weapon")]
    public SharedWeapon equippedWeapon;
    [Tooltip("The tag of the object that we are attacking")]
    public SharedString targetTag;
    [Tooltip("The agent's attack range")]
    public SharedFloat attackRange;
    [Tooltip("The object variable that will be set when a object is found what the object is")]
    public SharedGameObject returnedObject;
    [Tooltip("The LayerMask of the objects to ignore when performing the line of sight check")]
    public LayerMask ignoreLayerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
    [Tooltip("The LayerMask of the objects to check when doing the overlap sphere check")]
    public LayerMask obstaclesLayerMask;
    [Tooltip("The raycast offset relative to the pivot position")]
    public SharedVector3 offset;
    [Tooltip("The target raycast offset relative to the pivot position")]
    public SharedVector3 targetOffset;
    [Tooltip("Should a debug look ray be drawn to the scene view?")]
    public SharedBool drawDebugRay;

    private List<GameObject> objects;
    private float sqrMagnitude;
    BehaviorTree bt;
    Weapon w;
    public override void OnAwake()
    {
        bt = GetComponent<BehaviorTree>();
        w = equippedWeapon.Value.GetComponent<Weapon>();
    }


    public override void OnStart()
    {

        sqrMagnitude = attackRange.Value * attackRange.Value;
        if (objects != null)
        {
            objects.Clear();
        }
        else
        {
            objects = new List<GameObject>();
        }
        // if objects is null then find all of the objects using the layer mask or tag
        if (targetObject.Value == null)
        {
            if (!string.IsNullOrEmpty(targetTag.Value))
            {
                var gameObjects = GameObject.FindGameObjectsWithTag(targetTag.Value);
                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    objects.Add(gameObjects[i]);
                }
            }
        }
        else
        {
            objects.Add(targetObject.Value);
        }
    }

    public override TaskStatus OnUpdate()
    {
        Vector3 direction;
        // check each object. All it takes is one object to be able to return success
        for (int i = 0; i < objects.Count; ++i)
        {
            if (objects[i] == null)
            {
                continue;
            }
            string objectTag = objects[i].tag;
            direction = objects[i].transform.position - w.LineOfSightStart.position;
            direction.y = 0;
            if (Vector3.SqrMagnitude(direction) < sqrMagnitude)
            {
                RaycastHit hitInfo;
                if (Physics.SphereCast(w.LineOfSightStart.position,0.05f, w.transform.forward, out hitInfo, Mathf.Infinity, ~ignoreLayerMask))
                {
                    var obstacles = Physics.OverlapSphere(w.LineOfSightStart.position, .5f, obstaclesLayerMask);
                    if (obstacles.Length == 0 && hitInfo.collider.CompareTag(objectTag))
                    {
                        //Debug.Log(hitTransform.name);
                        returnedObject.Value = objects[i];
                        var playerLastPos = (SharedVector3)bt.GetVariable("PlayerLastKnownPosition");
                        playerLastPos.Value = objects[i].transform.position;
                        return TaskStatus.Success;
                    }
                }
            }
            else
            {
                return TaskStatus.Failure;
            }
        }

        return TaskStatus.Failure;

    }

}
