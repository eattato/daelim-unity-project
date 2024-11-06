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
        if (this.actable) // 재행동 가능 = 이전 행동 끝남, 이전 히트박스를 끝냄
        {
            openedHitbox.KillHitbox();
            openedHitbox = null;
        }
    }

    public override bool Stun(float stunDuration = 0)
    {
        bool applied = base.Stun(stunDuration);
        if (!applied) return applied;

        openedHitbox.KillHitbox();
        openedHitbox = null;
        return applied;
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
        player.Damage(30);
        player.Stun(1.45f); // 1타 힛박 시간 + 2타 선딜 + 힛박 시간까지 스턴
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
        rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
        if (!actable) return;

        actable = false;
        movable = false;
        lockRotation = lookVector;
        animator.applyRootMotion = true;

        superArmorDurability = 1.2f; // 강인도 설정, 공격 중엔 1.2초 짜리 스턴 버틸 수 있음
        superArmorTime = Time.time + 10; // 그냥 10초로 뒀다가 애니메이션 이벤트로 지울 예정

        animator.SetInteger("variant", 0);
        animator.SetTrigger("attack");
    }
}
