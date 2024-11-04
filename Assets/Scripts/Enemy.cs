using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    [SerializeField] List<Transform> wanderingNodes = new List<Transform>();
    [SerializeField] bool loopWandering = true;
    [SerializeField] Transform target;

    Status status = Status.Wandering;
    Transform currentWanderingNode = null;
    bool wanderBackwards = false;

    enum Status
    {
        Wandering,
        OnGuard,
        Approaching
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        switch (status)
        {
            case Status.Wandering:
                Wander();
                break;
        }
    }

    void Wander()
    {
        if (wanderingNodes.Count == 0) return;
        if (currentWanderingNode == null)
        {
            currentWanderingNode = wanderingNodes[0];
        }

        // pathfind and moves to next wandering node

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

    void Move(Vector3 direction)
    {
        if (!movable) return;

        Vector3 moveDirection = Vector3.Normalize(new Vector3(direction.x, 0, direction.z));
        rigid.AddForce(moveDirection * moveSpeed - rigid.velocity);

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
    }
}
