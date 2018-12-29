using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class TrackInfo
{
    public string name = string.Empty;
    public AudioMixerGroup group = null;
    public IEnumerator trackFader = null;
}

public class AudioManager : MonoBehaviour {

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

    Dictionary<string, TrackInfo> tracks = new Dictionary<string, TrackInfo>();

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

}
