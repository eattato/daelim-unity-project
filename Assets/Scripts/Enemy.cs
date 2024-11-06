using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    [Header("배회 설정")]
    [SerializeField] protected List<Transform> wanderingNodes = new List<Transform>();
    [SerializeField] protected bool loopWandering = true;
    [SerializeField] protected float wanderingSpeed = 5;

    [Header("시야 설정")]
    [SerializeField] protected float sightAngle = 120f;
    [SerializeField] protected int sightSplit = 20;
    [SerializeField] protected float sightRange = 20;
    [SerializeField] protected Vector3 sightPos = Vector3.zero;

    [Header("경계 설정")]
    [SerializeField] protected float detectionMult = 1; // 감지 거리 배율
    [SerializeField] protected float onGuardTime = 10;
    [SerializeField] protected float followDistance = 20;

    [SerializeField] protected Transform target;

    // variables
    protected Status status = Status.Wandering;
    protected Transform currentWanderingNode = null;
    protected bool wanderBackwards = false;
    protected float targetLastSeenTime = 0;
    protected Vector3 targetLastSeenPos = Vector3.zero;

    protected enum Status
    {
        Wandering,
        OnGuard,
        OnAttack
    }

    
    // unity methods
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        switch (status)
        {
            case Status.Wandering:
                Wander();
                break;
            case Status.OnGuard:
                OnGuard();
                break;
            case Status.OnAttack:
                OnAttack();
                break;
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (status == Status.OnAttack) return;

        float angle = sightAngle / sightSplit;
        for (int y = -sightSplit / 2; y <= sightSplit; y++)
        {
            for (int x = -sightSplit / 2; x <= sightSplit; x++)
            {
                //Vector3 direction = Quaternion.Euler(new Vector3(angle * y, angle * x, 0)) * transform.forward;
                Vector3 direction = Quaternion.AngleAxis(angle * x, transform.right) * transform.forward;
                direction = Quaternion.AngleAxis(angle * y, transform.up) * direction;

                Vector3 detectionSightPos = transform.position + sightPos;
                Vector3 rayEndPoint = detectionSightPos + direction * sightRange;

                RaycastHit hit;
                bool isHit = Physics.Raycast(transform.position + sightPos, direction, out hit, sightRange);
                if (isHit) rayEndPoint = hit.point;

                Gizmos.color = Color.red;
                Gizmos.DrawLine(detectionSightPos, rayEndPoint);
            }
        }

        foreach (GameObject soundPart in GameObject.FindGameObjectsWithTag("Sound"))
        {
            float distance = (transform.position - soundPart.transform.position).magnitude;
            SoundPart soundPartData = soundPart.GetComponent<SoundPart>();

            float range = soundPartData.SoundRange * detectionMult;
            if (distance < range * 2)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(soundPart.transform.position, range);
            }
        }
    }


    // state methods


    // utils
    void DetectSight()
    {
        List<RaycastHit> hits = new List<RaycastHit>();

        float angle = sightAngle / sightSplit;
        for (int y = -sightSplit / 2; y <= sightSplit; y++)
        {
            for (int x = -sightSplit / 2; x <= sightSplit; x++)
            {
                //Vector3 direction = Quaternion.Euler(new Vector3(angle * y, angle * x, 0)) * transform.forward;
                Vector3 direction = Quaternion.AngleAxis(angle * x, transform.right) * transform.forward;
                direction = Quaternion.AngleAxis(angle * y, transform.up) * direction;

                RaycastHit hit;
                bool isHit = Physics.Raycast(transform.position + sightPos, direction, out hit, sightRange);
                if (isHit) hits.Add(hit);
            }
        }

        foreach (RaycastHit hit in hits)
        {
            Entity targetEntity = hit.transform.GetComponent<Entity>();
            if (targetEntity == null) continue;
            if (targetEntity.Died) continue;

            if (hit.transform.gameObject.tag == "Player")
            {
                if (!target) target = hit.transform;
                if (target == hit.transform)
                {
                    status = Status.OnAttack;
                    targetLastSeenTime = Time.time;
                    targetLastSeenPos = hit.transform.position;
                }
            }
        }
    }

    void DetectSound()
    {
        List<Vector3> detection = new List<Vector3>();

        foreach (GameObject soundPart in GameObject.FindGameObjectsWithTag("Sound"))
        {
            float distance = (transform.position - soundPart.transform.position).magnitude;
            SoundPart soundPartData = soundPart.GetComponent<SoundPart>();

            if (distance - soundPartData.SoundRange * detectionMult <= 0)
            {
                detection.Add(soundPart.transform.position);
            }
        }

        if (detection.Count == 0) return;
        if (status == Status.Wandering || status == Status.OnGuard)
        {
            Vector3 nearest = detection[0];
            foreach (Vector3 soundPos in detection)
            {
                float oldDist = (transform.position - nearest).magnitude;
                float currentDist = (transform.position - soundPos).magnitude;

                if (currentDist < oldDist) nearest = soundPos;
            }

            targetLastSeenTime = Time.time;
            targetLastSeenPos = nearest;
        }
    }


    // other methods
    protected virtual void Move(Vector3 direction)
    {
        if (!movable) return;

        Vector3 moveDirection = Vector3.Normalize(new Vector3(direction.x, 0, direction.z));
        float speed = status == Status.Wandering ? wanderingSpeed : moveSpeed;
        rigid.AddForce(moveDirection * speed - rigid.velocity);

        if (target)
        {
            Vector3 lookVector = Vector3Utils.LookVector(transform.position, target.position, true);
            Quaternion lookRotation = Quaternion.LookRotation(lookVector);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        else if (moveDirection.magnitude > 0)
        {
            Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        animator.SetBool("walking", moveDirection.magnitude > 0);
        animator.SetBool("sprint", status == Status.OnAttack);
    }


    // enemy state methods
    protected virtual void Wander()
    {
        // 감지
        DetectSight();
        DetectSound();
        if (status != Status.Wandering) return;

        // 이동
        if (wanderingNodes.Count == 0) return;
        if (currentWanderingNode == null)
        {
            currentWanderingNode = wanderingNodes[0];
        }

        // pathfind and moves to next wandering node
        Vector3 lookVector = Vector3Utils.LookVector(transform.position, currentWanderingNode.position);
        Move(lookVector);

        if ((transform.position - currentWanderingNode.position).magnitude < 0.01f)
        {
            int index = wanderingNodes.IndexOf(currentWanderingNode);
            index += wanderBackwards ? -1 : 1;

            if (index >= 0 && index < wanderingNodes.Count)
            {
                currentWanderingNode = wanderingNodes[index];
            } else if (loopWandering)
            {
                wanderBackwards = !wanderBackwards;
            }
        }
    }

    protected virtual void OnGuard()
    {
        if (targetLastSeenTime + onGuardTime < Time.time)
        {
            Debug.Log("return to wander");
            status = Status.Wandering;
            target = null;
            return;
        }

        // 감지
        DetectSight();
        DetectSound();

        // pathfind and moves to last seen pos
        Vector3 lookVector = Vector3Utils.LookVector(transform.position, targetLastSeenPos);
        Move(lookVector);
    }

    protected virtual void OnAttack()
    {
        Entity targetEntity = target.GetComponent<Entity>();
        bool noTarget = target == null;
        bool targetLost = targetLastSeenTime + onGuardTime < Time.time;

        if (noTarget || targetLost || targetEntity.Died)
        {
            Debug.Log("return to wander");
            status = Status.Wandering;
            target = null;
            return;
        }

        // 감지
        DetectSight();

        // pathfind and moves to last seen pos
        float distance = (transform.position - target.position).magnitude;
        if (distance <= followDistance)
        {
            targetLastSeenTime = Time.time;
            targetLastSeenPos = target.position;
        }
    }
}
