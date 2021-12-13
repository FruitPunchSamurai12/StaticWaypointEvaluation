using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightSensor : MonoBehaviour,ISensor
{
    [SerializeField] string targetTag;
    [SerializeField] float sightRange;
    [SerializeField] float sightAngle;
    [SerializeField] Transform eyes;
    [SerializeField] LayerMask obstaclesLayer;

    float sightRangeSquared;

    Blackboard blackboard;
    List<GameObject> targets = new List<GameObject>();

    public string TargetTag => targetTag;

    //public List<GameObject> Targets => targets;

    public Blackboard Blackboard => blackboard;


    private void Awake()
    {
        sightRangeSquared = sightRange * sightRange;
        blackboard = GetComponent<Blackboard>();
        UpdateTargets();
    }

    private void UpdateTargets()
    {
        targets.Clear();
        foreach (var target in GameObject.FindGameObjectsWithTag(targetTag))
        {
            if (target.transform.parent == null)
                targets.Add(target);
        }
    }
    public void SenseTarget()
    {
        foreach (var target in targets)
        {
            Vector3 targetPosition = target.transform.position;
            Vector3 targetDir = targetPosition.FlatVector() - transform.position.FlatVector();
            float angle = Vector3.Angle(targetDir.FlatVector(), eyes.transform.forward.FlatVector());
            float distance = Vector3.Distance(targetPosition.FlatVector(),eyes.transform.position.FlatVector());
            if (distance < sightRange && angle < sightAngle)
            {
                RaycastHit hit;
                Physics.Raycast(eyes.position, targetDir.normalized, out hit, distance, obstaclesLayer);
                if (hit.collider== null || hit.collider.CompareTag(targetTag))
                {
                    blackboard.SeenTarget(target);
                }
                else
                {
                    blackboard.LostTarget(target);
                }
            }
            else
                blackboard.LostTarget(target);
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Color color = new Color(0, 0, 1, 0.25f);
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawSolidArc(eyes.transform.position, Vector3.up, Quaternion.Euler(0, -sightAngle, 0) *transform.forward, sightAngle*2f, sightRange);
    }
#endif
}
