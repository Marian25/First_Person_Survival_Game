using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateMachineLink : StateMachineBehaviour {

    protected AIStateMachine stateMachine;
    public AIStateMachine SetStateMachine { set { stateMachine = value; } }
}
