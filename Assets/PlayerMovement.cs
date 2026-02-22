using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Attack Dash")]
    public float attackDashForce = 6f;

    [Header("Roll")]
    public float rollForce = 12f;
    public float rollDuration = 0.25f;
    public float rollCooldown = 0.5f;

    private Rigidbody2D rb;
    private Animator anim;

    // ===== Attack =====
    private bool attackQueued = false;
    private bool isAttackDashing = false;

    // ===== Roll =====
    private bool isRolling = false;
    private float rollTimer;
    private float rollCooldownTimer;
    public int damage = 20;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        bool inAttack = anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");

        // ================= ATAK =================
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (!inAttack && !isRolling)
                anim.SetTrigger("Attack");
            else if (inAttack)
                attackQueued = true;
        }

        // ================= ROLL =================
        if (Input.GetKeyDown(KeyCode.LeftShift) &&
            !isRolling &&
            !inAttack &&
            rollCooldownTimer <= 0)
        {
            StartRoll();
        }

        // Roll fizycznie trwa okreœlony czas
        if (isRolling)
        {
            rollTimer -= Time.deltaTime;

            if (rollTimer <= 0)
            {
                EndRoll();
            }
        }

        // Cooldown
        if (rollCooldownTimer > 0)
            rollCooldownTimer -= Time.deltaTime;

        // ================= RUCH =================
        if (!inAttack && !isRolling)
        {
            rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
            anim.SetFloat("Speed", Mathf.Abs(moveInput));

            if (moveInput > 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (moveInput < 0)
                transform.localScale = new Vector3(1, 1, 1);
        }
        else if (!isAttackDashing && !isRolling)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            anim.SetFloat("Speed", 0);
        }
    }

    // ================= ROLL METHODS =================

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

    // Animation Event — dodajesz w konkretnej klatce rolla
    public void AllowRollExit()
    {
        anim.SetBool("CanExitRoll", true);
    }

    // ================= ATAK DASH EVENTS =================

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
            anim.SetTrigger("Attack");
        }
    }
    public void DealDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider2D enemy in enemies)
        {
            enemy.GetComponent<EnemyAI>()?.TakeDamage(damage);
        }
    }
}