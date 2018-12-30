using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComChannelName { comChannel1, comChannel2, comChannel3, comChannel4 }

public class AIStateMachineLink : StateMachineBehaviour {

    protected AIStateMachine stateMachine;
    public AIStateMachine SetStateMachine { set { stateMachine = value; } }
}
