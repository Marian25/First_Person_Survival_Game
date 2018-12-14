using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentNoRootMotion : MonoBehaviour {

    public AIWaypointNetwork waypointNetwork = null;
    public int currentIndex = 0;
    public bool hasPath = false;
    public bool pathPending = false;
    public NavMeshPathStatus pathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve jumpCurve = new AnimationCurve();

    private NavMeshAgent navAgent = null;
    private Animator animator = null;
    private float originalMaxSpeed = 0;

	// Use this for initialization
	void Start () {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        originalMaxSpeed = navAgent.speed;

        if (waypointNetwork == null) {
            return;
        }

        SetNextDestination(false);
	}
	
    void SetNextDestination (bool increment) {
        if (waypointNetwork == null) {
            return;
        }

        int incStep = increment ? 1 : 0;   
        int nextWaypoint = (currentIndex + incStep >= waypointNetwork.Waypoints.Count) ? 0 : currentIndex + incStep;

        Transform nextWaypointTransform = waypointNetwork.Waypoints[nextWaypoint];

        if (nextWaypointTransform != null) {
            currentIndex = nextWaypoint;
            navAgent.destination = nextWaypointTransform.position;
            return;
        }

        currentIndex++;
    }

	// Update is called once per frame
	void Update () {
        int turnOnSpot;

        hasPath = navAgent.hasPath;
        pathPending = navAgent.pathPending;
        pathStatus = navAgent.pathStatus;

        Vector3 cross = Vector3.Cross(transform.forward, navAgent.desiredVelocity.normalized);
        float horizontal = (cross.y < 0) ? cross.magnitude : cross.magnitude;
        horizontal = Mathf.Clamp(horizontal * 2.32f, -2.32f, 2.32f);

        if (navAgent.desiredVelocity.magnitude < 1.0f && Vector3.Angle(transform.forward, navAgent.desiredVelocity) > 10.0f)
        {
            navAgent.speed = 0.1f;
            turnOnSpot = (int) Mathf.Sign(horizontal);
        } else
        {
            navAgent.speed = originalMaxSpeed;
            turnOnSpot = 0;
        }

        animator.SetFloat("horizontal", horizontal, 0.1f, Time.deltaTime);
        animator.SetFloat("vertical", navAgent.desiredVelocity.magnitude, 0.1f, Time.deltaTime);
        animator.SetInteger("turnOnSpot", turnOnSpot);

        /*
        if (navAgent.isOnOffMeshLink) {
            StartCoroutine(Jump(1.0f));
            return;
        }
        */

        if ((!hasPath && !pathPending) || pathStatus == NavMeshPathStatus.PathInvalid) {
            SetNextDestination(true);
        } else if (navAgent.isPathStale) {
            SetNextDestination(false);
        }
	}

    IEnumerator Jump(float duration) {
        OffMeshLinkData data = navAgent.currentOffMeshLinkData;
        Vector3 startPos = navAgent.transform.position;
        Vector3 endPos = data.endPos + (navAgent.baseOffset * Vector3.up);

        float time = 0.0f;

        while (time <= duration) {
            float t = time / duration;
            navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + jumpCurve.Evaluate(t) * Vector3.up;
            time += Time.deltaTime;
            yield return null;
        }

        navAgent.CompleteOffMeshLink();
    }

}
