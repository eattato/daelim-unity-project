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
    protected AnimatorManager animManager;

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

    public float Health
    {
        get { return health; }
    }

    public float MaxHealth
    {
        get { return maxHealth; }
    }


    // unity methods
    protected virtual void Start()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        animManager = GetComponent<AnimatorManager>();
    }

    protected virtual void Update()
    {
        RegenStamina();
    }


    // state methods (called by animator / StateMachineBehaviour)
    public virtual void EnableActable(AnimationEvent animEvent)
    {
        if (!animManager.IsValidEvent(animEvent)) return;
        actable = true;
    }

    public virtual void DisableActable(AnimationEvent animEvent)
    {
        if (!animManager.IsValidEvent(animEvent)) return;
        actable = false;
    }

    public virtual void EnableMovable(AnimationEvent animEvent)
    {
        if (!animManager.IsValidEvent(animEvent)) return;
        movable = true;
        animator.applyRootMotion = false;
    }

    public virtual void DisableMovable(AnimationEvent animEvent)
    {
        if (!animManager.IsValidEvent(animEvent)) return;
        movable = false;
        animator.applyRootMotion = true;
    }

    public virtual void EnableInvincible(AnimationEvent animEvent)
    {
        if (!animManager.IsValidEvent(animEvent)) return;
        invincible = true;
    }

    public virtual void DisableInvincible(AnimationEvent animEvent)
    {
        if (!animManager.IsValidEvent(animEvent)) return;
        invincible = false;
    }

    public virtual void EnableSuperArmor(AnimationEvent animEvent)
    {
        if (!animManager.IsValidEvent(animEvent)) return;
        superArmorTime = Time.time + 999;
    }

    public virtual void DisableSuperArmor(AnimationEvent animEvent)
    {
        if (!animManager.IsValidEvent(animEvent)) return;
        superArmorTime = 0;
    }

    public virtual void EnableParrying(AnimationEvent animEvent)
    {
        if (!animManager.IsValidEvent(animEvent)) return;
        parrying = true;
    }

    public virtual void DisableParrying(AnimationEvent animEvent)
    {
        if (!animManager.IsValidEvent(animEvent)) return;
        parrying = false;
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

    public virtual void Damage(float amount, Entity damageBy = null)
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
        AudioClip hitSound = SoundManager.Instance.hitSounds[Random.Range(0, SoundManager.Instance.hitSounds.Count)];
        SoundManager.Instance.CreateSoundPart(transform.position, hitSound, 10);

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
        parrying = false;

        try
        {
            float speed = stunMotion.length / stunDuration;
            animator.SetFloat("stunSpeed", speed);
            animator.SetTrigger("stun");
        } catch { }
        return true;
    }
}
