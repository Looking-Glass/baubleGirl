using UnityEngine;

public class idleChooser : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetInteger("idleChoice", Random.Range(0, 4));
    }
}