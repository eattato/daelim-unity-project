using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("엔티티 설정")]
    [SerializeField] protected float maxHealth = 100;
    [SerializeField] protected float health = 100;
    [SerializeField] protected float moveSpeed = 7;
    [SerializeField] protected float rotationSpeed = 10f;
    [SerializeField] protected float maxStamina = 100;
    [SerializeField] protected float stamina = 100;
    [SerializeField] protected float staminaRegen = 0.5f;
    [SerializeField] protected float staminaRegenDelay = 0.5f;

    // componentes
    protected Rigidbody rigid;
    protected Animator animator;

    // variables
    protected bool actable = true;
    protected bool movable = true;
    protected bool invincible = false;
    protected float staminaLastUsed = 0;

    // properties
    public bool Movable
    {
        get { return movable; }
        set { movable = value; }
    }


    // unity methods
    protected virtual void Start()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        RegenStamina();
    }


    // state methods (called by animator / StateMachineBehaviour)
    public virtual void EnableAct()
    {
        actable = true;
    }

    public virtual void EnableMove()
    {
        movable = true;
    }

    public virtual void OnStun()
    {
        movable = false;
        actable = false;
    }


    // other methods
    protected virtual void Watch(Vector3 direction)
    {
        direction = new Vector3(direction.x, 0, direction.z).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
    
    protected virtual void RegenStamina()
    {
        if (stamina < maxStamina && staminaLastUsed + staminaRegenDelay < Time.deltaTime)
        {
            stamina += maxStamina * staminaRegen * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
        }
    }

    protected virtual void UseStamina(float amount)
    {
        stamina -= amount;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        staminaLastUsed = Time.time;
    }

    public virtual void Damage(float amount)
    {
        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (health == 0) Dead();
    }

    public virtual void Dead()
    {
        health = 0;
    }
}
