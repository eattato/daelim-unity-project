using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pushblocker : MonoBehaviour
{
    List<Collider> GetColliderDescendants(Transform target, List<Collider> result = null)
    {
        if (result == null) result = new List<Collider>();

        Collider collider = target.GetComponent<Collider>();
        if (collider) result.Add(collider);

        foreach (Transform child in target)
        {
            GetColliderDescendants(child, result);
        }

        return result;
    }

    // Start is called before the first frame update
    void Start()
    {
        Collider collider = GetComponent<Collider>();
        List<Collider> targetColliders = GetColliderDescendants(transform.parent);

        foreach (Collider otherCollider in targetColliders)
        {
            Physics.IgnoreCollision(collider, otherCollider);
        }
    }
}
