using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionConfigurator : AIStateMachineLink {

    [SerializeField] private int rootPosition = 0;
    [SerializeField] private int rootRotation = 0;

    private bool rootMotionProcessed = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (stateMachine != null && !rootMotionProcessed)
        {
            stateMachine.AddRootMotionRequest(rootPosition, rootRotation);
            rootMotionProcessed = true;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (stateMachine != null && rootMotionProcessed)
        {
            stateMachine.AddRootMotionRequest(-rootPosition, -rootRotation);
            rootMotionProcessed = false;
        }
    }

}
