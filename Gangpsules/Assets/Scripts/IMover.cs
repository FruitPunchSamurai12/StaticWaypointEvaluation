using UnityEngine;

public interface IMover
{
    Vector3 Direction { get; }
    float Speed { get; }
}