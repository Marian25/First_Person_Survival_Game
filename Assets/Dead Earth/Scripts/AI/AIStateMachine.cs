using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateType { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead }
public enum AITargetType { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio }
public enum AITriggerEventType { Enter, Stay, Exit }
public enum AIBoneAlignmentType { XAxis, YAxis, ZAxis, XAxisInverted, YAxisInverted, ZAxisInverted }

public struct AITarget
{
    private AITargetType type;
    private Collider collider;
    private Vector3 position;
    public float distance;
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
    protected int rootPositionRefCount = 0;
    protected int rootRotationRefCount = 0;
    protected bool _isTargetReached = false;
    protected List<Rigidbody> bodyParts = new List<Rigidbody>();
    protected int aiBodyPartLayer = -1;
    protected bool _cinematicEnabled = false;

    [SerializeField] protected AIStateType currentStateType = AIStateType.Idle;
    [SerializeField] protected Transform rootBone = null;
    [SerializeField] protected AIBoneAlignmentType rootBoneAlignment = AIBoneAlignmentType.ZAxis;
    [SerializeField] protected SphereCollider targetTrigger = null;
    [SerializeField] protected SphereCollider sensorTrigger = null;
    [SerializeField] protected AIWaypointNetwork waypointNetwork = null;
    [SerializeField] protected bool randomPatrol = false;
    [SerializeField] protected int currentWaypoint = -1;
    [SerializeField] [Range(0, 15)] protected float stoppingDistance = 1.0f;

    protected Animator animator = null;
    protected NavMeshAgent navAgent = null;
    protected Collider collider = null;

    public bool isTargetReached {  get { return _isTargetReached; } }
    public bool inMeleeRange { get; set; }
    public Animator GetAnimator { get { return animator; } }
    public NavMeshAgent GetNavAgent { get { return navAgent; } }
    public Vector3 sensorPosition
    {
        get {
            if (sensorTrigger == null) return Vector3.zero;

            Vector3 point = sensorTrigger.transform.position;
            point.x += sensorTrigger.center.x * sensorTrigger.transform.lossyScale.x;
            point.y += sensorTrigger.center.y * sensorTrigger.transform.lossyScale.y;
            point.z += sensorTrigger.center.z * sensorTrigger.transform.lossyScale.z;
            return point;
        }
    }

    public float sensorRadius
    {
        get
        {
            if (sensorTrigger == null) return 0;

            float radius = Mathf.Max(sensorTrigger.radius * sensorTrigger.transform.lossyScale.x,
                                     sensorTrigger.radius * sensorTrigger.transform.lossyScale.y);
            return Mathf.Max(radius, sensorTrigger.radius * sensorTrigger.transform.lossyScale.z);
        }
    }

    public bool useRootPosition { get { return rootPositionRefCount > 0; } }
    public bool useRootRotation { get { return rootRotationRefCount > 0; } }
    public AITargetType targetType { get { return target.GetType; } }
    public Vector3 targetPosition { get { return target.GetPosition; } }
    public int targetColliderID
    {
        get
        {
            if (target.GetCollider)
                return target.GetCollider.GetInstanceID();
            else
                return -1;
        }
    }
    public bool cinematicEnabled
    {
        get { return _cinematicEnabled; }
        set { _cinematicEnabled = value; }
    }

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        collider = GetComponent<Collider>();

        aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");

        if (GameSceneManager.GetInstance() != null)
        {
            if (collider != null)
            {
                GameSceneManager.GetInstance().RegisterAIStateMachine(collider.GetInstanceID(), this);
            }
            if (sensorTrigger != null)
            {
                GameSceneManager.GetInstance().RegisterAIStateMachine(sensorTrigger.GetInstanceID(), this);
            }
        }

        if (rootBone != null)
        {
            Rigidbody[] bodies = rootBone.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody bodyPart in bodies)
            {
                if (bodyPart != null && bodyPart.gameObject.layer == aiBodyPartLayer)
                {
                    bodyParts.Add(bodyPart);
                    GameSceneManager.GetInstance().RegisterAIStateMachine(bodyPart.GetInstanceID(), this);
                }
            }

        }
    }

    protected virtual void Start()
    {
        if (sensorTrigger != null)
        {
            AISensor script = sensorTrigger.GetComponent<AISensor>();
            if (script != null)
            {
                script.SetParentStateMachine = this;
            }
        }

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

        if (animator != null)
        {
            AIStateMachineLink[] scripts = animator.GetBehaviours<AIStateMachineLink>();
            foreach(AIStateMachineLink script in scripts)
            {
                script.SetStateMachine = this;
            }
        }
    }

    public Vector3 GetWaypointPosition(bool increment)
    {
        if (currentWaypoint == -1)
        {
            if (randomPatrol)
                currentWaypoint = Random.Range(0, waypointNetwork.Waypoints.Count);
            else
                currentWaypoint = 0;
        }
        else if (increment)
            NextWaypoint();
        
        if (waypointNetwork.Waypoints[currentWaypoint] != null)
        {
            Transform newWaypoint = waypointNetwork.Waypoints[currentWaypoint];
            
            SetTarget(AITargetType.Waypoint,
                        null,
                        newWaypoint.position,
                        Vector3.Distance(newWaypoint.position, transform.position));

            return newWaypoint.position;
        }

        return Vector3.zero;
    }
    
    private void NextWaypoint()
    {
        if (randomPatrol && waypointNetwork.Waypoints.Count > 1)
        {
            int oldWaypoint = currentWaypoint;
            while (currentWaypoint == oldWaypoint)
            {
                currentWaypoint = Random.Range(0, waypointNetwork.Waypoints.Count);
            }
        }
        else
            currentWaypoint = currentWaypoint == waypointNetwork.Waypoints.Count - 1 ? 0 : currentWaypoint + 1;
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

        _isTargetReached = false;
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

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (targetTrigger == null || other != targetTrigger) return;

        _isTargetReached = true;

        if (currentState != null)
        {
            currentState.OnDestinationReached(true);
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (targetTrigger == null || other != targetTrigger) return;

        _isTargetReached = true;
    }

    protected void OnTriggerExit(Collider other)
    {
        if (targetTrigger == null || other != targetTrigger) return;

        _isTargetReached = false;

        if (currentState != null)
        {
            currentState.OnDestinationReached(false);
        }
    }

    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)
    {
        if (currentState != null)
        {
            currentState.OnTriggerEvent(type, other);
        }
    }

    protected virtual void OnAnimatorMove()
    {
        if (currentState != null)
        {
            currentState.OnAnimatorUpdated();
        }
    }

    protected virtual void OnAnimatorIK(int layer)
    {
        if (currentState != null)
        {
            currentState.OnAnimatorIKUpdated();
        }
    }

    public void NavAgentControl(bool positionUpdate, bool rotationUpdate)
    {
        if (navAgent != null)
        {
            navAgent.updatePosition = positionUpdate;
            navAgent.updateRotation = rotationUpdate;
        }
    }

    public void AddRootMotionRequest(int rootPosition, int rootRotation)
    {
        rootPositionRefCount += rootPosition;
        rootRotationRefCount += rootRotation;
    }

    public virtual void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager, int hitDirection = 0)
    {
       
    }

}
