using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    private Animator animator = null;
    private int horizontalHash = 0;
    private int verticalHash = 0;
    private int attackHash = 0;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        horizontalHash = Animator.StringToHash("horizontal");
        verticalHash = Animator.StringToHash("vertical");
        attackHash = Animator.StringToHash("attack");
	}
	
	// Update is called once per frame
	void Update () {
        float xAxis = Input.GetAxis("Horizontal") * 2.32f;
        float yAxis = Input.GetAxis("Vertical") * 5.66f;

        if (Input.GetMouseButtonDown(0)) {
            animator.SetTrigger(attackHash);
        }

        animator.SetFloat(horizontalHash, xAxis, 0.1f, Time.deltaTime);
        animator.SetFloat(verticalHash, yAxis, 1.0f, Time.deltaTime);
	}
}
