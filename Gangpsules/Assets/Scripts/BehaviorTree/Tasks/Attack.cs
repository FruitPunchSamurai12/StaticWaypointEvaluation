using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Movement;
using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using Pathfinding;

[TaskDescription("Attack with the equipped weapon")]
[TaskCategory("Custom")]
public class Attack : Action
{
    [Tooltip("The agent's weapon")]
    public SharedWeapon equippedWeapon;
    [Tooltip("The target we are attacking")]
    public SharedGameObject targetObject;

    Weapon w;


    public override TaskStatus OnUpdate()
    {
        w = equippedWeapon.Value;
        if (w == null)
            return TaskStatus.Failure;       
        w.Attack(w.transform.forward);       
        return TaskStatus.Running;
    }

}
