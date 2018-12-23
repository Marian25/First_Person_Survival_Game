using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Patrol1 : AIZombieState {

    [SerializeField] [Range(0.0f, 3.0f)] float speed = 1.0f;
    [SerializeField] float turnOnSpotThreshold = 80.0f;
    [SerializeField] float slerpSpeed = 5.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        Debug.Log("Entering Patrol State");

        if (zombieStateMachine == null) return;

        zombieStateMachine.NavAgentControl(true, false);
        zombieStateMachine.speed = speed;
        zombieStateMachine.seeking = 0;
        zombieStateMachine.feeding = false;
        zombieStateMachine.attackType = 0;

        zombieStateMachine.GetNavAgent.SetDestination(zombieStateMachine.GetWaypointPosition(false));

        zombieStateMachine.GetNavAgent.isStopped = false;

    }

    public override AIStateType OnUpdate()
    {
        if (zombieStateMachine.visualThreat.GetType == AITargetType.Visual_Player)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }

        if (zombieStateMachine.visualThreat.GetType == AITargetType.Visual_Light)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            return AIStateType.Alerted;
        }

        if (zombieStateMachine.audioThreat.GetType == AITargetType.Audio)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.audioThreat);
            return AIStateType.Alerted;
        }

        if (zombieStateMachine.visualThreat.GetType == AITargetType.Visual_Food)
        {
            if ((1.0f - zombieStateMachine.satisfaction) > (zombieStateMachine.visualThreat.GetDistance / zombieStateMachine.sensorRadius))
            {
                zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
                return AIStateType.Pursuit;
            }
        }

        float angle = Vector3.Angle(zombieStateMachine.transform.forward, (zombieStateMachine.GetNavAgent.steeringTarget - zombieStateMachine.transform.position));

        if (angle > turnOnSpotThreshold)
        {
            return AIStateType.Alerted;
        }

        if (!zombieStateMachine.useRootRotation)
        {
            Quaternion newRot = Quaternion.LookRotation(zombieStateMachine.GetNavAgent.desiredVelocity);
            zombieStateMachine.transform.rotation = Quaternion.Slerp(zombieStateMachine.transform.rotation, newRot, slerpSpeed * Time.deltaTime);
        }

        if (zombieStateMachine.GetNavAgent.isPathStale ||
            !zombieStateMachine.GetNavAgent.hasPath ||
            zombieStateMachine.GetNavAgent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            zombieStateMachine.GetWaypointPosition(true);
        }



        return AIStateType.Patrol;
    }

    

    public override void OnDestinationReached(bool isReached)
    {
        base.OnDestinationReached(isReached);

        if (zombieStateMachine == null || !isReached) return;

        if (zombieStateMachine.targetType == AITargetType.Waypoint)
        {
            zombieStateMachine.GetNavAgent.SetDestination(zombieStateMachine.GetWaypointPosition(true));
        }

    }

    //public override void OnAnimatorIKUpdated()
    //{
    //    base.OnAnimatorIKUpdated();

    //    if (zombieStateMachine == null) return;

    //    zombieStateMachine.GetAnimator.SetLookAtPosition(zombieStateMachine.targetPosition + Vector3.up);
    //    zombieStateMachine.GetAnimator.SetLookAtWeight(0.55f);
    //}

}
