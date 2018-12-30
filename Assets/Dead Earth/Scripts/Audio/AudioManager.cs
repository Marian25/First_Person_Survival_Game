using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class TrackInfo
{
    public string name = string.Empty;
    public AudioMixerGroup group = null;
    public IEnumerator trackFader = null;
}

public class AudioPoolItem
{
    public GameObject gameObject = null;
    public Transform transform = null;
    public AudioSource audioSource = null;
    public float unimportance = float.MaxValue;
    public bool playing = false;
    public IEnumerator coroutine = null;
    public ulong ID = 0;
}

public class AudioManager : MonoBehaviour
{

    private static AudioManager _instance = null;
    public static AudioManager instance
    {
        get
        {
            if (_instance == null)
                _instance = (AudioManager)FindObjectOfType(typeof(AudioManager));
            return _instance;
        }
    }

    [SerializeField] AudioMixer mixer = null;
    [SerializeField] int maxSounds = 10;

    Dictionary<string, TrackInfo> tracks = new Dictionary<string, TrackInfo>();
    List<AudioPoolItem> pool = new List<AudioPoolItem>();
    Dictionary<ulong, AudioPoolItem> activePool = new Dictionary<ulong, AudioPoolItem>();
    ulong idGiver = 0;
    Transform listenerPos = null;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (!mixer) return;

        AudioMixerGroup[] groups = mixer.FindMatchingGroups(string.Empty);

        foreach (AudioMixerGroup group in groups)
        {
            TrackInfo track = new TrackInfo();
            track.name = group.name;
            track.group = group;
            track.trackFader = null;
            tracks[group.name] = track;
        }

        // Generate pool
        for (int i = 0; i < maxSounds; i++)
        {
            GameObject go = new GameObject("Pool Item");
            AudioSource audioSource = go.AddComponent<AudioSource>();
            go.transform.parent = transform;

            AudioPoolItem poolItem = new AudioPoolItem();
            poolItem.gameObject = go;
            poolItem.audioSource = audioSource;
            poolItem.transform = go.transform;
            poolItem.playing = false;
            go.SetActive(false);
            pool.Add(poolItem);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        listenerPos = FindObjectOfType<AudioListener>().transform;
    }

    private void Update()
    {

    }

    public float GetTrackVolume(string track)
    {
        TrackInfo trackInfo;
        if (tracks.TryGetValue(track, out trackInfo))
        {
            float volume;
            mixer.GetFloat(track, out volume);
            return volume;
        }
        return float.MinValue;
    }

    public AudioMixerGroup GetAudioGroupFromTrackName(string name)
    {
        TrackInfo trackInfo;
        if (tracks.TryGetValue(name, out trackInfo))
        {
            return trackInfo.group;
        }
        return null;
    }

    public void SetTrackVolume(string track, float volume, float fadeTime = 0)
    {
        if (!mixer) return;

        TrackInfo trackInfo;
        if (tracks.TryGetValue(track, out trackInfo))
        {
            if (trackInfo.trackFader != null)
                StopCoroutine(trackInfo.trackFader);

            if (fadeTime == 0)
                mixer.SetFloat(track, volume);
            else
            {
                trackInfo.trackFader = SetTrackVolumeInternal(track, volume, fadeTime);
                StartCoroutine(trackInfo.trackFader);
            }
        }
    }

    protected IEnumerator SetTrackVolumeInternal(string track, float volume, float fadeTime)
    {
        float startVolume = 0;
        float timer = 0;

        mixer.GetFloat(track, out startVolume);

        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            mixer.SetFloat(track, Mathf.Lerp(startVolume, volume, timer / fadeTime));
            yield return null;
        }

        mixer.SetFloat(track, volume);
    }

    protected ulong ConfigurePoolObject(int poolIndex, string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, float unimportance)
    {
        if (poolIndex < 0 || poolIndex >= pool.Count) return 0;

        AudioPoolItem poolItem = pool[poolIndex];
        idGiver++;

        AudioSource source = poolItem.audioSource;
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = spatialBlend;

        source.outputAudioMixerGroup = tracks[track].group;

        source.transform.position = position;

        poolItem.playing = true;
        poolItem.unimportance = unimportance;
        poolItem.ID = idGiver;
        poolItem.gameObject.SetActive(true);
        source.Play();
        poolItem.coroutine = StopSoundDelayed(idGiver, source.clip.length);
        StartCoroutine(poolItem.coroutine);

        activePool[idGiver] = poolItem;

        return idGiver;
    }

    protected IEnumerator StopSoundDelayed(ulong id, float duration)
    {
        yield return new WaitForSeconds(duration);

        AudioPoolItem activeSound;

        if (activePool.TryGetValue(id, out activeSound))
        {
            activeSound.audioSource.Stop();
            activeSound.audioSource.clip = null;
            activeSound.gameObject.SetActive(false);
            activePool.Remove(id);

            activeSound.playing = false;
        }
    }

    public void StopOneShotSound(ulong id)
    {
        AudioPoolItem activeSound;

        if (activePool.TryGetValue(id, out activeSound))
        {
            StopCoroutine(activeSound.coroutine);

            activeSound.audioSource.Stop();
            activeSound.audioSource.clip = null;
            activeSound.gameObject.SetActive(false);

            activePool.Remove(id);

            activeSound.playing = false;
        }
    }

    public ulong PlayOneShotSound(string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, int priority = 128)
    {
        if (!tracks.ContainsKey(track) || clip == null || volume == 0) return 0;

        float unimportance = (listenerPos.position - position).sqrMagnitude / Mathf.Max(1, priority);

        int leastImportantIndex = -1;
        float leastImportantValue = float.MaxValue;

        for (int i = 0; i < pool.Count; i++)
        {
            AudioPoolItem poolItem = pool[i];

            if (!poolItem.playing)
            {
                return ConfigurePoolObject(i, track, clip, position, volume, spatialBlend, unimportance);
            }
            else if (poolItem.unimportance > leastImportantValue)
            {
                leastImportantValue = poolItem.unimportance;
                leastImportantIndex = i;
            }
        }

        if (leastImportantValue > unimportance)
        {
            return ConfigurePoolObject(leastImportantIndex, track, clip, position, volume, spatialBlend, unimportance);
        }
        return 0;
    }

    public IEnumerator PlayOneShotSoundDelayed(string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, float duration, int priority = 128)
    {
        yield return new WaitForSeconds(duration);
        PlayOneShotSound(track, clip, position, volume, spatialBlend, priority);
    }

}
