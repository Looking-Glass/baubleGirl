using UnityEngine;
using System.Collections;

public class resetTrigger : StateMachineBehaviour
{

    public string triggerName;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(triggerName);
    }
}
