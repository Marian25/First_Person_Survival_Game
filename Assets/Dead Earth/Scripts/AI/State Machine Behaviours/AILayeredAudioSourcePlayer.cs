using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AILayeredAudioSourcePlayer : AIStateMachineLink
{
    [SerializeField] AudioCollection _collection = null;
    [SerializeField] int _bank = 0;
    [SerializeField] bool _looping = true;
    [SerializeField] bool _stopOnExit = false;
    
    float _prevLayerWeight = 0.0f;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (stateMachine == null) return;
        
        float layerWeight = animator.GetLayerWeight(layerIndex);

        if (_collection != null)
        {
            if (layerIndex == 0 || layerWeight > 0.5f)
                stateMachine.PlayAudio(_collection, _bank, layerIndex, _looping);
            else
                stateMachine.StopAudio(layerIndex);
        }
        
        _prevLayerWeight = layerWeight;
    }
    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (stateMachine == null) return;
        
        float layerWeight = animator.GetLayerWeight(layerIndex);

        if (layerWeight != _prevLayerWeight && _collection != null)
        {
            if (layerWeight > 0.5f) stateMachine.PlayAudio(_collection, _bank, layerIndex, true);
            else stateMachine.StopAudio(layerIndex);
        }

        _prevLayerWeight = layerWeight;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (stateMachine && _stopOnExit)
            stateMachine.StopAudio(layerIndex);
    }

}
