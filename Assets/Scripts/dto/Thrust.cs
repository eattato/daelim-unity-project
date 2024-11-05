using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thrust
{
    float endTime;
    Vector3 force;
    public float EndTime
    {
        get { return endTime; }
    }

    public Vector3 Force
    {
        get { return force; }
    }

    public Thrust(Vector3 force, float duration)
    {
        this.endTime = Time.time + duration;
        this.force = force;
    }
}
