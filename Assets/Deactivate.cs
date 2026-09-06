using UnityEngine;

public class Deactivate : StateMachineBehaviour
{
    void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }
}
