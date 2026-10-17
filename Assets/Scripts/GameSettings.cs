using System;
using UnityEngine;

// Player settings that outlive a play session. Master volume drives AudioListener.volume, so any
// sound added later follows it automatically; VolumeChanged is the hook for separate music/SFX mixes.
public static class GameSettings
{
    const string VolumeKey = "masterVolume";

    public static event Action<float> VolumeChanged;

    static float? volume;

    public static float MasterVolume
    {
        get
        {
            if (volume == null) volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
            return volume.Value;
        }
        set
        {
            float v = Mathf.Clamp01(value);
            volume = v;
            AudioListener.volume = v;
            PlayerPrefs.SetFloat(VolumeKey, v);
            VolumeChanged?.Invoke(v);
        }
    }

    // Applies the saved volume when the game starts.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void ApplySaved()
    {
        volume = null;
        AudioListener.volume = MasterVolume;
    }
}
