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
    AnimationClip hurtMotion;

    
    // unity methods
    protected override void Start()
    {
        base.Start();
        thruster = GetComponent<Thruster>();
        camController = Camera.main.GetComponent<CameraController>();
        hitbox = sword.GetComponent<RaycastHitbox>();

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == "hit")
            {
                hurtMotion = clip;
                break;
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        Move();
        Roll();
        Attack();

        healthGauge.SetFilled(health / maxHealth);
        staminaGauge.SetFilled(stamina / maxStamina);

        AnimatorStateInfo animInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (animInfo.IsName("hit"))
        {
            //Debug.Log(animInfo.normalizedTime);
            if (animInfo.normalizedTime >= 1)
            {
                //EnableMove();
            }
        }
    }


    // state methods
    public override bool Stun(float stunDuration = 0)
    {
        bool applied = base.Stun(stunDuration);
        if (!applied) return applied;

        if (hurtMotion)
        {
            float speed = hurtMotion.length / stunDuration;
            animator.SetFloat("hurtSpeed", speed);
        }

        animator.SetTrigger("stun");
        return applied;
    }

    public override void SetActable(int actable)
    {
        base.SetActable(actable);
        //Debug.Log("set actable " + this.actable);
    }

    public override void SetMovable(int movable)
    {
        base.SetMovable(movable);
        //Debug.Log("set movable " + this.movable);
    }

    public override void Dead()
    {
        base.Dead();
        actable = false;
        movable = false;
        invincible = true;

        IEnumerator co()
        {
            yield return new WaitForSeconds(3);
            titleUI.AddTitle(titleUI.DeadLine, titleUI.DeadColor);
            yield return new WaitForSeconds(7);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        StartCoroutine(co());
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
        }
    }

    void Attack()
    {
        if (movable && Input.GetMouseButtonDown(0) && stamina >= attackStaminaCost)
        {
            movable = false;
            UseStamina(attackStaminaCost);
            rigid.velocity = Vector3.zero;

            Vector3 moveDirection = GetMoveDirection(true);
            //transform.forward = moveDirection;
            thruster.AddThrust(moveDirection * 5, 1);

            List<string> detectionTagList = new List<string>();
            detectionTagList.Add("Enemy");
            hitbox.AddHitbox(detectionTagList, 0.5f);

            IEnumerator co()
            {
                yield return new WaitForSeconds(0.5f);
                movable = true;
            }

            StartCoroutine(co());
        }
    }
}
