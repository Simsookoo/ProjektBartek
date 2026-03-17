using Assets.PlayerAnimationBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class PlayerAttackCollider : MonoBehaviour
    {
        [SerializeField] private Animator anim;

        public PlayerAttackAnimBehaviour currentAttackData;
        public int damage = 20;

        private readonly HashSet<int> _targetsHitInCurrentHitCycle = new();


        public HashSet<int> TargetsHitInCurrentHitCycle => _targetsHitInCurrentHitCycle;

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag != "Enemy") return;
            if (HitInCurrentCycle(collision)) return;

            if (collision.TryGetComponent(out EnemyAI enemyHealth))
            {
                enemyHealth.TakeDamage(damage, currentAttackData != null && currentAttackData.specialAttack ? 1 : null);
            }
        }

        private bool HitInCurrentCycle(Collider2D collider)
        {
            var hitTargetId = collider.GetInstanceID();

            if (_targetsHitInCurrentHitCycle.Contains(hitTargetId))
            {
                return true;
            }

            _targetsHitInCurrentHitCycle.Add(hitTargetId);

            return false;
        }

        public void OnDisable()
        {
            _targetsHitInCurrentHitCycle.Clear();
        }
    }
}
