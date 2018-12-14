using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentRootMotion : MonoBehaviour {

    public AIWaypointNetwork waypointNetwork = null;
    public int currentIndex = 0;
    public bool hasPath = false;
    public bool pathPending = false;
    public NavMeshPathStatus pathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve jumpCurve = new AnimationCurve();
    public bool mixedCode = true;

    private NavMeshAgent navAgent = null;
    private Animator animator = null;
    private float smoothAngle = 0;

	// Use this for initialization
	void Start () {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        navAgent.updateRotation = false;

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
    void Update() {
        hasPath = navAgent.hasPath;
        pathPending = navAgent.pathPending;
        pathStatus = navAgent.pathStatus;

        Vector3 localDesiredVelocity = transform.InverseTransformVector(navAgent.desiredVelocity);
        float angle = Mathf.Atan2(localDesiredVelocity.x, localDesiredVelocity.z) * Mathf.Rad2Deg;
        smoothAngle = Mathf.MoveTowardsAngle(smoothAngle, angle, 80.0f * Time.deltaTime);

        float speed = localDesiredVelocity.z;

        animator.SetFloat("angle", smoothAngle);
        animator.SetFloat("speed", speed, 0.1f, Time.deltaTime);

        if (navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
        {
            if (!mixedCode || (mixedCode && Mathf.Abs(angle) < 80.0f && animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion")))
            {
                Quaternion lookRotation = Quaternion.LookRotation(navAgent.desiredVelocity, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5.0f * Time.deltaTime);
            }
        }
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

    private void OnAnimatorMove()
    {
        if (mixedCode && !animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion"))
        {
            transform.rotation = animator.rootRotation;
        }
        navAgent.velocity = animator.deltaPosition / Time.deltaTime;
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
