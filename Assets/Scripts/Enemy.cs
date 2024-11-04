using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    [Header("배회 설정")]
    [SerializeField] List<Transform> wanderingNodes = new List<Transform>();
    [SerializeField] bool loopWandering = true;
    [SerializeField] float wanderingSpeed = 5;

    [Header("시야 설정")]
    [SerializeField] float sightAngle = 120f;
    [SerializeField] int sightSplit = 20;
    [SerializeField] float sightRange = 20;
    [SerializeField] Vector3 sightPos = Vector3.zero;

    [Header("경계 설정")]
    [SerializeField] float detectionMult = 1; // 감지 거리 배율
    [SerializeField] float onGuardTime = 10;
    [SerializeField] float followDistance = 20;

    [SerializeField] Transform target;

    Animator animator;

    Status status = Status.Wandering;
    Transform currentWanderingNode = null;
    bool wanderBackwards = false;
    float targetLastSeenTime = 0;
    Vector3 targetLastSeenPos = Vector3.zero;

    enum Status
    {
        Wandering,
        OnGuard,
        Attack
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (status)
        {
            case Status.Wandering:
                Wander();
                break;
            case Status.OnGuard:
                OnGuard();
                break;
            case Status.Attack:
                Attack();
                break;
        }
    }

    void OnDrawGizmos()
    {
        if (status == Status.Attack) return;

        float angle = sightAngle / sightSplit;
        for (int y = -sightSplit / 2; y <= sightSplit; y++)
        {
            for (int x = -sightSplit / 2; x <= sightSplit; x++)
            {
                Vector3 direction = Quaternion.Euler(new Vector3(angle * y, angle * x, 0)) * transform.forward;
                Vector3 detectionSightPos = transform.position + sightPos;

                Gizmos.color = Color.red;
                Gizmos.DrawLine(detectionSightPos, detectionSightPos + direction * sightRange);
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

    void DetectSight()
    {
        List<RaycastHit> hits = new List<RaycastHit>();

        float angle = sightAngle / sightSplit;
        for (int y = -sightSplit / 2; y <= sightSplit; y++)
        {
            for (int x = -sightSplit / 2; x <= sightSplit; x++)
            {
                Vector3 direction = Quaternion.Euler(new Vector3(angle * y, angle * x, 0)) * transform.forward;

                RaycastHit hit;
                bool isHit = Physics.Raycast(transform.position + sightPos, direction * sightRange, out hit);
                if (isHit) hits.Add(hit);
            }
        }

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.tag == "Player")
            {
                if (!target) target = hit.transform;
                if (target == hit.transform)
                {
                    status = Status.Attack;
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

    void Wander()
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

    void OnGuard()
    {
        if (targetLastSeenTime + onGuardTime < Time.time)
        {
            status = Status.Wandering;
            return;
        }

        // 감지
        DetectSight();
        DetectSound();

        // pathfind and moves to last seen pos
        Vector3 lookVector = Vector3Utils.LookVector(transform.position, targetLastSeenPos);
        Move(lookVector);
    }

    void Attack()
    {
        if (targetLastSeenTime + onGuardTime < Time.time)
        {
            status = Status.Wandering;
            return;
        }

        // pathfind and moves to last seen pos
        float distance = (transform.position - target.position).magnitude;
        if (distance <= followDistance)
        {
            targetLastSeenTime = Time.time;
            targetLastSeenPos = target.position;
        }

        // override under here
        if (distance > 5) {
            Vector3 lookVector = Vector3Utils.LookVector(transform.position, targetLastSeenPos);
            Move(lookVector);
        } else
        {
            // attack
        }
    }

    void Move(Vector3 direction)
    {
        if (!movable) return;

        Vector3 moveDirection = Vector3.Normalize(new Vector3(direction.x, 0, direction.z));
        float speed = status == Status.Wandering ? wanderingSpeed : moveSpeed;
        rigid.AddForce(moveDirection * speed - rigid.velocity);

        if (target)
        {
            Vector3 lookVector = Vector3Utils.LookVector(transform.position, target.position, true);
            transform.forward = Vector3.SmoothDamp(
                transform.forward, lookVector, ref rotationVelocity, rotationSmoothTime
            );
        }
        else if (moveDirection.magnitude > 0)
        {
            transform.forward = Vector3.SmoothDamp(
                transform.forward, moveDirection, ref rotationVelocity, rotationSmoothTime
            );
        }

        //if (moveDirection.magnitude > 0) animator.SetBool("walking", true);
        //else animator.SetBool("walking", false);
    }
}
