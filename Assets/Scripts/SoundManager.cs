using System.Collections.Generic;
using UnityEngine;

// Plays the game's sounds from Assets/Resources/Audio/<id> (list in Docs/sound-prompts.md).
// One-shots go through a small pool so each can have its own pitch; loops and music get their own sources.
// UI sounds and music keep playing while the game is paused (AudioListener.pause); everything follows the
// master volume, which GameSettings applies to AudioListener.volume.
public class SoundManager : MonoBehaviour
{
    const int PoolSize = 10;
    const float TrimThreshold = 0.02f;   // leading silence below this is cut from one-shots
    public const float EffectsMix = 0.55f;      // overall level of effects under the master volume (music has its own)

    static SoundManager instance;
    readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    readonly AudioSource[] pool = new AudioSource[PoolSize];
    int next;
    AudioSource musicA, musicB;
    float musicVolume = 0.18f;
    string musicId;
    float musicFade = 1f;   // 0..1 progress of the current cross-fade

    static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("SoundManager");
                instance = go.AddComponent<SoundManager>();
            }
            return instance;
        }
    }

    void Awake()
    {
        for (int i = 0; i < PoolSize; i++) pool[i] = NewSource(false);
        musicA = NewSource(true);
        musicB = NewSource(true);
        musicA.loop = musicB.loop = true;
    }

    AudioSource NewSource(bool ignorePause)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = ignorePause;
        return source;
    }

    // Fire-and-forget sound. ui = keeps playing while paused. delay is in real seconds.
    public static void Play(string id, float volume = 1f, float pitch = 1f, bool ui = false, float delay = 0f)
    {
        var clip = Instance.Clip(id, true);
        if (clip == null) return;
        var source = Instance.pool[Instance.next];
        Instance.next = (Instance.next + 1) % PoolSize;
        source.Stop();
        source.clip = clip;
        source.volume = volume * EffectsMix;
        source.pitch = pitch;
        source.ignoreListenerPause = ui;
        if (delay > 0f) source.PlayDelayed(delay);
        else source.Play();
    }

    // A dedicated looping source the caller drives (volume/pitch); starts silent.
    public static AudioSource Loop(string id)
    {
        var clip = Instance.Clip(id, false);
        var source = Instance.NewSource(false);
        source.clip = clip;
        source.loop = true;
        source.volume = 0f;
        if (clip != null) source.Play();
        return source;
    }

    // Cross-fades to another music loop (no-op if it's already playing).
    public static void PlayMusic(string id, float volume = 0.18f)
    {
        var m = Instance;
        if (m.musicId == id) return;
        m.musicId = id;
        m.musicVolume = volume;
        (m.musicA, m.musicB) = (m.musicB, m.musicA);   // A is the new track, B fades out
        m.musicA.clip = m.Clip(id, false);
        m.musicA.volume = 0f;
        if (m.musicA.clip != null) m.musicA.Play();
        m.musicFade = 0f;
    }

    void Update()
    {
        if (musicFade >= 1f) return;
        musicFade = Mathf.MoveTowards(musicFade, 1f, Time.unscaledDeltaTime / 1.5f);
        musicA.volume = musicVolume * musicFade;
        musicB.volume = musicVolume * (1f - musicFade);
        if (musicFade >= 1f) musicB.Stop();
    }

    AudioClip Clip(string id, bool trim)
    {
        if (clips.TryGetValue(id, out var clip)) return clip;
        clip = Resources.Load<AudioClip>("Audio/" + id);
        if (clip == null) Debug.LogWarning($"Sound missing: {id}");
        else if (trim) clip = TrimStart(clip);
        clips[id] = clip;
        return clip;
    }

    // MP3s from the generator start with a short silence (encoder delay, sometimes more); cut it so
    // clicks land on the click. Keeps 3 ms before the first audible sample.
    static AudioClip TrimStart(AudioClip clip)
    {
        if (clip.loadType != AudioClipLoadType.DecompressOnLoad) return clip;
        var data = new float[clip.samples * clip.channels];
        if (!clip.GetData(data, 0)) return clip;
        int first = 0;
        while (first < data.Length && Mathf.Abs(data[first]) < TrimThreshold) first++;
        int startFrame = Mathf.Max(0, first / clip.channels - clip.frequency * 3 / 1000);
        if (startFrame < clip.frequency / 100) return clip;   // under 10 ms: leave it
        int frames = clip.samples - startFrame;
        var trimmed = new float[frames * clip.channels];
        System.Array.Copy(data, startFrame * clip.channels, trimmed, 0, trimmed.Length);
        var result = AudioClip.Create(clip.name, frames, clip.channels, clip.frequency, false);
        result.SetData(trimmed, 0);
        return result;
    }
}
