using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateType { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead }
public enum AITargetType { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio }
public enum AITriggerEventType { Enter, Stay, Exit }

public struct AITarget
{
    private AITargetType type;
    private Collider collider;
    private Vector3 position;
    private float distance;
    private float time;

    public AITargetType GetType { get { return type; } }
    public Collider GetCollider { get { return collider; } }
    public Vector3 GetPosition { get { return position; } }
    public float GetDistance { get { return distance; } }
    public float SetDistance { set { distance = value; } }
    public float GetTime { get { return time; } }

    public void Set(AITargetType t, Collider c, Vector3 p, float d)
    {
        type = t;
        collider = c;
        position = p;
        distance = d;
        time = Time.time;
    }

    public void Clear()
    {
        type = AITargetType.None;
        collider = null;
        position = Vector3.zero;
        distance = Mathf.Infinity;
        time = 0;
    }
}

public abstract class AIStateMachine : MonoBehaviour {

    public AITarget visualThreat = new AITarget();
    public AITarget audioThreat = new AITarget();

    protected AIState currentState = null;
    protected Dictionary<AIStateType, AIState> dictStates = new Dictionary<AIStateType, AIState>();
    protected AITarget target = new AITarget();

    [SerializeField] protected AIStateType currentStateType = AIStateType.Idle;
    [SerializeField] protected SphereCollider targetTrigger = null;
    [SerializeField] protected SphereCollider sensorTrigger = null;
    [SerializeField] [Range(0, 15)] protected float stoppingDistance = 1.0f;

    protected Animator animator = null;
    protected NavMeshAgent navAgent = null;
    protected Collider collider = null;

    public Animator GetAnimator { get { return animator; } }
    public NavMeshAgent GetNavAgent { get { return navAgent; } }

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        collider = GetComponent<Collider>();
    }

    protected virtual void Start()
    {
        AIState[] states = GetComponents<AIState>();
        foreach(AIState state in states)
        {
            if (state != null && !dictStates.ContainsKey(state.GetStateType()))
            {
                dictStates[state.GetStateType()] = state;
                state.SetStateMachine(this);
            }
        }

        if (dictStates.ContainsKey(currentStateType))
        {
            currentState = dictStates[currentStateType];
            currentState.OnEnterState();
        } else
        {
            currentState = null;
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        target.Set(t, c, p, d);

        if (targetTrigger != null)
        {
            targetTrigger.radius = stoppingDistance;
            targetTrigger.transform.position = target.GetPosition;
            targetTrigger.enabled = true;
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s)
    {
        target.Set(t, c, p, d);

        if (targetTrigger != null)
        {
            targetTrigger.radius = s;
            targetTrigger.transform.position = target.GetPosition;
            targetTrigger.enabled = true;
        }
    }

    public void SetTarget(AITarget t)
    {
        target = t;

        if (targetTrigger != null)
        {
            targetTrigger.radius = stoppingDistance;
            targetTrigger.transform.position = target.GetPosition;
            targetTrigger.enabled = true;
        }
    }

    public void ClearTarget()
    {
        target.Clear();
        if (targetTrigger != null)
        {
            targetTrigger.enabled = false;
        }
    }

    protected virtual void FixedUpdate()
    {
        visualThreat.Clear();
        audioThreat.Clear();

        if (target.GetType != AITargetType.None)
        {
            target.SetDistance = Vector3.Distance(transform.position, target.GetPosition);
        }
    }

    protected virtual void Update()
    {
        if (currentState == null) return;

        AIStateType newStateType = currentState.OnUpdate();

        if (newStateType != currentStateType)
        {
            AIState newState = null;
            if (dictStates.TryGetValue(newStateType, out newState))
            {
                currentState.OnExitState();
                newState.OnEnterState();
                currentState = newState;
            }
            else if (dictStates.TryGetValue(AIStateType.Idle, out newState))
            {
                currentState.OnExitState();
                newState.OnEnterState();
                currentState = newState;
            }

            currentStateType = newStateType;
        }
    }
}
