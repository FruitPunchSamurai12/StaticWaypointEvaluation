using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    [SerializeField] float distance = 20f;
    [SerializeField] Transform targetToFollow;
    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = new Vector3(targetToFollow.position.x, distance, targetToFollow.position.z);
    }
}
