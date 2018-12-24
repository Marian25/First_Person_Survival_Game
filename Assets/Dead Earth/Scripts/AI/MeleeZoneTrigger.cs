using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeZoneTrigger : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        AIStateMachine machine = GameSceneManager.GetInstance().GetAIStateMachine(other.GetInstanceID());

        if (machine) machine.inMeleeRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        AIStateMachine machine = GameSceneManager.GetInstance().GetAIStateMachine(other.GetInstanceID());

        if (machine) machine.inMeleeRange = false;
    }
}
