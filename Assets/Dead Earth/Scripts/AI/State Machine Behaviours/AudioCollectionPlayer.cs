using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCollectionPlayer : AIStateMachineLink {

    [SerializeField] ComChannelName commandChannel = ComChannelName.comChannel1;
    [SerializeField] AudioCollection collection = null;
    [SerializeField] CustomCurve customCurve = null;

    private int previousCommand = 0;
    private AudioManager audioManager = null;
    private int commandChannelHash = 0;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        audioManager = AudioManager.instance;
        previousCommand = 0;

        if (commandChannelHash == 0)
            commandChannelHash = Animator.StringToHash(commandChannel.ToString());
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (layerIndex != 0 && animator.GetLayerWeight(layerIndex).Equals(0)) return;
        if (stateMachine == null) return;

        int customCommand = customCurve == null ? 0 : Mathf.FloorToInt(customCurve.Evaluate(animatorStateInfo.normalizedTime - (long)animatorStateInfo.normalizedTime));

        int command;

        if (customCommand != 0) command = customCommand;
        else                    command = Mathf.FloorToInt(animator.GetFloat(commandChannelHash));

        if (previousCommand != command && command > 0 && audioManager != null && collection != null)
        {
            int bank = Mathf.Max(0, Mathf.Min(command - 1, collection.bankCount - 1));

            audioManager.PlayOneShotSound(  collection.audioGroup,
                                            collection[bank],
                                            stateMachine.transform.position,
                                            collection.volume,
                                            collection.spatialBlend,
                                            collection.priority);
        }

        previousCommand = command;
    }

}
