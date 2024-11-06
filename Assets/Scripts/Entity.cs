using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

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
    [SerializeField] protected float defaultSuperArmor = 0;

    [Header("모션 설정")]
    [SerializeField] protected AnimationClip stunMotion;

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
    protected float superArmorDurability = 0;
    protected bool parrying = false;

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

    public float SuperArmorDurability
    {
        get { return superArmorDurability; }
    }

    public bool Parrying
    {
        get { return parrying; }
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

    public virtual void SetSuperArmor(int superArmor)
    {
        // 0이면 슈퍼아머 삭제, 그 이상이면 해당 초 만큼 슈퍼아머 생성
        // 보통은 애니메이션 이벤트에서 삭제로만 씀
        this.superArmorTime = superArmor == 0 ? 0 : superArmor;
    }

    public virtual void SetParrying(int parrying)
    {
        this.parrying = parrying > 0;
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
        // 피격됐을때 슈퍼아머가 없거나 강인도가 약하면 깨져서 갱신됨
        bool stunned = true;
        if (!SuperArmor && defaultSuperArmor >= stunDuration) stunned = false; // 상시적용 슈퍼아머
        if (SuperArmor && Mathf.Max(superArmorDurability, defaultSuperArmor) >= stunDuration) stunned = false;

        if (!stunned)
        {
            try
            {
                animator.SetTrigger("hurt");
            } catch { }
            return stunned;
        }

        this.superArmorDurability = 999; // 스턴먹어서 생긴 슈퍼아머는 안 깨짐, 깨지면 무한스턴 되버림
        this.superArmorTime = Time.time + stunDuration;

        movable = false;
        actable = false;

        try
        {
            float speed = stunMotion.length / stunDuration;
            animator.SetFloat("stunSpeed", speed);
            animator.SetTrigger("stun");
        } catch { }
        return true;
    }
}
