using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIBoneControlType { Animated, Ragdoll, RagdollToAnim }

public class AIZombieStateMachine : AIStateMachine {

    [SerializeField] [Range(10f, 360f)] private float _fov = 50f;
    [SerializeField] [Range(0f, 1f)] private float _sight = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float _hearing = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float _aggression = 0.5f;
    [SerializeField] [Range(0, 100)] private int _health = 100;
    [SerializeField] [Range(0, 100)] private int lowerBodyDamage = 0;
    [SerializeField] [Range(0, 100)] private int upperBodyDamage = 0;
    [SerializeField] [Range(0, 100)] private int upperBodyThreshold = 30;
    [SerializeField] [Range(0, 100)] private int limpThreshold = 30;
    [SerializeField] [Range(0, 100)] private int crawlThreshold = 90;
    [SerializeField] [Range(0f, 1f)] private float _intelligence = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float _satisfaction = 1.0f;
    [SerializeField] float _replenishRate = 0.5f;
    [SerializeField] float _depletionRate = 0.1f;

    private int _seeking = 0;
    private bool _feeding = false;
    private bool _crawling = false;
    private int _attackType = 0;
    private float _speed = 0;

    private AIBoneControlType boneControlType = AIBoneControlType.Animated;

    // Hashes
    private int speedHash = Animator.StringToHash("speed");
    private int feedingHash = Animator.StringToHash("feeding");
    private int seekingHash = Animator.StringToHash("seeking");
    private int attackHash = Animator.StringToHash("attack");
    private int crawlingHash = Animator.StringToHash("crawling");
    private int hitTriggerHash = Animator.StringToHash("hit");
    private int hitTypeHash = Animator.StringToHash("hitType");

    // Getters
    public float replenishRate  { get { return _replenishRate; } }
    public float fov            { get { return _fov; } }
    public float sight          { get { return _sight; } }
    public float hearing        { get { return _hearing; } }
    public bool  crawling       { get { return _crawling; } }
    public float intelligence   { get { return _intelligence; } }
    public float satisfaction   { get { return _satisfaction; } set { _satisfaction = value; } }
    public float aggression     { get { return _aggression; } set { _aggression = value; } }
    public int health           { get { return _health; } set { _health = value; } }
    public int attackType       { get { return _attackType; } set { _attackType = value; } }
    public bool feeding         { get { return _feeding; } set { _feeding = value; } }
    public int seeking          { get { return _seeking; } set { _seeking = value; } }
    public float speed          { get { return _speed; } set { _speed = value; } }
    public bool isCrawling      { get { return lowerBodyDamage >= crawlThreshold; } }

    protected override void Start()
    {
        base.Start();

        UpdateAnimatorDamage();
    }

    protected override void Update()
    {
        base.Update();

        if (animator != null)
        {
            animator.SetFloat(speedHash, speed);
            animator.SetBool(feedingHash, feeding);
            animator.SetInteger(seekingHash, seeking);
            animator.SetInteger(attackHash, attackType);
        }

        satisfaction = Mathf.Max(0, satisfaction - _depletionRate * Time.deltaTime / 100 * Mathf.Pow(speed, 3));
    }

    protected void UpdateAnimatorDamage()
    {
        if (animator != null)
        {
            animator.SetBool(crawlingHash, isCrawling);
        }
    }

    public override void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager, int hitDirection = 0)
    {
        base.TakeDamage(position, force, damage, bodyPart, characterManager, hitDirection);

        if (GameSceneManager.GetInstance() != null && GameSceneManager.GetInstance().bloodParticles != null)
        {
            ParticleSystem sys = GameSceneManager.GetInstance().bloodParticles;
            sys.transform.position = position;

            var settings = sys.main;
            settings.simulationSpace = ParticleSystemSimulationSpace.World;
            sys.Emit(60);
        }

        float hitStrength = force.magnitude;

        if (boneControlType == AIBoneControlType.Ragdoll)
        {
            if (bodyPart != null)
            {
                if (hitStrength > 1.0f)
                {
                    bodyPart.AddForce(force, ForceMode.Impulse);
                }

                if (bodyPart.CompareTag("Head"))
                {
                    health = Mathf.Max(health - damage, 0);
                } else if (bodyPart.CompareTag("Upper Body"))
                {
                    upperBodyDamage += damage;
                } else if (bodyPart.CompareTag("Lower Body"))
                {
                    lowerBodyDamage += damage;
                }

                UpdateAnimatorDamage();

                if (health > 0)
                {
                    // TODO: reanimate zombie
                }
            }
            return;
        }

        Vector3 attackerLocPos = transform.InverseTransformPoint(characterManager.transform.position);
        Vector3 hitLocPos = transform.InverseTransformPoint(position);
        
        bool shouldRagdoll = hitStrength > 1f;

        if (bodyPart != null)
        {
            if (bodyPart.CompareTag("Head"))
            {
                health = Mathf.Max(health - damage, 0);
                if (health == 0) shouldRagdoll = true;
            }
            else if (bodyPart.CompareTag("Upper Body"))
            {
                upperBodyDamage += damage;
                UpdateAnimatorDamage();
            }
            else if (bodyPart.CompareTag("Lower Body"))
            {
                lowerBodyDamage += damage;
                UpdateAnimatorDamage();
                shouldRagdoll = true;
            }
        }

        if (boneControlType != AIBoneControlType.Animated || isCrawling || cinematicEnabled || attackerLocPos.z < 0)
            shouldRagdoll = true;

        if (!shouldRagdoll)
        {
            float angle = 0;
            if (hitDirection == 0)
            {
                Vector3 vecToHit = (position - transform.position).normalized;
                angle = AIState.FindSignedAngle(vecToHit, transform.forward);
            }

            int hitType = 0;
            if (bodyPart.gameObject.CompareTag("Head"))
            {
                if (angle < -10     || hitDirection == -1)  hitType = 1;
                else if (angle > 10 || hitDirection == 1)   hitType = 3;
                else hitType = 2;
            } else if (bodyPart.gameObject.CompareTag("Upper Body"))
            {
                if (angle < -20     || hitDirection == -1)  hitType = 4;
                else if (angle > 20 || hitDirection == 1)   hitType = 6;
                else hitType = 5;
            }

            if (animator)
            {
                animator.SetInteger(hitTypeHash, hitType);
                animator.SetTrigger(hitTriggerHash);
            }

            return;

        } else
        {
            if (currentState != null)
            {
                currentState.OnExitState();
                currentState = null;
                currentStateType = AIStateType.None;
            }

            if (navAgent) navAgent.enabled = false;
            if (animator) animator.enabled = false;
            if (collider) collider.enabled = false;

            inMeleeRange = false;

            foreach (Rigidbody body in bodyParts)
            {
                if (body) body.isKinematic = false;
            }

            if (hitStrength > 1.0f)
            {
                bodyPart.AddForce(force, ForceMode.Impulse);
            }

            boneControlType = AIBoneControlType.Ragdoll;

            if (health > 0)
            {
                // TODO: Reanimate Zombie
            }
        }

    }

}
