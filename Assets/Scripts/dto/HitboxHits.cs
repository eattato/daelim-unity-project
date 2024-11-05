using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxHits
{
    private List<string> detectionTagList;
    private List<Transform> filter;
    private bool died = false;
    private bool useLifetime = true;
    private float endTime = 0;

    public bool UseLifetime
    {
        get { return useLifetime; }
    }

    public float EndTime
    {
        get { return endTime; }
    }

    public HitboxHits(List<string> detectionTagList)
    {
        this.detectionTagList = detectionTagList;
        this.filter = new List<Transform>();
        this.useLifetime = false;
    }

    public HitboxHits(List<string> detectionTagList, float duration)
    {
        this.detectionTagList = detectionTagList;
        this.filter = new List<Transform>();
        this.endTime = Time.time + duration;
    }

    public void Hit(List<RaycastHit> hits)
    {
        foreach (RaycastHit hit in hits)
        {
            if (!detectionTagList.Contains(hit.transform.gameObject.tag)) continue;
            if (filter.Contains(hit.transform)) continue;

            filter.Add(hit.transform);
            Debug.Log(hit.transform.gameObject.name);
        }
    }

    public bool IsDead()
    {
        if (useLifetime && Time.time > endTime) died = true;
        return died;
    }

    public void KillHitbox()
    {
        died = true;
    }
}
