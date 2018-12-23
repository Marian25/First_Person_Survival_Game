using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerMoveStatus { NotMoving, Walking, Running, NotGrounded, Landing }

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour {

    [SerializeField] private float _walkSpeed = 1.0f;
    [SerializeField] private float _runSpeed = 4.5f;
    [SerializeField] private float jumpSpeed = 7.5f;
    [SerializeField] private float stickToGroundForce = 5.0f;
    [SerializeField] private float gravityMultiplier = 2.5f;

    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.MouseLook mouseLook;

    private Camera camera = null;
    private bool jumpButtonPressed = false;
    private Vector2 inputVector = Vector2.zero;
    private Vector3 moveDirection = Vector3.zero;
    private bool previoulyGrounded = false;
    private bool isWalking = true;
    private bool isJumping = false;

    private float fallingTimer = 0;

    private CharacterController characterController = null;
    private PlayerMoveStatus _movementStatus = PlayerMoveStatus.NotMoving;

    public PlayerMoveStatus movementStatus { get { return _movementStatus; } }
    public float walkSpeed { get { return _walkSpeed; } }
    public float runSpeed { get { return _runSpeed; } }

    protected void Start()
    {
        characterController = GetComponent<CharacterController>();
        camera = Camera.main;

        _movementStatus = PlayerMoveStatus.NotMoving;

        fallingTimer = 0;

        mouseLook.Init(transform, camera.transform);
    }

    protected void Update()
    {
        if (characterController.isGrounded) fallingTimer = 0;
        else fallingTimer += Time.deltaTime;

        if (Time.timeScale > Mathf.Epsilon)
        {
            mouseLook.LookRotation(transform, camera.transform);
        }

        if (!jumpButtonPressed)
        {
            jumpButtonPressed = Input.GetButtonDown("Jump"); 
        } 

        if (!previoulyGrounded && characterController.isGrounded)
        {
            if (fallingTimer > 0.5f)
            {
                // TODO: play landing sound
            }

            moveDirection.y = 0;
            isJumping = false;
            _movementStatus = PlayerMoveStatus.Landing;
        } else if (!characterController.isGrounded)
        {
            _movementStatus = PlayerMoveStatus.NotGrounded;
        } else if (characterController.velocity.sqrMagnitude < 0.01f)
        {
            _movementStatus = PlayerMoveStatus.NotMoving;
        } else if (isWalking)
        {
            _movementStatus = PlayerMoveStatus.Walking;
        } else
        {
            _movementStatus = PlayerMoveStatus.Running;
        }

        previoulyGrounded = characterController.isGrounded;
    }

    protected void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool wasWalking = isWalking;
        isWalking = !Input.GetKey(KeyCode.LeftShift);

        float speed = isWalking ? _walkSpeed : _runSpeed;
        inputVector = new Vector2(horizontal, vertical);

        if (inputVector.sqrMagnitude > 1) inputVector.Normalize();

        Vector3 desiredMove = transform.forward * inputVector.y + transform.right * inputVector.x;

        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo, characterController.height / 2f, 1))
        {
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
        }

        moveDirection.x = desiredMove.x * speed;
        moveDirection.z = desiredMove.z * speed;

        if (characterController.isGrounded)
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

        characterController.Move(moveDirection * Time.fixedDeltaTime);
    }




}
