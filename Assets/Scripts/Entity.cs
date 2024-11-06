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
    protected bool died = false;
    protected float staminaLastUsed = 0;
    protected float superArmorTime = 0;

    // properties
    public bool Movable
    {
        get { return movable; }
        set { movable = value; }
    }

    public bool Invincible
    {
        get { return invincible; }
    }

    public bool SuperArmor
    {
        get { return superArmorTime >= Time.time; }
    }

    public bool Died
    { 
        get { return died; }
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
    public virtual void SetActable(int actable)
    {
        this.actable = actable > 0;
        animator.applyRootMotion = !this.actable;
    }

    public virtual void SetMovable(int movable)
    {
        this.movable = movable > 0;
        animator.applyRootMotion = !this.movable;
    }

    public virtual void SetInvincible(int invincible)
    {
        this.invincible = invincible > 0;
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
        if (stamina >= maxStamina) return;
        if (staminaLastUsed + staminaRegenDelay < Time.time)
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
        actable = false;
        movable = false;
        invincible = true;
        died = true;
        gameObject.layer = 6; // Ghost entity
    }

    public virtual bool Stun(float stunDuration = 0)
    {
        if (SuperArmor) return false;
        this.superArmorTime = Time.time + stunDuration;

        movable = false;
        actable = false;
        return true;
    }
}
