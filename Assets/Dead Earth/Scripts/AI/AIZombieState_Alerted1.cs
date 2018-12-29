using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Alerted1 : AIZombieState {

    [SerializeField][Range(1, 60)] float maxDuration = 10f;
    [SerializeField] float waypointAngleThreshold = 90f;
    [SerializeField] float threatAngleThreshold = 10f;
    [SerializeField] float directionChangeTime = 1.5f;
    [SerializeField] float slerpSpeed = 45.0f;

    private float timer = 0f;
    private float directionChangeTimer = 0f;
    private float screamChance = 0;
    private float nextScream = 0;
    private float screamFrequency = 120f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        Debug.Log("Entering Alerted State");

        if (zombieStateMachine == null) return;

        zombieStateMachine.NavAgentControl(true, false);
        zombieStateMachine.speed = 0;
        zombieStateMachine.seeking = 0;
        zombieStateMachine.feeding = false;
        zombieStateMachine.attackType = 0;

        timer = maxDuration;
        directionChangeTimer = 0;
        screamChance = zombieStateMachine.screamChance - Random.value;
    }

    public override AIStateType OnUpdate()
    {
        timer -= Time.deltaTime;
        directionChangeTimer += Time.deltaTime;

        if (timer <= 0)
        {
            zombieStateMachine.GetNavAgent.SetDestination(zombieStateMachine.GetWaypointPosition(false));
            zombieStateMachine.GetNavAgent.isStopped = false;
            timer = maxDuration;
        }

        if (zombieStateMachine.visualThreat.GetType == AITargetType.Visual_Player)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);

            if (screamChance > 0 && Time.time > nextScream)
            {
                if (zombieStateMachine.Scream())
                {
                    screamChance = float.MinValue;
                    nextScream = Time.time + screamFrequency;
                    return AIStateType.Alerted;
                }
            }

            return AIStateType.Pursuit;
        }

        if (zombieStateMachine.audioThreat.GetType == AITargetType.Audio)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.audioThreat);
            timer = maxDuration;
        }

        if (zombieStateMachine.visualThreat.GetType == AITargetType.Visual_Light)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            timer = maxDuration;
        }

        if (zombieStateMachine.audioThreat.GetType == AITargetType.None &&
            zombieStateMachine.visualThreat.GetType == AITargetType.Visual_Food &&
            zombieStateMachine.targetType == AITargetType.None)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }

        float angle;

        if ((zombieStateMachine.targetType == AITargetType.Audio || zombieStateMachine.targetType == AITargetType.Visual_Light) && !zombieStateMachine.isTargetReached)
        {
            angle = AIState.FindSignedAngle(zombieStateMachine.transform.forward,
                                            zombieStateMachine.targetPosition - zombieStateMachine.transform.position);

            if (zombieStateMachine.targetType == AITargetType.Audio && Mathf.Abs(angle) < threatAngleThreshold)
            {
                return AIStateType.Pursuit;
            }

            if (directionChangeTimer > directionChangeTime)
            {
                if (Random.value < zombieStateMachine.intelligence)
                {
                    zombieStateMachine.seeking = (int)Mathf.Sign(angle);
                }
                else
                {
                    zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1f, 1f));
                }

                directionChangeTimer = 0;
            }
        } else 
        if (zombieStateMachine.targetType == AITargetType.Waypoint && !zombieStateMachine.GetNavAgent.pathPending)
        {
            angle = AIState.FindSignedAngle (zombieStateMachine.transform.forward, 
                zombieStateMachine.GetNavAgent.steeringTarget - zombieStateMachine.transform.position);

            if (Mathf.Abs (angle) < waypointAngleThreshold)
                return AIStateType.Patrol;
            if (directionChangeTimer > directionChangeTime) 
            {
                zombieStateMachine.seeking = (int)Mathf.Sign (angle);
                directionChangeTimer = 0.0f;
            }
        }
        else 
        {
            if (directionChangeTimer > directionChangeTime) 
            {
                zombieStateMachine.seeking = (int)Mathf.Sign (Random.Range (-1.0f, 1.0f));
                directionChangeTimer = 0.0f;
            }
        }

        if (!zombieStateMachine.useRootRotation)
            zombieStateMachine.transform.Rotate (new Vector3(0.0f, slerpSpeed * zombieStateMachine.seeking * Time.deltaTime, 0.0f ));

        return AIStateType.Alerted;
    }


}
