using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : Enemy
{
    Vector3 lockRotation = Vector3.zero;
    RaycastHitbox hitbox;
    HitboxHits openedHitbox = null;

    protected override void Start()
    {
        base.Start();
        hitbox = GetComponent<RaycastHitbox>();
    }

    // 히트 프레임 종료 & 스턴 시 히트박스 삭제
    public override void EnableAct()
    {
        base.EnableAct();
        openedHitbox.KillHitbox();
        openedHitbox = null;
    }

    public override void OnStun()
    {
        base.OnStun();
        openedHitbox.KillHitbox();
        openedHitbox = null;
    }

    // state machine
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

        openedHitbox = hitbox.AddHitbox("Player");
    }
}
