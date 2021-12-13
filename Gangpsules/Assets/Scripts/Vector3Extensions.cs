using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions 
{
    public static float FlatVectorDistanceSquared(this Vector3 v1,Vector3 v2)
    {
        return Vector3.SqrMagnitude(FlatVector(v1) - FlatVector(v2));
    }

    public static Vector3 FlatVector(this Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }
}
