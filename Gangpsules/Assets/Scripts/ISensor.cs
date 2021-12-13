using System.Collections.Generic;
using UnityEngine;

public interface ISensor
{
    string TargetTag { get; }
    //List<GameObject> Targets { get; }
    Blackboard Blackboard { get; }
    void SenseTarget();
}