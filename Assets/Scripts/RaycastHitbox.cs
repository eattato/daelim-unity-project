using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RaycastHitbox : MonoBehaviour
{
    [SerializeField] List<Transform> hitPoints = new List<Transform>();

    List<HitboxHits> hitboxOpened;
    Dictionary<Transform, Vector3> oldPoints;

    // Start is called before the first frame update
    void Awake()
    {
        hitboxOpened = new List<HitboxHits>();
        oldPoints = new Dictionary<Transform, Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        List<HitboxHits> nextHitboxOpened = new List<HitboxHits>();

        foreach (HitboxHits hitbox in hitboxOpened)
        {
            if (hitbox.IsDead()) continue;
            nextHitboxOpened.Add(hitbox);

            List<RaycastHit> totalHits = new List<RaycastHit>();
            foreach (Transform point in hitPoints)
            {
                Vector3 oldPos = oldPoints.ContainsKey(point) ? oldPoints[point] : point.position;
                Vector3 direction = point.position - oldPos;
                RaycastHit[] hits = Physics.RaycastAll(oldPos, direction.normalized, direction.magnitude);
                totalHits.AddRange(hits);
                oldPoints[point] = point.position;
            }

            hitbox.Hit(totalHits);
        }

        hitboxOpened = nextHitboxOpened;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (hitboxOpened.Count == 0) return;

        foreach (Transform point in hitPoints)
        {
            Vector3 oldPos = oldPoints.ContainsKey(point) ? oldPoints[point] : point.position;
            Gizmos.DrawLine(oldPos, point.position);
        }
    }

    public HitboxHits AddHitbox(string detectionTag, Action<RaycastHit> onHit = null)
    {
        List<string> detectionTagList = new List<string>();
        detectionTagList.Add(detectionTag);
        return AddHitbox(detectionTagList, onHit);
    }

    public HitboxHits AddHitbox(List<string> detectionTagList, Action<RaycastHit> onHit = null)
    {
        HitboxHits hitbox = new HitboxHits(detectionTagList, onHit);
        hitboxOpened.Add(hitbox);
        return hitbox;
    }

    public void AddHitbox(List<string> detectionTagList, float duration)
    {
        HitboxHits hitbox = new HitboxHits(detectionTagList, duration);
        hitboxOpened.Add(hitbox);
    }
}
