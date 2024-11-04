using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3Utils
{
    public static Vector3 LookVector(Vector3 from, Vector3 to, bool removeY = false)
    {
        if (removeY) from = new Vector3(from.x, to.y, from.z);
        return Vector3.Normalize(to - from);
    }
}
