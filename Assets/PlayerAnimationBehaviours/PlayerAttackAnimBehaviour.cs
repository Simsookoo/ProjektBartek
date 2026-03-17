using UnityEngine;

namespace Assets.PlayerAnimationBehaviours
{
    public class PlayerAttackAnimBehaviour : StateMachineBehaviour
    {
        public bool specialAttack = false;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var combat = animator.GetComponent<PlayerController>();
            combat.attackCollider.currentAttackData = this;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var combat = animator.GetComponent<PlayerController>();
            combat.attackCollider.currentAttackData = null;
        }
    }
}
