using UnityEngine;

public class EnemyHurtbox : MonoBehaviour
{
    [SerializeField] private EnemyAI enemy;

    public void ReceiveHit(int damage, int? overrideAnimationIndex = null)
    {
        if (enemy == null) return;
        enemy.TakeDamage(damage, overrideAnimationIndex);
    }
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = Color.green;

        if (col is BoxCollider2D box)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
            Gizmos.matrix = oldMatrix;
        }
    }
}