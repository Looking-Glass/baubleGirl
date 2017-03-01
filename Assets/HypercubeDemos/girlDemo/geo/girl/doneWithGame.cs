using UnityEngine;

public class doneWithGame : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        girl g = GameObject.FindObjectOfType<girl>();
        g.doneCompleted();
    }

}
