using System.Collections;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Unity.SharedVariables
{
    [TaskCategory("Custom")]
    [TaskDescription("Sets the SharedBool variable to the specified object after a delay. Returns Success.")]
    public class SetSharedBoolWithDelay : Action
    {
        [Tooltip("The value to set the SharedBool to")]
        public SharedBool targetValue;
        [RequiredField]
        [Tooltip("The SharedBool to set")]
        public SharedBool targetVariable;
        public SharedFloat delay;
        public SharedFloat delayMin;
        public SharedFloat delayMax;
        public SharedBool randomDelay;

        public override TaskStatus OnUpdate()
        {
            StartCoroutine(SetBool());

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            targetValue = false;
            targetVariable = false;
        }

        IEnumerator SetBool()
        {
            float d = randomDelay.Value ? Random.Range(delayMin.Value, delayMax.Value) : delay.Value;
            yield return new WaitForSeconds(d);
            targetVariable.Value = targetValue.Value;
        }
    }
}