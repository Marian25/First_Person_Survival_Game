using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDamageTrigger : MonoBehaviour {

    [SerializeField] string parameter = "";
    [SerializeField] int bloodParticlesBurstAmount = 10;

    private AIStateMachine stateMachine = null;
    private Animator animator = null;
    private int parameterHash = -1;

    private void Start()
    {
        stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();

        if (stateMachine != null)
        {
            animator = stateMachine.GetAnimator;
        }

        parameterHash = Animator.StringToHash(parameter);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!animator) return;

        if (other.gameObject.CompareTag("Player") && animator.GetFloat(parameterHash) > 0.9f)
        {
            if (GameSceneManager.GetInstance() && GameSceneManager.GetInstance().bloodParticles)
            {
                ParticleSystem system = GameSceneManager.GetInstance().bloodParticles;

                // temporary
                system.transform.position = transform.position;
                system.transform.rotation = Camera.main.transform.rotation;

                system.simulationSpace = ParticleSystemSimulationSpace.World;

                system.Emit(bloodParticlesBurstAmount);
            }

            Debug.Log("Player Get Damage");
        }
    }
}
