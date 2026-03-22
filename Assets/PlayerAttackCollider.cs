using Assets.PlayerAnimationBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class PlayerAttackCollider : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Collider2D collider;

        [SerializeField] private List<AudioClip> swingSounds;
        [SerializeField] private List<AudioClip> hitSounds;

        private int hitSoundClipIndex = 0;

        public PlayerAttackAnimBehaviour currentAttackData;
        public int damage = 20;

        private readonly HashSet<int> _targetsHitInCurrentHitCycle = new();


        public HashSet<int> TargetsHitInCurrentHitCycle => _targetsHitInCurrentHitCycle;


        private void OnEnable()
        {
            var results = new Collider2D[10];
            collider.OverlapCollider(new ContactFilter2D() { useTriggers = true }, results);

            foreach (var item in results)
            {
                if(item == null) continue;
                OnTriggerEnter2D(item);
            }
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out EnemyHurtbox hurtbox)) return;
            if (HitInCurrentCycle(collision)) return;

            hurtbox.ReceiveHit(damage, currentAttackData != null && currentAttackData.specialAttack ? 2 : null);
            PlayHitSound();
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

        public void PlaySwingSound()
        {
            if (audioSource == null) return;
            if (swingSounds == null || swingSounds.Count == 0) return;

            AudioClip clip = swingSounds[Random.Range(0, swingSounds.Count)];
            audioSource.PlayOneShot(clip);
        }

        private void PlayHitSound()
        {
            if (audioSource == null) return;
            if (hitSounds == null || hitSounds.Count == 0) return;

            AudioClip clip = hitSounds[hitSoundClipIndex % hitSounds.Count];
            hitSoundClipIndex++;

            audioSource.PlayOneShot(clip);
        }

        public void OnDisable()
        {
            _targetsHitInCurrentHitCycle.Clear();
        }
    }

}
