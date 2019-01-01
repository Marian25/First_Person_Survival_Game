using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionObjective : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (GameSceneManager.GetInstance())
        {
            PlayerInfo playerInfo = GameSceneManager.GetInstance().GetPlayerInfo(col.GetInstanceID());
            if (playerInfo != null)
                playerInfo.characterManager.DoLevelComplete();
        }
    }
}

