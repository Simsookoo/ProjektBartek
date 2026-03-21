using Assets.PlayerAnimationBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class PlayerAttackCollider : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        [SerializeField] private AudioSource audioSource;

        [SerializeField] private List<AudioClip> attackSounds;
        private int attackSoundClipIndex = 0;

        public PlayerAttackAnimBehaviour currentAttackData;
        public int damage = 20;

        private readonly HashSet<int> _targetsHitInCurrentHitCycle = new();


        public HashSet<int> TargetsHitInCurrentHitCycle => _targetsHitInCurrentHitCycle;

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out EnemyHurtbox hurtbox)) return;
            if (HitInCurrentCycle(collision)) return;

            hurtbox.ReceiveHit(damage, currentAttackData != null && currentAttackData.specialAttack ? 2 : null);
            PlayAttackSound();
        }

        private bool HitInCurrentCycle(Collider2D collider)
        {
            EnemyHurtbox hurtbox = collider.GetComponent<EnemyHurtbox>();
            if (hurtbox == null) return true;

            EnemyAI enemy = hurtbox.GetComponentInParent<EnemyAI>();
            if (enemy == null) return true;

            int hitTargetId = enemy.gameObject.GetInstanceID();

            if (_targetsHitInCurrentHitCycle.Contains(hitTargetId))
            {
                return true;
            }

            _targetsHitInCurrentHitCycle.Add(hitTargetId);
            return false;
        }

        private void PlayAttackSound()
        {
            if (audioSource == null) return;

            AudioClip clip = attackSounds[attackSoundClipIndex++ % attackSounds.Count];

            audioSource.PlayOneShot(clip);
        }

        public void OnDisable()
        {
            _targetsHitInCurrentHitCycle.Clear();
        }
    }

}
