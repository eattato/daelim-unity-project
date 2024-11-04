using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("엔티티 설정")]
    [SerializeField] protected float maxHealth = 100;
    [SerializeField] protected float health = 100;
    [SerializeField] protected float moveSpeed = 7;
    [SerializeField] protected float rotationSmoothTime = 0.1f;

    protected Rigidbody rigid;

    protected bool movable = true;
    protected bool invincible = false;
    protected Vector3 rotationVelocity = Vector3.zero;

    protected virtual void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }
}
