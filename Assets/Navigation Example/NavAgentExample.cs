using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour {

    public AIWaypointNetwork waypointNetwork = null;
    public int currentIndex = 0;
    public bool hasPath = false;
    public bool pathPending = false;
    public NavMeshPathStatus pathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve jumpCurve = new AnimationCurve();

    private NavMeshAgent navAgent = null;

	// Use this for initialization
	void Start () {
        navAgent = GetComponent<NavMeshAgent>();

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
        hasPath = navAgent.hasPath;
        pathPending = navAgent.pathPending;
        pathStatus = navAgent.pathStatus;

        if (navAgent.isOnOffMeshLink) {
            StartCoroutine(Jump(1.0f));
            return;
        }

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
