using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Idle1 : AIZombieState {

    [SerializeField] Vector2 idleTimeRange = new Vector2(10.0f, 60.0f);

    private float idleTime = 0.0f;
    private float timer = 0.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Idle;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        Debug.Log("Entering idle state");

        if (zombieStateMachine == null) return;

        idleTime = Random.Range(idleTimeRange.x, idleTimeRange.y);
        timer = 0.0f;

        zombieStateMachine.NavAgentControl(true, false);
        zombieStateMachine.speed = 0;
        zombieStateMachine.seeking = 0;
        zombieStateMachine.feeding = false;
        zombieStateMachine.attackType = 0;
        zombieStateMachine.ClearTarget();
    }

    public override AIStateType OnUpdate()
    {
        if (zombieStateMachine == null) return AIStateType.Idle;

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
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }

        timer += Time.deltaTime;
        if (timer > idleTime)
        {
            Debug.Log("Going to Patrol");
            return AIStateType.Patrol;
        }


        return AIStateType.Idle;
    }

}
