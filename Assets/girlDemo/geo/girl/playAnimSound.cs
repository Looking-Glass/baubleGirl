using UnityEngine;
using System.Collections;


//a simple StateMachineBehaviour that allows a state to play a sound on start or end
//multiple provided sounds count as variants

public class playAnimSound : StateMachineBehaviour
{
    girl g;
    public AudioClip[] onEnterSound;
    public AudioClip[] onExitSound;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!g)
            g = GameObject.FindObjectOfType<girl>();

        if (onEnterSound.Length > 0)
            g.playVoiceSound(onEnterSound[Random.Range(0, onEnterSound.Length)]);
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (onExitSound.Length > 0)
            g.playVoiceSound(onExitSound[Random.Range(0, onExitSound.Length)]);
    }



}
