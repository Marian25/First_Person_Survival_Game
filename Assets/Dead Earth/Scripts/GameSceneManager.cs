using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
