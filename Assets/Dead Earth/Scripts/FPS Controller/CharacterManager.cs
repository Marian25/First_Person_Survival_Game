using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour {

    [SerializeField] private CapsuleCollider meleeTrigger = null;
    [SerializeField] private CameraBloodEffect cameraBloodEffect = null;
    [SerializeField] private Camera camera = null;
    [SerializeField] private float health = 100f;

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

    public void TakeDamage(float amount)
    {
        health = Mathf.Max(health - amount * Time.deltaTime, 0);

        if (cameraBloodEffect != null)
        {
            cameraBloodEffect.minBloodAmount = (1f - health / 100f) / 3;
            cameraBloodEffect.bloodAmount = Mathf.Min(cameraBloodEffect.minBloodAmount + 0.3f, 1f);
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
                stateMachine.TakeDamage(hit.point, ray.direction * 5.0f, 25, hit.rigidbody, this, 0);
            }
        }

    }

	// Update is called once per frame
	void Update () {
		
        if (Input.GetMouseButtonDown(0))
        {
            DoDamage();
        }

	}
}
