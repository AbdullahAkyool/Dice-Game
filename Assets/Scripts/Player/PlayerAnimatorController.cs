using UnityEngine;

namespace DiceGame.Player
{
    public class PlayerAnimatorController : MonoBehaviour
    {
        [SerializeField] private Animator playerAnimator;
        
        public void IdleAnimation()
        {
            playerAnimator.SetBool("IsRunning", false);
        }

        public void RunAnimation()
        {
            playerAnimator.SetBool("IsRunning", true);            
        }
    }
}
