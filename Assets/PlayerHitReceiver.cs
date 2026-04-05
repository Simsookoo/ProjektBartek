using UnityEngine;

public class PlayerHitReceiver : MonoBehaviour
{
    [Header("Main switches")]
    [SerializeField] private bool canBeHit = true;
    [SerializeField] private bool playHitAnimation = true;
    [SerializeField] private bool lockMovementOnHit = true;
    [SerializeField] private bool useKnockback = true;
    [SerializeField] private bool faceAttackerOnHit = true;

    [Header("Hit settings")]
    [SerializeField] private string hitTriggerName = "Hit";
    [SerializeField] private string canMoveAfterHitBoolName = "CanMoveAfterHit";

    [Header("Knockback settings")]
    [SerializeField] private float knockbackForceX = 8f;
    [SerializeField] private float knockbackForceY = 2.5f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Rigidbody2D rb;

    private bool isHitStunned = false;

    public bool IsHitStunned => isHitStunned;

    public void ReceiveHit(Transform attacker = null)
    {
        Debug.Log($"ENEMY-ATTACK-PLAYER: canBeHit: {canBeHit}");
        Debug.Log($"ENEMY-ATTACK-PLAYER: isHitStunned: {isHitStunned}");
        Debug.Log($"ENEMY-ATTACK-PLAYER: Receive Hit");

        if (!canBeHit) return;
        if (isHitStunned) return;

        if (playerController != null && playerController.IsDeflecting())
            return;

        isHitStunned = true;

        if (animator != null)
        {
            animator.SetBool(canMoveAfterHitBoolName, false);
        }

        if (faceAttackerOnHit && attacker != null)
        {
            FaceAttacker(attacker);
        }

        if (lockMovementOnHit && playerController != null)
        {
            playerController.SetMovementLocked(true);
        }

        if (playHitAnimation && animator != null)
        {
            animator.ResetTrigger(hitTriggerName);
            animator.SetTrigger(hitTriggerName);
        }

        if (useKnockback && rb != null && attacker != null)
        {
            float direction = transform.position.x >= attacker.position.x ? 1f : -1f;

            rb.velocity = Vector2.zero;
            rb.AddForce(new Vector2(direction * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

            if (playerController != null)
            {
                playerController.StartKnockbackLock();
            }
        }
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.1f, 0.15f);
        }
    }

    public void AllowMovementAgain()
    {
        if (!isHitStunned) return;

        isHitStunned = false;

        if (lockMovementOnHit && playerController != null)
        {
            playerController.SetMovementLocked(false);
        }

        if (animator != null)
        {
            animator.SetBool(canMoveAfterHitBoolName, true);
        }
    }

    private void FaceAttacker(Transform attacker)
    {
        float direction = attacker.position.x > transform.position.x ? -1f : 1f;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }
}