using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TestEnemy : Enemy
{
    Vector3 lockRotation = Vector3.zero;
    RaycastHitbox hitbox;
    HitboxHits openedHitbox = null;


    // unity methods
    protected override void Start()
    {
        base.Start();
        hitbox = GetComponent<RaycastHitbox>();
    }


    // state methods
    public override void SetActable(int actable)
    {
        base.SetActable(actable);
        if (this.actable)
        {
            openedHitbox.KillHitbox();
            openedHitbox = null;
        }
    }

    public override void OnStun()
    {
        base.OnStun();
        openedHitbox.KillHitbox();
        openedHitbox = null;
    }

    public void EnableHitbox()
    {
        openedHitbox = hitbox.AddHitbox("Player", OnHit);
    }


    // other methods
    public void OnHit(RaycastHit hit) // hitbox callback
    {
        PlayerController player = hit.transform.GetComponent<PlayerController>();
        if (player.Invincible) return;
        player.OnStun();
    }


    // enemy state methods
    protected override void OnAttack()
    {
        base.OnAttack();
        if (!target) return;

        Vector3 lookVector = Vector3Utils.LookVector(transform.position, targetLastSeenPos);
        float distance = (transform.position - target.position).magnitude;

        if (distance > 2.5f) Move(lookVector);
        else Attack(lookVector);

        if (!movable) Watch(lockRotation);
    }

    void Attack(Vector3 lookVector)
    {
        if (!actable) return;

        actable = false;
        movable = false;
        lockRotation = lookVector;
        animator.applyRootMotion = true;

        animator.SetInteger("variant", 0);
        animator.SetTrigger("attack");
        rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
    }
}
