using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public int maxHealth = 3;
    public float moveSpeed = 2f;
    public float chaseRange = 5f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;

    private int currentHealth;
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;

    private bool isDead = false;
    private bool isAttacking = false;
    private float lastAttackTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
        else if (distance <= chaseRange && !isAttacking)
        {
            Chase();
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            anim.SetBool("IsWalking", false);
        }
    }

    void Chase()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(dir.x * moveSpeed, rb.velocity.y);

        anim.SetBool("IsWalking", true);

        if (dir.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (dir.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void Attack()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        anim.SetBool("IsWalking", false);
        anim.SetTrigger("Attack");

        lastAttackTime = Time.time;
        Invoke(nameof(ResetAttack), 0.8f); // dopasuj do długości animacji
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        anim.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        anim.SetBool("IsDead", true);
    }
}