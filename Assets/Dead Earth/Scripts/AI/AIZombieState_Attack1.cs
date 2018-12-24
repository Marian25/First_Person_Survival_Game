using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Attack1 : AIZombieState {

    [SerializeField] [Range(0, 10)] float speed = 0;
    [SerializeField] float stoppingDistance = 1.0f;
    [SerializeField] [Range(0, 1)] float lookAtWeight = 0.7f;
    [SerializeField] [Range(0, 90)] float lookAtAngleThreshold = 15f;
    [SerializeField] float slerpSpeed = 5f;

    private float currentLookAtWeight = 0;

    public override AIStateType GetStateType()
    {
        return AIStateType.Attack;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        Debug.Log("Entering Attack State");

        if (zombieStateMachine) return;

        zombieStateMachine.NavAgentControl(true, false);
        zombieStateMachine.seeking = 0;
        zombieStateMachine.feeding = false;
        zombieStateMachine.attackType = Random.Range(1, 100);
        zombieStateMachine.speed = speed;
        currentLookAtWeight = 0;
    }

    public override void OnExitState()
    {
        base.OnExitState();
        zombieStateMachine.attackType = 0;
    }

    public override AIStateType OnUpdate()
    {
        Vector3 targetPos;
        Quaternion newRot;

        if (Vector3.Distance(zombieStateMachine.transform.position, zombieStateMachine.targetPosition) < stoppingDistance)
        {
            zombieStateMachine.speed = 0;
        } else
        {
            zombieStateMachine.speed = speed;
        }

        if (zombieStateMachine.visualThreat.GetType == AITargetType.Visual_Player)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);

            if (!zombieStateMachine.inMeleeRange) return AIStateType.Pursuit;

            if (!zombieStateMachine.useRootRotation)
            {
                targetPos = zombieStateMachine.targetPosition;
                targetPos.y = zombieStateMachine.transform.position.y;
                newRot = Quaternion.LookRotation(targetPos - zombieStateMachine.transform.position);
                zombieStateMachine.transform.rotation = Quaternion.Slerp(zombieStateMachine.transform.rotation, newRot, slerpSpeed * Time.deltaTime);
            }

            zombieStateMachine.attackType = Random.Range(1, 100);
            return AIStateType.Attack;
        }

        if (!zombieStateMachine.useRootRotation)
        {
            targetPos = zombieStateMachine.targetPosition;
            targetPos.y = zombieStateMachine.transform.position.y;
            newRot = Quaternion.LookRotation(targetPos - zombieStateMachine.transform.position);
            zombieStateMachine.transform.rotation = Quaternion.Slerp(zombieStateMachine.transform.rotation, newRot, slerpSpeed * Time.deltaTime);
        }

        return AIStateType.Alerted;
    }

    public override void OnAnimatorIKUpdated()
    {
        base.OnAnimatorIKUpdated();
        if (zombieStateMachine) return;

        if (Vector3.Angle(zombieStateMachine.transform.forward, zombieStateMachine.targetPosition - zombieStateMachine.transform.position) < lookAtAngleThreshold)
        {
            zombieStateMachine.GetAnimator.SetLookAtPosition(zombieStateMachine.targetPosition + Vector3.up);
            currentLookAtWeight = Mathf.Lerp(currentLookAtWeight, lookAtWeight, Time.deltaTime);
            zombieStateMachine.GetAnimator.SetLookAtWeight(currentLookAtWeight);
        } else
        {
            currentLookAtWeight = Mathf.Lerp(currentLookAtWeight, 0, Time.deltaTime);
            zombieStateMachine.GetAnimator.SetLookAtWeight(currentLookAtWeight);
        }
    }

}
