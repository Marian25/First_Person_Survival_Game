using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour {

    [SerializeField] private CapsuleCollider meleeTrigger = null;
    [SerializeField] private CameraBloodEffect cameraBloodEffect = null;
    [SerializeField] private Camera camera = null;
    [SerializeField] private float health = 100f;
    [SerializeField] private AISoundEmitter soundEmitter = null;
    [SerializeField] private float walkRadius = 0;
    [SerializeField] private float runRadius = 7.0f;
    [SerializeField] private float landingRadius = 12.0f;
    [SerializeField] private float bloodRadiusScale = 6.0f;

    // Pain Damage Audio
    [SerializeField] private AudioCollection _damageSounds = null;
    [SerializeField] private AudioCollection _painSounds = null;
    [SerializeField] private float _nextPainSoundTime = 0.0f;
    [SerializeField] private float _painSoundOffset = 0.35f;

    private Collider collider = null;
    private FPSController fpsController = null;
    private CharacterController characterController = null;
    private GameSceneManager gameSceneManager = null;
    private int aiBodyPartLayer = -1;

	// Use this for initialization
	void Start () {
        collider = GetComponent<Collider>();
        fpsController = GetComponent<FPSController>();
        characterController = GetComponent<CharacterController>();
        gameSceneManager = GameSceneManager.GetInstance();

        aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");

        if (gameSceneManager != null)
        {
            PlayerInfo info = new PlayerInfo();
            info.camera = camera;
            info.characterManager = this;
            info.collider = collider;
            info.meleeTrigger = meleeTrigger;

            gameSceneManager.RegisterPlayerInfo(collider.GetInstanceID(), info);
        }
	}

    public void TakeDamage(float amount, bool doDamage, bool doPain)
    {
        health = Mathf.Max(health - amount * Time.deltaTime, 0);

        if (fpsController)
        {
            fpsController.dragMultiplier = 0;
        }

        if (cameraBloodEffect != null)
        {
            cameraBloodEffect.minBloodAmount = (1f - health / 100f) * 0.5f;
            cameraBloodEffect.bloodAmount = Mathf.Min(cameraBloodEffect.minBloodAmount + 0.3f, 1f);
        }

        if (AudioManager.instance)
        {
            if (doDamage && _damageSounds != null)
                AudioManager.instance.PlayOneShotSound(_damageSounds.audioGroup,
                                                        _damageSounds.audioClip, 
                                                        transform.position,
                                                        _damageSounds.volume,
                                                        _damageSounds.spatialBlend,
                                                        _damageSounds.priority);

            if (doPain && _painSounds != null && _nextPainSoundTime < Time.time)
            {
                AudioClip painClip = _painSounds.audioClip;
                if (painClip)
                {
                    _nextPainSoundTime = Time.time + painClip.length;
                    StartCoroutine(AudioManager.instance.PlayOneShotSoundDelayed(_painSounds.audioGroup,
                                                                                      painClip,
                                                                                      transform.position,
                                                                                      _painSounds.volume,
                                                                                      _painSounds.spatialBlend,
                                                                                      _painSoundOffset,
                                                                                      _painSounds.priority));
                }
            }
        }
    }
	
    public void DoDamage(int hitDirection = 0)
    {
        if (camera == null) return;
        if (gameSceneManager == null) return;

        Ray ray;
        RaycastHit hit;
        bool isSomethingHit = false;

        ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        isSomethingHit = Physics.Raycast(ray, out hit, 1000, 1 << aiBodyPartLayer);

        if (isSomethingHit)
        {
            AIStateMachine stateMachine = gameSceneManager.GetAIStateMachine(hit.rigidbody.GetInstanceID());

            if (stateMachine)
            {
                stateMachine.TakeDamage(hit.point, ray.direction * 1.0f, 50, hit.rigidbody, this, 0);
            }
        }

    }

	// Update is called once per frame
	void Update () {
		
        if (Input.GetMouseButtonDown(0))
        {
            DoDamage();
        }

        if (fpsController)
        {
            float newRadius = Mathf.Max(walkRadius, (100.0f - health) / bloodRadiusScale);

            switch (fpsController.movementStatus)
            {
                case PlayerMoveStatus.Landing: newRadius = Mathf.Max(newRadius, landingRadius); break;
                case PlayerMoveStatus.Running: newRadius = Mathf.Max(newRadius, runRadius); break;
            }

            soundEmitter.SetRadius(newRadius);

            fpsController.dragMultiplierLimit = Mathf.Max(health / 100.0f, 0.25f);
        }

	}
}
