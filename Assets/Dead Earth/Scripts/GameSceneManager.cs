using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    public Collider collider = null;
    public CharacterManager characterManager = null;
    public Camera camera = null;
    public CapsuleCollider meleeTrigger = null; 
}

public class GameSceneManager : MonoBehaviour {

    [SerializeField] private ParticleSystem _bloodParticles = null;

    private static GameSceneManager instance = null;

    public static GameSceneManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<GameSceneManager>();
        }
        return instance;
    }

    private Dictionary<int, AIStateMachine> stateMachines = new Dictionary<int, AIStateMachine>();
    private Dictionary<int, PlayerInfo> playerInfos = new Dictionary<int, PlayerInfo>();
    private Dictionary<int, InteractiveItem> _interactiveItems = new Dictionary<int, InteractiveItem>();

    public ParticleSystem bloodParticles { get { return _bloodParticles; } }

    public void RegisterAIStateMachine(int key, AIStateMachine stateMachine)
    {
        if (!stateMachines.ContainsKey(key))
        {
            stateMachines[key] = stateMachine;
        }
    }

    public AIStateMachine GetAIStateMachine(int key)
    {
        AIStateMachine stateMachine = null;

        if (stateMachines.TryGetValue(key, out stateMachine))
        {
            return stateMachine;
        }
        return null;
    }

    public void RegisterPlayerInfo(int key, PlayerInfo playerInfo)
    {
        if (!playerInfos.ContainsKey(key))
        {
            playerInfos[key] = playerInfo;
        }
    }

    public PlayerInfo GetPlayerInfo(int key)
    {
        PlayerInfo playerInfo = null;

        if (playerInfos.TryGetValue(key, out playerInfo))
        {
            return playerInfo;
        }
        return null;
    }

    public void RegisterInteractiveItem(int key, InteractiveItem script)
    {
        if (!_interactiveItems.ContainsKey(key))
        {
            _interactiveItems[key] = script;
        }
    }

    public InteractiveItem GetInteractiveItem(int key)
    {
        InteractiveItem item = null;
        _interactiveItems.TryGetValue(key, out item);
        return item;
    }

}
