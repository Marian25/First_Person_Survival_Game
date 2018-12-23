using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Feeding1 : AIZombieState {

    [SerializeField] float slerpSpeed = 5f;

    private int eatingStateHash = Animator.StringToHash("Feeding State");
    private int eatingLayerIndex = -1;


    public override AIStateType GetStateType()
    {
        return AIStateType.Feeding;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        Debug.Log("Entering Feeding State");

        if (zombieStateMachine == null) return;

        if (eatingLayerIndex == -1)
        {
            eatingLayerIndex = zombieStateMachine.GetAnimator.GetLayerIndex("Cinematic Layer");
        }

        zombieStateMachine.feeding = true;
        zombieStateMachine.seeking = 0;
        zombieStateMachine.speed = 0;
        zombieStateMachine.attackType = 0;

        zombieStateMachine.NavAgentControl(true, false);
    }

    public override void OnExitState()
    {
        base.OnExitState();

        if (zombieStateMachine != null)
            zombieStateMachine.feeding = false;
    }

    public override AIStateType OnUpdate()
    {
        if (zombieStateMachine.satisfaction > 0.9f)
        {
            zombieStateMachine.GetWaypointPosition(false);
            return AIStateType.Alerted;
        }

        if (zombieStateMachine.visualThreat.GetType != AITargetType.None && zombieStateMachine.visualThreat.GetType != AITargetType.Visual_Food)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            return AIStateType.Alerted;
        }

        if (zombieStateMachine.audioThreat.GetType == AITargetType.Audio)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.audioThreat);
            return AIStateType.Alerted;
        }

        if (zombieStateMachine.GetAnimator.GetCurrentAnimatorStateInfo(eatingLayerIndex).shortNameHash == eatingStateHash)
        {
            zombieStateMachine.satisfaction = Mathf.Min(zombieStateMachine.satisfaction + Time.deltaTime * zombieStateMachine.replenishRate / 100, 1.0f);
        }

        if (!zombieStateMachine.useRootRotation)
        {
            Vector3 targetPos = zombieStateMachine.targetPosition;
            targetPos.y = zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - zombieStateMachine.transform.position);
            zombieStateMachine.transform.rotation = Quaternion.Slerp(zombieStateMachine.transform.rotation, newRot, Time.deltaTime * slerpSpeed);
        }

        return AIStateType.Feeding;
    }

}
