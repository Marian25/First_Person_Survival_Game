using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIZombieState : AIState {

    protected int playerLayerMask = -1;
    protected int bodyPartLayer = -1;
    protected int visualLayerMask = -1;
    protected AIZombieStateMachine zombieStateMachine = null;

    private void Awake()
    {
        playerLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Default"); // +1 else
        visualLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Default", "Visual Aggravator");
        bodyPartLayer = LayerMask.NameToLayer("AI Body Part");
    }

    public override void SetStateMachine(AIStateMachine machine)
    {
        if (machine.GetType() == typeof(AIZombieStateMachine))
        {
            base.SetStateMachine(machine);
            zombieStateMachine = (AIZombieStateMachine)machine;
        }
    }

    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
    {
        base.OnTriggerEvent(eventType, other);

        if (zombieStateMachine == null) return;

        if (eventType != AITriggerEventType.Exit)
        {
            AITargetType curType = zombieStateMachine.visualThreat.GetType;

            if (other.CompareTag("Player"))
            {
                float distance = Vector3.Distance(zombieStateMachine.sensorPosition, other.transform.position);

                if (curType != AITargetType.Visual_Player ||
                    (curType == AITargetType.Visual_Player && distance < zombieStateMachine.visualThreat.GetDistance))
                {
                    RaycastHit hitInfo;

                    if (ColliderIsVisible(other, out hitInfo, playerLayerMask))
                    {
                        zombieStateMachine.visualThreat.Set(AITargetType.Visual_Player, other, other.transform.position, distance);
                    }
                }
            } else if (other.CompareTag("Flashlight") && curType != AITargetType.Visual_Player)
            {
                BoxCollider flashLightTrigger = (BoxCollider)other;
                float distanceToThreat = Vector3.Distance(zombieStateMachine.sensorPosition, flashLightTrigger.transform.position);
                float zSize = flashLightTrigger.size.z * flashLightTrigger.transform.lossyScale.z;
                float aggrFactor = distanceToThreat / zSize;

                if (aggrFactor <= zombieStateMachine.sight && aggrFactor <= zombieStateMachine.intelligence)
                {
                    zombieStateMachine.visualThreat.Set(AITargetType.Visual_Light, other, other.transform.position, distanceToThreat);
                }

            } else if (other.CompareTag("AI Sound Emitter"))
            {
                SphereCollider soundTrigger = (SphereCollider)other;
                if (soundTrigger == null) return;

                Vector3 agentSensorPosition = zombieStateMachine.sensorPosition;

                Vector3 soundPos;
                float soundRadius;
                AIState.ConvertSphereColliderToWorldSpace(soundTrigger, out soundPos, out soundRadius);

                float distanceToThreat = (soundPos - agentSensorPosition).magnitude;
                float distanceFactor = distanceToThreat / soundRadius;

                distanceFactor += distanceFactor * (1.0f - zombieStateMachine.hearing);

                // too far away 
                if (distanceFactor > 1.0f) return;

                if (distanceToThreat < zombieStateMachine.audioThreat.GetDistance)
                {
                    zombieStateMachine.audioThreat.Set(AITargetType.Audio, other, soundPos, distanceToThreat);
                }

            } else if ( other.CompareTag("AI Food") && 
                        curType != AITargetType.Visual_Player && 
                        curType != AITargetType.Visual_Light && 
                        zombieStateMachine.satisfaction <= 0.9f &&
                        zombieStateMachine.audioThreat.GetType == AITargetType.None)
            {
                float distanceToThreat = Vector3.Distance(other.transform.position, zombieStateMachine.sensorPosition);

                if (distanceToThreat < zombieStateMachine.visualThreat.GetDistance)
                {
                    RaycastHit hitInfo;
                    if (ColliderIsVisible(other, out hitInfo, visualLayerMask))
                    {
                        zombieStateMachine.visualThreat.Set(AITargetType.Visual_Food, other, other.transform.position, distanceToThreat);
                    }
                }

            }
        }
    }

    protected virtual bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask = -1)
    {
        hitInfo = new RaycastHit();

        if (zombieStateMachine == null) return false;

        Vector3 head = zombieStateMachine.sensorPosition;
        Vector3 direction = other.transform.position - head;
        float angle = Vector3.Angle(direction, transform.forward);

        if (angle > zombieStateMachine.fov * 0.5f)
        {
            return false;
        }

        RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized, zombieStateMachine.sensorRadius * zombieStateMachine.sight, layerMask);

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
