using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour {

    public abstract AIStateType OnUpdate();
    public abstract AIStateType GetStateType();

    public void SetStateMachine(AIStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }

    public virtual void OnAnimatorUpdated() {
        if (stateMachine.useRootPosition)
        {
            stateMachine.GetNavAgent.velocity = stateMachine.GetAnimator.deltaPosition / Time.deltaTime;
        }

        if (stateMachine.useRootRotation)
        {
            stateMachine.transform.rotation = stateMachine.GetAnimator.rootRotation;
        }
    }

    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }
    public virtual void OnDestinationReached(bool isReached) { }

    protected AIStateMachine stateMachine;

}
