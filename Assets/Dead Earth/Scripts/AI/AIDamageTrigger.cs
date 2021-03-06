﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDamageTrigger : MonoBehaviour {

    [SerializeField] string parameter = "";
    [SerializeField] int bloodParticlesBurstAmount = 10;
    [SerializeField] float damageAmount = 0.1f;
    [SerializeField] bool _doDamageSound = true;
    [SerializeField] bool _doPainSound = true;

    private AIStateMachine stateMachine = null;
    private Animator animator = null;
    private int parameterHash = -1;
    private GameSceneManager gameSceneManager = null;
    private bool _firstContact = false;

    private void Start()
    {
        stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();

        if (stateMachine != null)
        {
            animator = stateMachine.GetAnimator;
        }

        parameterHash = Animator.StringToHash(parameter);

        gameSceneManager = GameSceneManager.GetInstance();
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!animator)
            return;

        if (col.gameObject.CompareTag("Player") && animator.GetFloat(parameterHash) > 0.9f)
            _firstContact = true;
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

                var settings = system.main;
                settings.simulationSpace = ParticleSystemSimulationSpace.World;

                system.Emit(bloodParticlesBurstAmount);
            }

            if (gameSceneManager != null)
            {
                PlayerInfo info = gameSceneManager.GetPlayerInfo(other.GetInstanceID());

                if (info != null && info.characterManager != null)
                {
                    info.characterManager.TakeDamage(damageAmount, _doDamageSound && _firstContact, _doPainSound);
                }
            }

            _firstContact = false;
        }
    }
}
