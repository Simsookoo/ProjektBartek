using UnityEngine;

public class PlayerParryHitbox : MonoBehaviour
{
    public bool isActive = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        EnemyAttackHitbox enemyHitbox = collision.GetComponent<EnemyAttackHitbox>();
        if (enemyHitbox == null) return;

        enemyHitbox.OnParried();
    }
}
