using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour {

    public abstract AIStateType OnUpdate();
    public abstract AIStateType GetStateType();

    public virtual void SetStateMachine(AIStateMachine stateMachine)
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

    public static void ConvertSphereColliderToWorldSpace(SphereCollider col, out Vector3 pos, out float radius)
    {
        pos = Vector3.zero;
        radius = 0.0f;

        if (col == null) return;

        pos = col.transform.position;
        pos.x = col.center.x * col.transform.lossyScale.x;
        pos.y = col.center.y * col.transform.lossyScale.y;
        pos.z = col.center.z * col.transform.lossyScale.z;

        radius = Mathf.Max(col.radius * col.transform.lossyScale.x,
                           col.radius * col.transform.lossyScale.y);
        radius = Mathf.Max(radius, col.radius * col.transform.lossyScale.z);
    }

}
