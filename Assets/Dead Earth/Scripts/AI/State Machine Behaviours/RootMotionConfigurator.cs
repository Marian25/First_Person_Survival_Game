using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionConfigurator : AIStateMachineLink {

    [SerializeField] private int rootPosition = 0;
    [SerializeField] private int rootRotation = 0;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (stateMachine != null)
        {
            stateMachine.AddRootMotionRequest(rootPosition, rootRotation);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (stateMachine != null)
        {
            stateMachine.AddRootMotionRequest(-rootPosition, -rootRotation);
        }
    }

}
