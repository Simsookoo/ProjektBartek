using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public int maxHealth = 3;
    public float moveSpeed = 2f;
    public float pushSpeed = 10f;
    public float chaseRange = 5f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public AnimationCurve pushForceInTime;
    public AnimationCurve DeadForceInTime;

    private int currentHealth;
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;

    private bool isDead = false;
    private bool isAttacking = false;
    private float lastAttackTime;

    private Coroutine pushCoroutine = null;
    private bool balanced = true;

    private Vector3? positionStartKnockback;
    private Vector3? positionEndKnockBack;

    private int hitAnimationIndex = 0;
    private int maxHitAnimationIndex = 1;

    [SerializeField] private GameObject hitEffect1;
    [SerializeField] private GameObject hitEffect2;
    [SerializeField] private Transform hitEffectPoint;
    [SerializeField] private float hitStopLight = 0.04f;
    [SerializeField] private float hitStopDeath = 0.08f;

    private bool useSecondHitEffect = false;

    private void ShowHitEffect()
    {
        GameObject effectToSpawn = useSecondHitEffect ? hitEffect2 : hitEffect1;

        if (effectToSpawn != null && hitEffectPoint != null)
        {
            Instantiate(effectToSpawn, hitEffectPoint.position, Quaternion.identity);
        }

        useSecondHitEffect = !useSecondHitEffect;
    }

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

        bool hit = anim.GetCurrentAnimatorStateInfo(0).IsTag("Hit");
        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");

        if (hit && !balanced) return;

        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown && !hit)
        {
            Attack();
        }
        else if (distance <= chaseRange && !isAttacking && !hit)
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
        //isAttacking = true;
        rb.velocity = Vector2.zero;

        anim.SetBool("IsWalking", false);
        anim.SetTrigger("Attack");

        lastAttackTime = Time.time;
        //StartCoroutine(ResetAttackRealtime(0.8f));
    }
    //private IEnumerator ResetAttackRealtime(float delay)
    //{
    //    yield return new WaitForSecondsRealtime(delay);
    //    isAttacking = false;
    //}

    public void TakeDamage(int damage, int? overrideAnimationIndex)
    {
        if (isDead) return;

        currentHealth -= damage;

        ShowHitEffect();

        if (currentHealth > 0)
        {
            if (HitStopManager.Instance != null)
                HitStopManager.Instance.Stop(hitStopLight);

            anim.SetTrigger("Hit");
            anim.SetInteger("HitIndex", overrideAnimationIndex != null ? overrideAnimationIndex.Value : hitAnimationIndex);
            Debug.Log($"hitIndex: {hitAnimationIndex}");

            if (overrideAnimationIndex == null)
                hitAnimationIndex = ++hitAnimationIndex > maxHitAnimationIndex ? 0 : hitAnimationIndex;

            Debug.Log($"SetTrigger(Hit) frame={Time.frameCount} from={gameObject.name}", this);
        }
        if (currentHealth <= 0)
        {
            if (HitStopManager.Instance != null)
                HitStopManager.Instance.Stop(hitStopDeath);

            Die();
        }
    }

    void Die()
    {
        isDead = true;

        anim.SetTrigger("IsDead");
    }

    void FreezeRagdoll()
    {
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void StartControlledPush()
    {
        Debug.Log($"Hit event frame={Time.frameCount} norm={GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime} animName: {GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).fullPathHash}");
        Debug.Log("StartControlledPush ");
        StopCoroutine();
        balanced = false;
        pushCoroutine = StartCoroutine(PushCoroutine(pushForceInTime));
    }

    public void StartDeadPush()
    {
        Debug.Log($"Hit event frame={Time.frameCount} norm={GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime} animName: {GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).fullPathHash}");
        Debug.Log("StartControlledPush ");
        StopCoroutine();
        balanced = false;
        pushCoroutine = StartCoroutine(PushCoroutine(DeadForceInTime));
    }

    private IEnumerator PushCoroutine(AnimationCurve pushCurve)
    {
        PrintDistance();

        float elapsed = 0f;
        while (elapsed < pushCurve.keys[^1].time)
        {
            float forceMultiplier = pushCurve.Evaluate(elapsed);
            Vector2 dir = (player.position - transform.position).normalized;
            dir.y = rb.velocity.y;

            dir = Vector2.right;

            rb.velocity = forceMultiplier * pushSpeed * -dir;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        StopCoroutine();
    }

    public void StopCoroutine()
    {

        if (pushCoroutine != null)
        {
            balanced = true;
            StopCoroutine(pushCoroutine);
            pushCoroutine = null;
            PrintDistance();
        }
    }

    private void PrintDistance()
    {
        if(positionStartKnockback == null)
        {
            positionStartKnockback = transform.position;
            return;
        }
        
        if(positionStartKnockback != null && positionEndKnockBack == null)
        {
            positionEndKnockBack = transform.position;
            var distance = Vector3.Distance((Vector3)positionStartKnockback, (Vector3)positionEndKnockBack);
            Debug.Log($"Distance in knockback: {distance}");
            positionEndKnockBack = null;
            positionStartKnockback = null;
        }
    }
}