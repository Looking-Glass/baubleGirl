﻿using UnityEngine;
using System.Collections;

public class resetOnExit : StateMachineBehaviour
{


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        girl g = GameObject.FindObjectOfType<girl>();
        g.reset();
    }

}
