using Assets;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public PlayerAttackCollider attackCollider;

    [Header("Movement")]
    public float speed = 5f;

    [Header("Attack Dash")]
    public float attackDashForce = 6f;

    [Header("Roll")]
    public float rollForce = 12f;
    public float rollDuration = 0.25f;
    public float rollCooldown = 0.5f;

    [Header("Hit Knockback")]
    public float knockbackControlLockTime = 0.2f;

    [Header("Animation Fix")]
    public float postKnockbackAnimBlockTime = 0.08f;

    private Rigidbody2D rb;
    private Animator anim;

    private bool attackQueued = false;
    private bool isAttackDashing = false;

    private bool isRolling = false;
    private float rollTimer;
    private float rollCooldownTimer;

    public int damage = 20;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    private bool _damageDealtThisAttack;
    private bool movementLocked = false;
    private bool knockbackActive = false;
    private float knockbackTimer = 0f;

    private float postKnockbackAnimBlockTimer = 0f;

    [SerializeField] private Collider2D parryHitbox;
    [SerializeField] private PlayerParryHitbox parryScript;

    [SerializeField] private float parryCooldown = 0.5f;

    private float parryCooldownTimer = 0f;
    private bool isParrying = false;


    public void PlaySwingSound()
    {
        if (attackCollider != null)
        {
            attackCollider.PlaySwingSound();
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        bool inAttack = anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");

        if (knockbackActive)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                knockbackActive = false;
                postKnockbackAnimBlockTimer = postKnockbackAnimBlockTime;

                rb.velocity = new Vector2(0f, rb.velocity.y);
                anim.SetFloat("Speed", 0f);
            }
        }

        if (postKnockbackAnimBlockTimer > 0f)
        {
            postKnockbackAnimBlockTimer -= Time.deltaTime;
        }

        if (!movementLocked)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                if (!inAttack && !isRolling)
                    Attack();
                else if (inAttack)
                    attackQueued = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) &&
                !isRolling &&
                !inAttack &&
                rollCooldownTimer <= 0)
            {
                StartRoll();
            }
        }

        if (isRolling)
        {
            rollTimer -= Time.deltaTime;

            if (rollTimer <= 0)
            {
                EndRoll();
            }
        }

        if (rollCooldownTimer > 0)
            rollCooldownTimer -= Time.deltaTime;

        if (movementLocked)
        {
            anim.SetFloat("Speed", 0f);
            return;
        }

        if (knockbackActive)
        {
            anim.SetFloat("Speed", 0f);
            return;
        }

        if (postKnockbackAnimBlockTimer > 0f)
        {
            anim.SetFloat("Speed", 0f);
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (!inAttack && !isRolling && !isParrying)

        {
            rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

            float absInput = Mathf.Abs(moveInput);
            anim.SetFloat("Speed", absInput > 0.01f ? absInput : 0f);

            if (moveInput > 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (moveInput < 0)
                transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            anim.SetFloat("Speed", 0f);
        }
        if (parryCooldownTimer > 0)
            parryCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) &&
            !isRolling &&
            !isParrying &&
            parryCooldownTimer <= 0)
        {
            StartParry();
        }
    }

    public bool IsParrying()
    {
        return isParrying;
    }

    void StartParry()
    {
        isParrying = true;
        parryCooldownTimer = parryCooldown;

        anim.SetTrigger("Parry");
    }

    public void EndParry()
    {
        DisableParry();
        isParrying = false;
    }



    void StartRoll()
    {
        float direction = transform.localScale.x > 0 ? -1 : 1;

        isRolling = true;
        rollTimer = rollDuration;
        rollCooldownTimer = rollCooldown;

        anim.SetBool("CanExitRoll", false);
        anim.SetTrigger("Roll");

        rb.velocity = new Vector2(direction * rollForce, rb.velocity.y);
    }

    void EndRoll()
    {
        isRolling = false;
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    public void AllowRollExit()
    {
        anim.SetBool("CanExitRoll", true);
    }

    public void StartDash()
    {
        float direction = transform.localScale.x > 0 ? -1 : 1;
        rb.velocity = new Vector2(direction * attackDashForce, rb.velocity.y);
        isAttackDashing = true;
    }

    public void StopDash()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        isAttackDashing = false;
    }

    public void CheckCombo()
    {
        if (attackQueued)
        {
            attackQueued = false;
            Attack();
        }
    }

    private void Attack()
    {
        anim.SetTrigger("Attack");
        _damageDealtThisAttack = false;
    }

    public void DealDamage()
    {
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;

        if (locked)
        {
            anim.SetFloat("Speed", 0f);
        }
    }

    public void StartKnockbackLock()
    {
        knockbackActive = true;
        knockbackTimer = knockbackControlLockTime;
        anim.SetFloat("Speed", 0f);
    }

    public void EnableParry()
    {
        if (parryHitbox != null)
            parryHitbox.enabled = true;

        if (parryScript != null)
            parryScript.isActive = true;
    }

    public void DisableParry()
    {
        if (parryHitbox != null)
            parryHitbox.enabled = false;

        if (parryScript != null)
            parryScript.isActive = false;
    }




}