using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieStateMachine : AIStateMachine {

    [SerializeField] [Range(10f, 360f)] private float _fov = 50f;
    [SerializeField] [Range(0f, 1f)] private float _sight = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float _hearing = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float _aggression = 0.5f;
    [SerializeField] [Range(0, 100)] private int _health = 100;
    [SerializeField] [Range(0f, 1f)] private float _intelligence = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float _satisfaction = 1.0f;
    [SerializeField] float _replenishRate = 0.5f;
    [SerializeField] float _depletionRate = 0.1f;

    private int _seeking = 0;
    private bool _feeding = false;
    private bool _crawling = false;
    private int _attackType = 0;
    private float _speed = 0;

    // Hashes
    private int speedHash = Animator.StringToHash("speed");
    private int feedingHash = Animator.StringToHash("feeding");
    private int seekingHash = Animator.StringToHash("seeking");
    private int attackHash = Animator.StringToHash("attack");

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

}
