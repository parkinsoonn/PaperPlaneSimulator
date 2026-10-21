using System.IO;
using UnityEditor;
using UnityEngine;

// Generates the sounds that weren't made with ElevenLabs (see Docs/sound-prompts.md) as WAV files in
// Assets/Resources/Audio: simple synthesis from noise, sines and envelopes, deterministic so re-running gives
// the same files. Menu: Tools > Kağıt Uçak > Generate Synth Sounds.
public static class SynthSounds
{
    const int Rate = 44100;
    const string Folder = "Assets/Resources/Audio";

    [MenuItem("Tools/Kağıt Uçak/Generate Synth Sounds")]
    public static void GenerateAll()
    {
        Directory.CreateDirectory(Folder);
        Write("coin_tick", CoinTick());
        Write("result_stamp", Stamp());
        Write("result_card", CardDrop());
        Write("crash_crumple", Crumple());
        Write("land_slide", Slide());
        Write("car_horn", Horn());
        AssetDatabase.Refresh();
    }

    // Tiny bright coin tick for counting gold.
    static float[] CoinTick()
    {
        var s = Buffer(0.09f);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Rate;
            float env = Mathf.Exp(-t / 0.014f);
            s[i] = env * (0.6f * Sin(2637f, t) + 0.35f * Sin(3951f, t) + 0.15f * Sin(5274f, t));
        }
        return Normalize(s, 0.7f);
    }

    // Heavy rubber stamp: low thump with a falling pitch, a muffled hit and a little ink squish.
    static float[] Stamp()
    {
        var rng = new System.Random(2);
        var s = Buffer(0.42f);
        float lp = 0f, phase = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Rate;
            float f = Mathf.Lerp(130f, 50f, Mathf.Clamp01(t / 0.12f));
            phase += f / Rate;
            float thump = Mathf.Sin(phase * Mathf.PI * 2f) * Mathf.Exp(-t / 0.09f);
            lp += 0.08f * (Noise(rng) - lp);
            float hit = lp * 3f * Mathf.Exp(-t / 0.03f);
            float squish = t > 0.04f && t < 0.2f ? Noise(rng) * 0.08f * Mathf.Sin((t - 0.04f) / 0.16f * Mathf.PI) : 0f;
            s[i] = thump + hit + squish;
        }
        return Normalize(s, 0.95f);
    }

    // Paper card slapped onto a table, then a quick strip of tape pressed down.
    static float[] CardDrop()
    {
        var rng = new System.Random(3);
        var s = Buffer(0.6f);
        float lp = 0f, hpPrev = 0f, hp = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Rate;
            float n = Noise(rng);
            lp += 0.25f * (n - lp);                       // body of the slap
            hp = 0.85f * (hp + n - hpPrev);               // crisp paper edge
            hpPrev = n;
            float slap = (lp * 1.2f + hp * 0.5f) * Mathf.Exp(-t / 0.035f);
            float tapeT = t - 0.26f;
            float tape = tapeT > 0f && tapeT < 0.22f
                ? hp * 0.35f * Mathf.Sin(tapeT / 0.22f * Mathf.PI) * (0.6f + 0.4f * Mathf.Sin(tapeT * 2f * Mathf.PI * 70f))
                : 0f;
            s[i] = slap + tape;
        }
        return Normalize(s, 0.9f);
    }

    // Paper plane hitting a car: dull thud plus a burst of crackling crumple that thins out.
    static float[] Crumple()
    {
        var rng = new System.Random(4);
        var s = Buffer(0.7f);
        float phase = 0f, hpPrev = 0f, hp = 0f, crackle = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Rate;
            float f = Mathf.Lerp(95f, 45f, Mathf.Clamp01(t / 0.1f));
            phase += f / Rate;
            float thud = Mathf.Sin(phase * Mathf.PI * 2f) * Mathf.Exp(-t / 0.07f) * 0.9f;
            float density = 0.012f * Mathf.Exp(-t / 0.2f);   // chance of a crackle per sample
            if (rng.NextDouble() < density) crackle = (float)(rng.NextDouble() * 2 - 1);
            crackle *= 0.93f;
            float n = Noise(rng);
            hp = 0.8f * (hp + n - hpPrev);
            hpPrev = n;
            float rustle = hp * 0.15f * Mathf.Exp(-t / 0.25f);
            s[i] = thud + crackle * 0.8f + rustle;
        }
        return Normalize(s, 0.95f);
    }

    // Paper sliding to a stop on asphalt: rough, filtered noise that fades out.
    static float[] Slide()
    {
        var rng = new System.Random(5);
        var s = Buffer(1f);
        float lp = 0f, lp2 = 0f, rough = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Rate;
            float n = Noise(rng);
            lp += 0.35f * (n - lp);
            lp2 += 0.05f * (lp - lp2);
            float band = lp - lp2;                         // mid band "scrape"
            if (i % 200 == 0) rough = 0.6f + 0.4f * (float)rng.NextDouble();
            float env = Mathf.Clamp01(t / 0.03f) * Mathf.Pow(1f - t, 2f);
            s[i] = band * rough * env;
        }
        return Normalize(s, 0.6f);
    }

    // Friendly little car horn: two slightly detuned buzzy tones, softened by distance.
    static float[] Horn()
    {
        var s = Buffer(0.5f);
        float lp = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Rate;
            float env = Mathf.Clamp01(t / 0.015f) * Mathf.Clamp01((0.5f - t) / 0.08f);
            float tone = Square(415f, t) + Square(523f, t) * 0.8f;
            lp += 0.12f * (tone - lp);
            s[i] = lp * env;
        }
        return Normalize(s, 0.5f);
    }

    static float[] Buffer(float seconds) => new float[(int)(seconds * Rate)];
    static float Noise(System.Random rng) => (float)(rng.NextDouble() * 2 - 1);
    static float Sin(float f, float t) => Mathf.Sin(2f * Mathf.PI * f * t);
    static float Square(float f, float t) => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * f * t)) * 0.5f;

    static float[] Normalize(float[] s, float peak)
    {
        float max = 0f;
        foreach (float v in s) max = Mathf.Max(max, Mathf.Abs(v));
        if (max > 0f) for (int i = 0; i < s.Length; i++) s[i] *= peak / max;
        return s;
    }

    // 16-bit PCM WAV.
    static void Write(string id, float[] samples, int channels = 1)
    {
        using (var w = new BinaryWriter(File.Create(Path.Combine(Folder, id + ".wav"))))
        {
            int bytes = samples.Length * 2;
            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(36 + bytes);
            w.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
            w.Write(16);
            w.Write((short)1);
            w.Write((short)channels);
            w.Write(Rate);
            w.Write(Rate * channels * 2);
            w.Write((short)(channels * 2));
            w.Write((short)16);
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(bytes);
            foreach (float v in samples) w.Write((short)Mathf.RoundToInt(Mathf.Clamp(v, -1f, 1f) * 32767f));
        }
    }
}
