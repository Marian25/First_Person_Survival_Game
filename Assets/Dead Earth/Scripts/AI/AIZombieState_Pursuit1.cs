using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Pursuit1 : AIZombieState {

    [SerializeField] [Range(0, 10)] private float speed = 1.0f;
    [SerializeField] [Range(0, 1)] float lookAtWeight = 0.7f;
    [SerializeField] [Range(0, 90)] float lookAtAngleThreshold = 15f;
    [SerializeField] private float slerpSpeed = 5.0f;
    [SerializeField] private float repathDistanceMultiplier = 0.035f;
    [SerializeField] private float repathVisualMinDuration = 0.05f;
    [SerializeField] private float repathVisualMaxDuration = 5.0f;
    [SerializeField] private float repathAudioMinDuration = 0.25f;
    [SerializeField] private float repathAudioMaxDuration = 5.0f;
    [SerializeField] private float maxDuration = 40f;

    private float timer = 0.0f;
    private float repathTimer = 0.0f;
    private float currentLookAtWeight = 0;

    public override AIStateType GetStateType()
    {
        return AIStateType.Pursuit;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        Debug.Log("Entering Pursuit State");

        if (zombieStateMachine == null) return;

        zombieStateMachine.NavAgentControl(true, false);
        zombieStateMachine.seeking = 0;
        zombieStateMachine.feeding = false;
        zombieStateMachine.attackType = 0;

        timer = 0;
        repathTimer = 0;

        zombieStateMachine.GetNavAgent.SetDestination(zombieStateMachine.targetPosition);
        zombieStateMachine.GetNavAgent.isStopped = false;

        currentLookAtWeight = 0;
    }

    public override AIStateType OnUpdate()
    {
        timer += Time.deltaTime;
        repathTimer += Time.deltaTime;

        if (timer > maxDuration) return AIStateType.Patrol;

        if (zombieStateMachine.targetType == AITargetType.Visual_Player && zombieStateMachine.inMeleeRange)
        {
            return AIStateType.Attack;
        }

        if (zombieStateMachine.isTargetReached)
        {
            switch (zombieStateMachine.targetType)
            {
                case AITargetType.Audio:
                case AITargetType.Visual_Light:
                    stateMachine.ClearTarget();
                    return AIStateType.Alerted;

                case AITargetType.Visual_Food:
                    return AIStateType.Feeding;
            }
        }

        if (zombieStateMachine.GetNavAgent.isPathStale || 
            (!zombieStateMachine.GetNavAgent.hasPath && !zombieStateMachine.GetNavAgent.pathPending) ||
            zombieStateMachine.GetNavAgent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete) 
        {
            return AIStateType.Alerted;
        }

        if (zombieStateMachine.GetNavAgent.pathPending)
            zombieStateMachine.speed = 0;
        else 
        {
            zombieStateMachine.speed = speed;

            if (!zombieStateMachine.useRootRotation &&
                zombieStateMachine.targetType == AITargetType.Visual_Player && 
                zombieStateMachine.visualThreat.GetType == AITargetType.Visual_Player && 
                zombieStateMachine.isTargetReached)
            {
                Vector3 targetPos = zombieStateMachine.targetPosition;
                targetPos.y = zombieStateMachine.transform.position.y;
                Quaternion newRot = Quaternion.LookRotation(targetPos - zombieStateMachine.transform.position);
                zombieStateMachine.transform.rotation = newRot;
            } else if (!zombieStateMachine.useRootRotation && !zombieStateMachine.isTargetReached) {
                Quaternion newRot = Quaternion.LookRotation(zombieStateMachine.GetNavAgent.desiredVelocity);

                zombieStateMachine.transform.rotation = Quaternion.Slerp(zombieStateMachine.transform.rotation, newRot, Time.deltaTime * slerpSpeed);
            } else if (zombieStateMachine.isTargetReached)
            {
                return AIStateType.Alerted;
            }
        }

        if (zombieStateMachine.visualThreat.GetType == AITargetType.Visual_Player)
        {
            if (zombieStateMachine.targetPosition != zombieStateMachine.visualThreat.GetPosition)
            {
                if (Mathf.Clamp(zombieStateMachine.visualThreat.distance * repathDistanceMultiplier, repathVisualMinDuration, repathVisualMaxDuration) < repathTimer)
                {
                    zombieStateMachine.GetNavAgent.SetDestination(zombieStateMachine.visualThreat.GetPosition);
                    repathTimer = 0;
                }
            }

            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);

            return AIStateType.Pursuit;
        }

        if (zombieStateMachine.targetType == AITargetType.Visual_Player)
        {
            return AIStateType.Pursuit;
        }

        if (zombieStateMachine.visualThreat.GetType == AITargetType.Visual_Light)
        {
            if (zombieStateMachine.targetType == AITargetType.Audio || zombieStateMachine.targetType == AITargetType.Visual_Food)
            {
                zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
                return AIStateType.Alerted;
            } else if (zombieStateMachine.targetType == AITargetType.Visual_Light)
            {
                int currentID = zombieStateMachine.targetColliderID;

                if (currentID == zombieStateMachine.visualThreat.GetCollider.GetInstanceID())
                {
                    if (zombieStateMachine.targetPosition != zombieStateMachine.visualThreat.GetPosition)
                    {
                        if (Mathf.Clamp(zombieStateMachine.visualThreat.distance * repathDistanceMultiplier, repathVisualMinDuration, repathVisualMaxDuration) < repathTimer)
                        {
                            zombieStateMachine.GetNavAgent.SetDestination(zombieStateMachine.visualThreat.GetPosition);
                            repathTimer = 0;
                        }
                    }

                    zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
                    return AIStateType.Pursuit;
                } else
                {
                    zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
                    return AIStateType.Alerted;
                }
            }
        } else if (zombieStateMachine.audioThreat.GetType == AITargetType.Audio)
        {
            if (zombieStateMachine.targetType == AITargetType.Visual_Food)
            {
                zombieStateMachine.SetTarget(zombieStateMachine.audioThreat);
                return AIStateType.Alerted;
            } else if (zombieStateMachine.targetType == AITargetType.Audio)
            {
                int currentID = zombieStateMachine.targetColliderID;

                if (currentID == zombieStateMachine.audioThreat.GetCollider.GetInstanceID())
                {
                    if (zombieStateMachine.targetPosition != zombieStateMachine.audioThreat.GetPosition)
                    {
                        if (Mathf.Clamp(zombieStateMachine.audioThreat.distance * repathDistanceMultiplier, repathAudioMinDuration, repathAudioMaxDuration) < repathTimer)
                        {
                            zombieStateMachine.GetNavAgent.SetDestination(zombieStateMachine.audioThreat.GetPosition);
                            repathTimer = 0;
                        }
                    }

                    zombieStateMachine.SetTarget(zombieStateMachine.audioThreat);
                    return AIStateType.Pursuit;
                } else
                {
                    zombieStateMachine.SetTarget(zombieStateMachine.audioThreat);
                    return AIStateType.Alerted;
                }
            }
        }

        return AIStateType.Pursuit;
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
        }
        else
        {
            currentLookAtWeight = Mathf.Lerp(currentLookAtWeight, 0, Time.deltaTime);
            zombieStateMachine.GetAnimator.SetLookAtWeight(currentLookAtWeight);
        }
    }

}
