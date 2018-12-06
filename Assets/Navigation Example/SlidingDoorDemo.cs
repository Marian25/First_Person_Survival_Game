using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorState { Open, Animating, Closed };

public class SlidingDoorDemo : MonoBehaviour {

    public float slidingDistance = 4.0f;
    public float duration = 1.5f;
    public AnimationCurve jumpCurve = new AnimationCurve();

    private Transform cachedTransform = null;
    private Vector3 openPos = Vector3.zero;
    private Vector3 closedPos = Vector3.zero;
    private DoorState doorState = DoorState.Closed;


	// Use this for initialization
	void Start () {
        cachedTransform = transform;
        closedPos = transform.position;
        openPos = closedPos - (transform.right * slidingDistance);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space) && doorState != DoorState.Animating) {
            StartCoroutine(AnimateDoor((doorState == DoorState.Open) ? DoorState.Closed : DoorState.Open));
        }
	}

    IEnumerator AnimateDoor(DoorState newState) {
        doorState = DoorState.Animating;
        float time = 0.0f;

        Vector3 startPos = (newState == DoorState.Open) ? closedPos : openPos;
        Vector3 endPos = (newState == DoorState.Open) ? openPos : closedPos;

        while (time <= duration) {
            float t = time / duration;

            cachedTransform.position = Vector3.Lerp(startPos, endPos, jumpCurve.Evaluate(t));
            time += Time.deltaTime;
            yield return null;
        }

        cachedTransform.position = endPos;
        doorState = newState;
    }


}
