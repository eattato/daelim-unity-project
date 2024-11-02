using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 2;

    Rigidbody rigid;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forwardVec = Camera.main.transform.forward * Input.GetAxisRaw("Vertical");
        Vector3 rightVec = Camera.main.transform.right * Input.GetAxisRaw("Horizontal");

        Vector3 moveDirection = forwardVec + rightVec;
        moveDirection = Vector3.Normalize(new Vector3(moveDirection.x, 0, moveDirection.z));
        rigid.AddForce(moveDirection * moveSpeed - rigid.velocity);
    }
}
