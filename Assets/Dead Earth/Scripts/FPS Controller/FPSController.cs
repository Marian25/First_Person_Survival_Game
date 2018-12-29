using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerMoveStatus { NotMoving, Crouching, Walking, Running, NotGrounded, Landing }
public enum CurveControlledBobCallbackType { Vertical, Horizontal }

public delegate void CurveControlledBobCallback();

[System.Serializable]
public class CurveControlledBobEvent
{
    public float time = 0;
    public CurveControlledBobCallback function = null;
    public CurveControlledBobCallbackType type = CurveControlledBobCallbackType.Vertical;

}

[System.Serializable]
public class CurveControlledBob
{
    [SerializeField]
    AnimationCurve bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                 new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                 new Keyframe(2f, 0f));
    [SerializeField] float horizontalMultiplier = 0.01f;
    [SerializeField] float verticalMultiplier = 0.02f;
    [SerializeField] float verticalToHorizontalSpeedRatio = 2.0f;
    [SerializeField] float baseInterval = 1.0f;

    private float prevXPlayHead;
    private float prevYPlayHead; 
    private float xPlayHead;
    private float yPlayHead;
    private float curveEndTime;
    private List<CurveControlledBobEvent> events = new List<CurveControlledBobEvent>();

    public void Initialize()
    {
        curveEndTime = bobcurve[bobcurve.length - 1].time;
        xPlayHead = 0;
        yPlayHead = 0;
        prevXPlayHead = 0;
        prevYPlayHead = 0;
    }

    public void RegisterEventCallback(float time, CurveControlledBobCallback function, CurveControlledBobCallbackType type)
    {
        CurveControlledBobEvent ccbeEvent = new CurveControlledBobEvent();
        ccbeEvent.time = time;
        ccbeEvent.function = function;
        ccbeEvent.type = type;
        events.Add(ccbeEvent);

        events.Sort(
            delegate (CurveControlledBobEvent t1, CurveControlledBobEvent t2)
            {
                return t1.time.CompareTo(t2.time);
            }
        );
    }

    public Vector3 GetVectorOffset(float speed)
    {
        xPlayHead += (speed * Time.deltaTime) / baseInterval;
        yPlayHead += ((speed * Time.deltaTime) / baseInterval) * verticalToHorizontalSpeedRatio;

        if (xPlayHead > curveEndTime)
        {
            xPlayHead = 0;
        }

        if (yPlayHead > curveEndTime)
        {
            yPlayHead = 0;
        }
        
        for (int i = 0; i < events.Count; i++)
        {
            CurveControlledBobEvent ev = events[i];
            if (ev != null)
            {
                if (ev.type == CurveControlledBobCallbackType.Vertical)
                {
                    if ((prevYPlayHead < ev.time && yPlayHead >= ev.time) ||
                        (prevYPlayHead > yPlayHead && (ev.time > prevYPlayHead || ev.time <= yPlayHead)))
                    {
                        ev.function();
                    }
                } else if(ev.type == CurveControlledBobCallbackType.Horizontal)
                {
                    if ((prevXPlayHead < ev.time && xPlayHead >= ev.time) ||
                        (prevXPlayHead > xPlayHead && (ev.time > prevXPlayHead || ev.time <= xPlayHead)))
                    {
                        ev.function();
                    }
                }
            }
        }

        float xPos = bobcurve.Evaluate(xPlayHead) * horizontalMultiplier;
        float yPos = bobcurve.Evaluate(yPlayHead) * verticalMultiplier;

        prevXPlayHead = xPlayHead;
        prevYPlayHead = yPlayHead;

        return new Vector3(xPos, yPos, 0);
    }

}

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour {

    public List<AudioSource> audioSources = new List<AudioSource>();
    private int audioToUse = 0;

    [SerializeField] private float _walkSpeed = 2.0f;
    [SerializeField] private float _runSpeed = 4.5f;
    [SerializeField] private float jumpSpeed = 7.5f;
    [SerializeField] private float crouchSpeed = 1.0f;
    [SerializeField] private float stickToGroundForce = 5.0f;
    [SerializeField] private float gravityMultiplier = 2.5f;
    [SerializeField] private float runStepLengthen = 0.75f;

    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.MouseLook mouseLook;
    [SerializeField] private CurveControlledBob headbob = new CurveControlledBob();
    [SerializeField] private GameObject flashlight = null;

    private Camera camera = null;
    private bool jumpButtonPressed = false;
    private Vector2 inputVector = Vector2.zero;
    private Vector3 moveDirection = Vector3.zero;
    private bool previoulyGrounded = false;
    private bool isWalking = true;
    private bool isJumping = false;
    private bool isCrouching = false;
    private Vector3 localSpaceCameraPos = Vector3.zero;
    private float controllerHeight = 0;

    private float fallingTimer = 0;

    private CharacterController _characterController = null;
    private PlayerMoveStatus _movementStatus = PlayerMoveStatus.NotMoving;

    public PlayerMoveStatus movementStatus { get { return _movementStatus; } }
    public float walkSpeed { get { return _walkSpeed; } }
    public float runSpeed { get { return _runSpeed; } }

    float _dragMultiplier = 1;
    float _dragMultiplierLimit = 1;
    [SerializeField] [Range(0, 1)] float npcStickiness = 0.5f; 

    public float dragMultiplierLimit
    {
        get { return _dragMultiplierLimit; }
        set { _dragMultiplierLimit = Mathf.Clamp01(value); }
    }

    public float dragMultiplier
    {
        get { return _dragMultiplier; }
        set { _dragMultiplier = Mathf.Min(value, dragMultiplierLimit); }
    }

    public CharacterController characterController
    {
        get { return _characterController; }
    }

    protected void Start()
    {
        _characterController = GetComponent<CharacterController>();
        controllerHeight = _characterController.height;

        camera = Camera.main;
        localSpaceCameraPos = camera.transform.localPosition;

        _movementStatus = PlayerMoveStatus.NotMoving;

        fallingTimer = 0;

        mouseLook.Init(transform, camera.transform);

        headbob.Initialize();
        headbob.RegisterEventCallback(1.5f, PlayFootStepSound, CurveControlledBobCallbackType.Vertical);

        if (flashlight) flashlight.SetActive(false);
    }

    protected void Update()
    {
        if (_characterController.isGrounded) fallingTimer = 0;
        else fallingTimer += Time.deltaTime;

        if (Time.timeScale > Mathf.Epsilon)
        {
            mouseLook.LookRotation(transform, camera.transform);
        }

        if (Input.GetButtonDown("Flashlight"))
        {
            if (flashlight) flashlight.SetActive(!flashlight.activeSelf);
        }

        if (!jumpButtonPressed && !isCrouching)
        {
            jumpButtonPressed = Input.GetButtonDown("Jump"); 
        }

        if (Input.GetButtonDown("Crouch"))
        {
            isCrouching = !isCrouching;
            _characterController.height = isCrouching == true ? controllerHeight / 2 : controllerHeight;
        }

        if (!previoulyGrounded && _characterController.isGrounded)
        {
            if (fallingTimer > 0.5f)
            {
                // TODO: play landing sound
            }

            moveDirection.y = 0;
            isJumping = false;
            _movementStatus = PlayerMoveStatus.Landing;
        }
        else if (!_characterController.isGrounded)
        {
            _movementStatus = PlayerMoveStatus.NotGrounded;
        }
        else if (_characterController.velocity.sqrMagnitude < 0.01f)
        {
            _movementStatus = PlayerMoveStatus.NotMoving;
        }
        else if (isCrouching)
        {
            _movementStatus = PlayerMoveStatus.Crouching;
        }
        else if (isWalking)
        {
            _movementStatus = PlayerMoveStatus.Walking;
        }
        else
        {
            _movementStatus = PlayerMoveStatus.Running;
        }

        previoulyGrounded = _characterController.isGrounded;

        dragMultiplier = Mathf.Min(dragMultiplier + Time.deltaTime, dragMultiplierLimit);
    }

    protected void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool wasWalking = isWalking;
        isWalking = !Input.GetKey(KeyCode.LeftShift);

        float speed = isCrouching ? crouchSpeed : isWalking ? _walkSpeed : _runSpeed;
        inputVector = new Vector2(horizontal, vertical);

        if (inputVector.sqrMagnitude > 1) inputVector.Normalize();

        Vector3 desiredMove = transform.forward * inputVector.y + transform.right * inputVector.x;

        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, _characterController.radius, Vector3.down, out hitInfo, _characterController.height / 2f, 1))
        {
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
        }

        moveDirection.x = desiredMove.x * speed * dragMultiplier;
        moveDirection.z = desiredMove.z * speed * dragMultiplier;

        if (_characterController.isGrounded)
        {
            moveDirection.y = -stickToGroundForce;

            if (jumpButtonPressed)
            {
                moveDirection.y = jumpSpeed;
                jumpButtonPressed = false;
                isJumping = true;
                // TODO: play jumping sound
            }
        } else
        {
            moveDirection += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }

        _characterController.Move(moveDirection * Time.fixedDeltaTime);

        Vector3 speedXZ = new Vector3(_characterController.velocity.x, 0, _characterController.velocity.z);
        if (speedXZ.magnitude > 0.01f)
        {
            camera.transform.localPosition = localSpaceCameraPos + headbob.GetVectorOffset(speedXZ.magnitude * (isCrouching || isWalking ? 1 : runStepLengthen));
        } else
        {
            camera.transform.localPosition = localSpaceCameraPos;
        }


    }

    void PlayFootStepSound()
    {
        if (isCrouching) return;

        audioSources[audioToUse].Play();
        audioToUse = (audioToUse == 0) ? 1 : 0;
    }

    public void DoStickiness()
    {
        dragMultiplier = 1.0f - npcStickiness;
    }

}
