using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Entity
{
    [Header("공격 속성")]
    [SerializeField] float attackStaminaCost = 15;

    [Header("구르기 속성")]
    [SerializeField] float rollSpeed = 10;
    [SerializeField] float rollTime = 0.5f;
    [SerializeField] float rollStunTime = 0.5f;
    [SerializeField] float rollStaminaCost = 10;
    [SerializeField] Transform sword;
    [SerializeField] float walkMotionTransSpeed = 5;

    Thruster thruster;
    CameraController camController;
    RaycastHitbox hitbox;
    Vector3 walkMotionTrans = Vector3.zero;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        thruster = GetComponent<Thruster>();
        camController = Camera.main.GetComponent<CameraController>();
        hitbox = sword.GetComponent<RaycastHitbox>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        Move();
        Roll();
        Attack();
    }

    public override void OnStun()
    {
        base.OnStun();
        Debug.Log("On Stun");
        animator.SetTrigger("stun");
    }

    public override void EnableMove()
    {
        base.EnableMove();
        animator.applyRootMotion = false;
    }

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
            UseStamina(rollStaminaCost);

            Vector3 moveDirection = GetMoveDirection(true);
            Vector3 rollVelocity = moveDirection * rollSpeed;
            transform.forward = moveDirection;

            animator.applyRootMotion = true;
            animator.SetTrigger("roll");
            //thruster.AddThrust(rollVelocity, rollTime);

            //IEnumerator co()
            //{
                //yield return new WaitForSeconds(rollTime);
                //thruster.AddThrust(rollVelocity / 2, rollStunTime);
                //yield return new WaitForSeconds(rollStunTime);
                //movable = true;
            //}

            //StartCoroutine(co());
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
