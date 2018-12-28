using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISoundEmitter : MonoBehaviour {

    [SerializeField] private float decayRate = 1.0f;

    private SphereCollider collider = null;
    private float sourceRadius = 0;
    private float targetRadius = 0;
    private float interpolator = 0;
    private float interpolatorSpeed = 0;

	// Use this for initialization
	void Start () {
        collider = GetComponent<SphereCollider>();
        if (collider == null) return;

        sourceRadius = targetRadius = collider.radius;

        interpolator = 0;

        if (decayRate > 0.02f)
        {
            interpolatorSpeed = 1.0f / decayRate;
        } else
        {
            interpolatorSpeed = 0;
        }
	}

    private void FixedUpdate()
    {
        if (collider == null) return;

        interpolator = Mathf.Clamp01(interpolator + Time.deltaTime * interpolatorSpeed);
        collider.radius = Mathf.Lerp(sourceRadius, targetRadius, interpolator);

        if (collider.radius < Mathf.Epsilon) collider.enabled = false;
        else collider.enabled = true;
    }

    public void SetRadius(float newRadius, bool instantResize = false)
    {
        if (collider == null || newRadius == targetRadius) return;

        sourceRadius = instantResize || newRadius > collider.radius ? newRadius : collider.radius;
        targetRadius = newRadius;
        interpolator = 0;
    }

}
