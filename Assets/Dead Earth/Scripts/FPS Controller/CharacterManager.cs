using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour {

    [SerializeField] private CapsuleCollider meleeTrigger = null;
    [SerializeField] private CameraBloodEffect cameraBloodEffect = null;
    [SerializeField] private Camera camera = null;
    [SerializeField] private float _health = 100f;
    [SerializeField] private AISoundEmitter soundEmitter = null;
    [SerializeField] private float walkRadius = 0;
    [SerializeField] private float runRadius = 7.0f;
    [SerializeField] private float landingRadius = 12.0f;
    [SerializeField] private float bloodRadiusScale = 6.0f;
    [SerializeField] private PlayerHUD _playerHUD = null;

    // Pain Damage Audio
    [SerializeField] private AudioCollection _damageSounds = null;
    [SerializeField] private AudioCollection _painSounds = null;
    [SerializeField] private AudioCollection _tauntSounds = null;

    [SerializeField] private float _nextPainSoundTime = 0.0f;
    [SerializeField] private float _painSoundOffset = 0.35f;
    [SerializeField] private float _tauntRadius = 10.0f;

    private Collider collider = null;
    private FPSController _fpsController = null;
    private CharacterController characterController = null;
    private GameSceneManager gameSceneManager = null;
    private int aiBodyPartLayer = -1;
    private int _interactiveMask = 0;
    private float _nextTauntTime = 0;

    public float health { get { return _health; } }
    public float stamina { get { return _fpsController != null ? _fpsController.stamina : 0.0f; } }
    public FPSController fpsController { get { return _fpsController; } }

    // Use this for initialization
    void Start () {
        collider = GetComponent<Collider>();
        _fpsController = GetComponent<FPSController>();
        characterController = GetComponent<CharacterController>();
        gameSceneManager = GameSceneManager.GetInstance();

        aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");
        _interactiveMask = 1 << LayerMask.NameToLayer("Interactive");
         
        if (gameSceneManager != null)
        {
            PlayerInfo info = new PlayerInfo();
            info.camera = camera;
            info.characterManager = this;
            info.collider = collider;
            info.meleeTrigger = meleeTrigger;

            gameSceneManager.RegisterPlayerInfo(collider.GetInstanceID(), info);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (_playerHUD) _playerHUD.Fade(2.0f, ScreenFadeType.FadeIn);
    }

    public void TakeDamage(float amount, bool doDamage, bool doPain)
    {
        _health = Mathf.Max(health - amount * Time.deltaTime, 0);

        if (_fpsController)
        {
            _fpsController.dragMultiplier = 0;
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

        if (_health <= 0.0f)
        {
            DoDeath();
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
                int randomDamage = Random.Range(5, 10);
                stateMachine.TakeDamage(hit.point, ray.direction * 5.0f, randomDamage, hit.rigidbody, this, 0);
            }
        }

    }

	// Update is called once per frame
	void Update () {

        Ray ray;
        RaycastHit hit;
        RaycastHit[] hits;

        ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        
        float rayLength = Mathf.Lerp(1.0f, 1.8f, Mathf.Abs(Vector3.Dot(camera.transform.forward, Vector3.up)));
        
        hits = Physics.RaycastAll(ray, rayLength, _interactiveMask);
        
        if (hits.Length > 0)
        {
            int highestPriority = int.MinValue;
            InteractiveItem priorityObject = null;
            
            for (int i = 0; i < hits.Length; i++)
            {
                hit = hits[i];
                
                InteractiveItem interactiveObject = gameSceneManager.GetInteractiveItem(hit.collider.GetInstanceID());
                
                if (interactiveObject != null && interactiveObject.priority > highestPriority)
                {
                    priorityObject = interactiveObject;
                    highestPriority = priorityObject.priority;
                }
            }
            
            if (priorityObject != null)
            {
                if (_playerHUD)
                    _playerHUD.SetInteractionText(priorityObject.GetText());

                if (Input.GetButtonDown("Use"))
                {
                    priorityObject.Activate(this);
                }
            }
        }
        else
        {
            if (_playerHUD)
                _playerHUD.SetInteractionText(null);
        }

        if (Input.GetMouseButtonDown(0))
        {
            DoDamage();
        }

        if (_fpsController)
        {
            float newRadius = Mathf.Max(walkRadius, (100.0f - health) / bloodRadiusScale);

            switch (_fpsController.movementStatus)
            {
                case PlayerMoveStatus.Landing: newRadius = Mathf.Max(newRadius, landingRadius); break;
                case PlayerMoveStatus.Running: newRadius = Mathf.Max(newRadius, runRadius); break;
            }

            soundEmitter.SetRadius(newRadius);

            _fpsController.dragMultiplierLimit = Mathf.Max(health / 100.0f, 0.25f);
        }

        if (Input.GetMouseButtonDown(1))
        {
            DoTaunt();
        }

        if (_playerHUD) _playerHUD.Invalidate(this);
    }

    void DoTaunt()
    {
        if (_tauntSounds == null || Time.time < _nextTauntTime || !AudioManager.instance) return;
        AudioClip taunt = _tauntSounds[0];
        AudioManager.instance.PlayOneShotSound(_tauntSounds.audioGroup,
                                                taunt,
                                                transform.position,
                                                _tauntSounds.volume,
                                                _tauntSounds.spatialBlend,
                                                _tauntSounds.priority
                                                 );
        if (soundEmitter != null)
            soundEmitter.SetRadius(_tauntRadius);
        _nextTauntTime = Time.time + taunt.length;
    }

    public void DoLevelComplete()
    {
        if (_fpsController)
            _fpsController.freezeMovement = true;

        if (_playerHUD)
        {
            _playerHUD.Fade(4.0f, ScreenFadeType.FadeOut);
            _playerHUD.ShowMissionText("Mission Completed");
            _playerHUD.Invalidate(this);
        }

        Invoke("GameOver", 4.0f);
    }

    public void DoDeath()
    {
        if (_fpsController)
            _fpsController.freezeMovement = true;

        if (_playerHUD)
        {
            _playerHUD.Fade(3.0f, ScreenFadeType.FadeOut);
            _playerHUD.ShowMissionText("Mission Failed");
            _playerHUD.Invalidate(this);
        }

        Invoke("GameOver", 3.0f);
    }

    void GameOver()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (ApplicationManager.instance)
            ApplicationManager.instance.LoadMainMenu();
    }
}
