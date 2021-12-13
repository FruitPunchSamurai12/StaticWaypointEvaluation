using UnityEngine;

public class PlayerAnimator:MonoBehaviour
{
    [SerializeField] Animator animator;


    public void MovementAnimation(Vector3 direction)
    {
        Vector3 moveDirection = transform.InverseTransformDirection(direction);
        animator.SetFloat("Horizontal", moveDirection.x);
        animator.SetFloat("Vertical", moveDirection.z);
    }

    public void DashAnimation(Vector3 direction)
    {
        Vector3 dashAnimationDirection = transform.InverseTransformDirection(direction);
        animator.SetFloat("RollH", dashAnimationDirection.x);
        animator.SetFloat("RollV", dashAnimationDirection.z);
        animator.SetTrigger("Roll");
    }
}