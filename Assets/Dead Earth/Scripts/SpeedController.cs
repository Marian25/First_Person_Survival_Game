using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedController : MonoBehaviour {

    public float speed = 0.0f;

    private Animator controller = null;

	// Use this for initialization
	void Start () {
        controller = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        controller.SetFloat("speed", speed);
	}
}
