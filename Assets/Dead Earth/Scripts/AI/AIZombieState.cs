﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIZombieState : AIState {

    protected int playerLayerMask = -1;
    protected int bodyPartLayer = -1;

    private void Awake()
    {
        playerLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Default"); // +1 else
        bodyPartLayer = LayerMask.GetMask("AI Body Part");
    }

    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
    {
        base.OnTriggerEvent(eventType, other);

        if (stateMachine == null) return;

        if (eventType != AITriggerEventType.Exit)
        {
            AITargetType curType = stateMachine.visualThreat.GetType;

            if (other.CompareTag("Player"))
            {
                float distance = Vector3.Distance(stateMachine.sensorPosition, other.transform.position);

                if (curType != AITargetType.Visual_Player ||
                    (curType == AITargetType.Visual_Player && distance < stateMachine.visualThreat.GetDistance))
                {
                    RaycastHit hitInfo;

                    if (ColliderIsVisible(other, out hitInfo, playerLayerMask))
                    {
                        stateMachine.visualThreat.Set(AITargetType.Visual_Player, other, other.transform.position, distance);
                    }
                }
            }
        }
    }

    protected virtual bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask = -1)
    {
        hitInfo = new RaycastHit();

        if (stateMachine == null || stateMachine.GetType() != typeof(AIZombieStateMachine)) return false;
        AIZombieStateMachine zombieStateMachine = (AIZombieStateMachine) stateMachine;

        Vector3 head = stateMachine.sensorPosition;
        Vector3 direction = other.transform.position - head;
        float angle = Vector3.Angle(direction, transform.forward);

        if (angle > zombieStateMachine.fov * 0.5f)
        {
            return false;
        }

        RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized, stateMachine.radius * zombieStateMachine.sight, layerMask);

        float closestColliderDistance = float.MaxValue;
        Collider closestCollider = null;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (hit.distance < closestColliderDistance)
            {
                if (hit.transform.gameObject.layer == bodyPartLayer)
                {
                    if (stateMachine != GameSceneManager.GetInstance().GetAIStateMachine(hit.rigidbody.GetInstanceID()))
                    {
                        closestColliderDistance = hit.distance;
                        closestCollider = hit.collider;
                        hitInfo = hit;
                    }
                }
                else
                {
                    closestColliderDistance = hit.distance;
                    closestCollider = hit.collider;
                    hitInfo = hit;
                }
            }
        }

        if (closestCollider != null && closestCollider.gameObject == other.gameObject) return true;

        return false;
    }

}
