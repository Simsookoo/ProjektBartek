using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private Collider2D hitboxCollider;

    private bool hasHitPlayer = false;
    private bool wasParried = false;

    public void ResetHitState()
    {
        hasHitPlayer = false;
        wasParried = false;

        if (hitboxCollider != null)
        {
            var results = new Collider2D[10];
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;

            int count = hitboxCollider.OverlapCollider(filter, results);

            for (int i = 0; i < count; i++)
            {
                if (results[i] == null) continue;
                TryHit(results[i]);
            }
        }
    }

    public void OnParried()
    {
        if (wasParried) return;
        if (enemyAI == null) return;

        wasParried = true;
        hasHitPlayer = true;

        if (hitboxCollider != null)
            hitboxCollider.enabled = false;

        enemyAI.OnParried();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryHit(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryHit(collision);
    }

    private void TryHit(Collider2D collision)
    {
        if (hasHitPlayer) return;
        if (wasParried) return;
        if (enemyAI == null) return;

        PlayerHitReceiver playerHitReceiver = collision.GetComponent<PlayerHitReceiver>();
        if (playerHitReceiver == null)
            playerHitReceiver = collision.GetComponentInParent<PlayerHitReceiver>();

        if (playerHitReceiver == null) return;

        hasHitPlayer = true;

        Debug.Log($"ENEMY-ATTACK-PLAYER: Receive Hit");

        playerHitReceiver.ReceiveHit(enemyAI.transform);
        enemyAI.PlayHitSound();
    }
}