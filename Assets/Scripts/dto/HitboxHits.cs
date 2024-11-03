using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxHits
{
    private List<string> detectionTagList;
    private List<Transform> filter;
    private float endTime;

    public float EndTime
    {
        get { return endTime; }
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
}
