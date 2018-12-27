using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicEnabler : AIStateMachineLink {

    public bool onEnter = false;
    public bool onExit = false;


    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (stateMachine)
            stateMachine.cinematicEnabled = onEnter;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (stateMachine)
            stateMachine.cinematicEnabled = onExit;
    }
}
