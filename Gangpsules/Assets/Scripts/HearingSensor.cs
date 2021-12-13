using System.Collections.Generic;
using UnityEngine;

public class HearingSensor : MonoBehaviour, ISensor
{
    [SerializeField] string targetTag;
    [SerializeField] float hearingRange;
    float hearingRangeSquared;

    Blackboard blackboard;
    Dictionary<GameObject, IMover> targets = new Dictionary<GameObject, IMover>();

    public string TargetTag => targetTag;

    

    public Blackboard Blackboard => blackboard;


    private void Awake()
    {
        hearingRangeSquared = hearingRange * hearingRange;
        blackboard = GetComponent<Blackboard>();
        UpdateTargets();
    }

    private void UpdateTargets()
    {
        targets.Clear();
        foreach (var target in GameObject.FindGameObjectsWithTag(targetTag))
        {
            if (target.transform.parent == null)
                targets.Add(target,target.GetComponent<IMover>());
        }
    }
    public void SenseTarget()
    {
        float minDistance = Mathf.Infinity;
        GameObject closestTarget = null;
        foreach (var target in targets)
        {
            float sqrMagnitude = Vector3.SqrMagnitude(target.Key.transform.position - transform.position);
            if(sqrMagnitude<=hearingRangeSquared)
            {
                if (target.Value.Direction != Vector3.zero)
                {
                    if(sqrMagnitude<minDistance)
                    {
                        minDistance = sqrMagnitude;
                        closestTarget = target.Key;
                    }
                }
            }
        }
        if(closestTarget!=null)
            blackboard.HeardTarget(closestTarget);
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Color color = new Color(0, 1, 0, 0.25f);
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, hearingRange);
    }
#endif
}
