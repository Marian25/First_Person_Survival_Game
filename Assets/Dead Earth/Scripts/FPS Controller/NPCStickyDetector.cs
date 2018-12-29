using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStickyDetector : MonoBehaviour {

    FPSController fpsController = null;

	// Use this for initialization
	void Start () {
        fpsController = GetComponentInParent<FPSController>();
	}

    private void OnTriggerStay(Collider other)
    {
        AIStateMachine machine = GameSceneManager.GetInstance().GetAIStateMachine(other.GetInstanceID());

        if (machine != null && fpsController != null)
        {
            fpsController.DoStickiness();
            machine.visualThreat.Set(AITargetType.Visual_Player,
                                    fpsController.characterController,
                                    fpsController.transform.position,
                                    Vector3.Distance(machine.transform.position, fpsController.transform.position));
            machine.SetStateOverride(AIStateType.Attack);
        }
    }

}
