using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Entity
{
    [SerializeField] float rollSpeed = 10;
    [SerializeField] float rollTime = 0.5f;
    [SerializeField] float rollStunTime = 0.5f;
    [SerializeField] float rotationSmoothTime = 0.1f;

    Rigidbody rigid;
    Thruster thruster;
    CameraController camController;

    bool movable = true;
    bool invinsible = false;
    Vector3 rotationVelocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        thruster = GetComponent<Thruster>();
        camController = Camera.main.GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Roll();
    }

    Vector3 GetMoveDirection()
    {
        Vector3 forwardVec = Camera.main.transform.forward * Input.GetAxisRaw("Vertical");
        Vector3 rightVec = Camera.main.transform.right * Input.GetAxisRaw("Horizontal");

        Vector3 moveDirection = forwardVec + rightVec;
        moveDirection = Vector3.Normalize(new Vector3(moveDirection.x, 0, moveDirection.z));
        return moveDirection;
    }

    void Move()
    {
        if (!movable) return;

        Vector3 moveDirection = GetMoveDirection();
        rigid.AddForce(moveDirection * moveSpeed - rigid.velocity);

        if (camController.Lockon)
        {
            Vector3 lookVector = camController.GetLookVector(camController.TrackPos, camController.Lockon.position, true);
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

    void Roll()
    {
        if (movable && Input.GetKeyDown(KeyCode.Space))
        {
            movable = false;

            Vector3 moveDirection = GetMoveDirection();
            Vector3 rollVelocity = moveDirection * rollSpeed;
            thruster.AddThrust(rollVelocity, rollTime);

            IEnumerator co()
            {
                yield return new WaitForSeconds(rollTime);
                thruster.AddThrust(rollVelocity / 2, rollStunTime);
                yield return new WaitForSeconds(rollStunTime);
                movable = true;
            }

            StartCoroutine(co());
        }
    }
}
