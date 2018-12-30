using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerEnabler : AIStateMachineLink
{
    public bool OnEnter = false;
    public bool OnExit = false;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (stateMachine)
            stateMachine.SetLayerActive(animator.GetLayerName(layerIndex), OnEnter);
    }
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (stateMachine)
            stateMachine.SetLayerActive(animator.GetLayerName(layerIndex), OnExit);
    }

}

