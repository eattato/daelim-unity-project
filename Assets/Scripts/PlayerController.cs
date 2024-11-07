using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : Entity
{
    [Header("UI 바인딩")]
    [SerializeField] GaugeUI healthGauge;
    [SerializeField] GaugeUI manaGauge;
    [SerializeField] GaugeUI staminaGauge;
    [SerializeField] TitleUI titleUI;

    [Header("공격 속성")]
    [SerializeField] float attackStaminaCost = 15;

    [Header("구르기 속성")]
    [SerializeField] float rollSpeed = 10;
    [SerializeField] float rollTime = 0.5f;
    [SerializeField] float rollStaminaCost = 10;
    [SerializeField] Transform sword;
    [SerializeField] float walkMotionTransSpeed = 5;

    // components
    Thruster thruster;
    CameraController camController;
    RaycastHitbox hitbox;

    // variables
    Vector3 walkMotionTrans = Vector3.zero;
    Vector3 lockRotation = Vector3.zero;
    HitboxHits openedHitbox = null;


    // unity methods
    protected override void Start()
    {
        base.Start();
        thruster = GetComponent<Thruster>();
        camController = Camera.main.GetComponent<CameraController>();
        hitbox = sword.GetComponent<RaycastHitbox>();
        animManager.AddEndedAction("action", ActionEnded);
    }

    protected override void Update()
    {
        base.Update();

        Move();
        Roll();
        Attack(transform.forward);
        Parry();

        healthGauge.SetFilled(health / maxHealth);
        staminaGauge.SetFilled(stamina / maxStamina);
    }


    // state methods
    public override bool Stun(float stunDuration = 0)
    {
        bool applied = base.Stun(stunDuration);
        if (!applied) return applied;

        if (openedHitbox != null)
        {
            openedHitbox.KillHitbox();
            openedHitbox = null;
        }

        return applied;
    }

    public override void SetActable(int actable)
    {
        base.SetActable(actable);
        if (!this.actable) return;
        if (openedHitbox != null)
        {
            openedHitbox.KillHitbox();
            openedHitbox = null;
        }
    }

    public override void Dead()
    {
        base.Dead();
        animator.SetTrigger("died");

        IEnumerator co()
        {
            yield return new WaitForSeconds(3);
            titleUI.AddTitle(titleUI.DeadLine, titleUI.DeadColor);
            yield return new WaitForSeconds(7);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        StartCoroutine(co());
    }

    void Attack(Vector3 lookVector)
    {
        if (!Input.GetMouseButtonDown(0)) return;

        rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
        if (!actable) return;

        actable = false;
        movable = false;
        lockRotation = lookVector;
        animator.applyRootMotion = true;

        superArmorDurability = 0.5f;
        superArmorTime = Time.time + 10;

        //animator.SetInteger("variant", 0);
        animator.SetTrigger("attack");
    }

    public void EnableHitbox()
    {
        if (openedHitbox != null) openedHitbox.KillHitbox();
        openedHitbox = hitbox.AddHitbox("Enemy", OnHit);
    }

    public void ActionEnded()
    {
        if (!died)
        {
            actable = true;
            movable = true;
        }
    }


    // other methods
    Vector3 GetMoveDirection(bool getForward = false)
    {
        Vector3 forwardVec = Camera.main.transform.forward * Input.GetAxisRaw("Vertical");
        Vector3 rightVec = Camera.main.transform.right * Input.GetAxisRaw("Horizontal");

        Vector3 moveDirection = forwardVec + rightVec;
        moveDirection = Vector3.Normalize(new Vector3(moveDirection.x, 0, moveDirection.z));

        if (getForward && moveDirection.magnitude < 0.01f)
        {
            moveDirection = transform.forward;
        }
        return moveDirection;
    }

    public void OnHit(RaycastHit hit) // hitbox callback
    {
        Enemy enemy = hit.transform.GetComponent<Enemy>();
        if (enemy.Invincible) return;
        enemy.Damage(20);
        enemy.Stun(0.5f); // 1타 힛박 시간 + 2타 선딜 + 힛박 시간까지 스턴
    }

    public override void Damage(float amount, Entity damageBy = null)
    {
        base.Damage(amount, damageBy);
        AudioClip hurtSound = SoundManager.Instance.hurtSounds[Random.Range(0, SoundManager.Instance.hurtSounds.Count)];
        SoundManager.Instance.CreateSoundPart(transform.position, hurtSound, 10);
        camController.AddCamShake(3, 1);
    }

    void Move()
    {
        if (!movable) return;

        Vector3 moveDirection = GetMoveDirection();
        rigid.AddForce(moveDirection * moveSpeed - rigid.velocity);

        if (camController.Lockon)
        {
            Vector3 lookVector = Vector3Utils.LookVector(camController.TrackPos, camController.Lockon.position, true);
            Quaternion lookRotation = Quaternion.LookRotation(lookVector);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        else if (moveDirection.magnitude > 0)
        {
            Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        animator.SetBool("walking", moveDirection.magnitude > 0);
        if (!camController.Lockon)
        {
            animator.SetInteger("forward", 1);
            animator.SetInteger("right", 0);
        }
        else
        {
            float forwardUnit = Vector3.Dot(transform.forward, moveDirection);
            float rightUnit = Vector3.Dot(transform.right, moveDirection);

            walkMotionTrans = Vector3.Lerp(walkMotionTrans, new Vector3(rightUnit, 0, forwardUnit), Time.deltaTime * walkMotionTransSpeed);
            animator.SetFloat("forward", walkMotionTrans.z);
            animator.SetFloat("right", walkMotionTrans.x);
        }
    }

    void Roll()
    {
        if (!movable || !actable) return;
        if (Input.GetKeyDown(KeyCode.Space) && stamina >= rollStaminaCost)
        {
            actable = false;
            movable = false;
            invincible = true;
            UseStamina(rollStaminaCost);

            Vector3 moveDirection = GetMoveDirection(true);
            Vector3 rollVelocity = moveDirection * rollSpeed;
            transform.forward = moveDirection;

            animator.applyRootMotion = false;
            animator.SetTrigger("roll");
            thruster.AddThrust(rollVelocity, rollTime);

            AudioClip rollSound = SoundManager.Instance.rollSound;
            SoundManager.Instance.CreateSoundPart(transform.position, rollSound, 10);
        }
    }

    void Parry()
    {
        if (!movable || !actable) return;
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            actable = false;
            movable = false;

            Vector3 moveDirection = GetMoveDirection(true);
            Vector3 rollVelocity = moveDirection * rollSpeed;
            transform.forward = moveDirection;

            animator.applyRootMotion = false;
            animator.SetTrigger("parry");
        }
    }
}
