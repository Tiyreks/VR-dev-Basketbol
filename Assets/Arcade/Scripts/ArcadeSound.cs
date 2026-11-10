using System.Collections.Generic;
using UnityEngine;

// Tiny synthesized sounds so the arcade has audio without any audio files.
public static class ArcadeSound
{
    public enum Kind { Start, Score, End, Fail, Ticket, Bounce, Swish, Tick, Buzzer, Quack, Clang }

    const int SampleRate = 44100;

    private static readonly Dictionary<Kind, AudioClip> clips = new Dictionary<Kind, AudioClip>();

    public static void Play(Kind kind, Vector3 position, float volume = 0.6f)
    {
        if (!clips.TryGetValue(kind, out AudioClip clip))
        {
            clip = Create(kind);
            clips[kind] = clip;
        }

        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    private static AudioClip Create(Kind kind)
    {
        float[] data;

        switch (kind)
        {
            case Kind.Start: data = Notes(new[] { 523f, 659f, 784f }, 0.08f); break;
            case Kind.Score: data = Notes(new[] { 880f, 1175f }, 0.06f); break;
            case Kind.End: data = Notes(new[] { 784f, 659f, 523f, 1046f }, 0.12f); break;
            case Kind.Fail: data = Notes(new[] { 220f, 180f }, 0.15f); break;
            case Kind.Bounce: data = Bounce(); break;
            case Kind.Swish: data = Swish(); break;
            case Kind.Tick: data = Notes(new[] { 1400f }, 0.04f); break;
            case Kind.Buzzer: data = Buzzer(); break;
            case Kind.Quack: data = Quack(); break;
            case Kind.Clang: data = Clang(); break;
            default: data = Notes(new[] { 1046f, 1318f, 1568f }, 0.07f); break;
        }

        AudioClip clip = AudioClip.Create("Arcade_" + kind, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Square-wave melody for a retro arcade sound.
    private static float[] Notes(float[] notes, float noteLength)
    {
        int samplesPerNote = (int)(SampleRate * noteLength);
        float[] data = new float[samplesPerNote * notes.Length];

        for (int n = 0; n < notes.Length; n++)
        {
            for (int i = 0; i < samplesPerNote; i++)
            {
                float t = i / (float)SampleRate;
                float envelope = 1f - i / (float)samplesPerNote;
                float wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * notes[n] * t)) * 0.25f;
                data[n * samplesPerNote + i] = wave * envelope;
            }
        }

        return data;
    }

    // Low thump with a falling pitch, like a ball hitting the floor.
    private static float[] Bounce()
    {
        int length = (int)(SampleRate * 0.15f);
        float[] data = new float[length];
        float phase = 0f;

        for (int i = 0; i < length; i++)
        {
            float k = i / (float)length;
            float frequency = Mathf.Lerp(160f, 60f, k);
            phase += 2f * Mathf.PI * frequency / SampleRate;
            float envelope = Mathf.Exp(-k * 6f);
            data[i] = Mathf.Sin(phase) * envelope * 0.9f;
        }

        return data;
    }

    // Soft noise burst that swells and fades, like the ball going through the net.
    private static float[] Swish()
    {
        int length = (int)(SampleRate * 0.35f);
        float[] data = new float[length];
        var random = new System.Random(3);
        float smoothed = 0f;

        for (int i = 0; i < length; i++)
        {
            float k = i / (float)length;
            float noise = (float)(random.NextDouble() * 2.0 - 1.0);
            // Simple low-pass filter to take the harsh edge off the noise.
            smoothed += (noise - smoothed) * 0.35f;
            float envelope = Mathf.Sin(k * Mathf.PI) * (1f - k);
            data[i] = smoothed * envelope * 0.8f;
        }

        return data;
    }

    // Nasal, falling "vak" for a hit duck.
    private static float[] Quack()
    {
        int length = (int)(SampleRate * 0.25f);
        float[] data = new float[length];
        float phase = 0f;

        for (int i = 0; i < length; i++)
        {
            float k = i / (float)length;
            float frequency = Mathf.Lerp(520f, 300f, k) + Mathf.Sin(k * 60f) * 25f;
            phase += frequency / SampleRate;
            // Sawtooth with a few harmonics for the buzzy duck voice.
            float wave = (Mathf.Repeat(phase, 1f) * 2f - 1f) * 0.6f + Mathf.Sin(phase * 2f * Mathf.PI * 3f) * 0.2f;
            float envelope = Mathf.Min(1f, k * 20f) * (1f - k);
            data[i] = wave * envelope * 0.35f;
        }

        return data;
    }

    // Metallic ring for a can that gets hit.
    private static float[] Clang()
    {
        int length = (int)(SampleRate * 0.3f);
        float[] data = new float[length];
        float[] partials = { 1200f, 1870f, 2650f, 3410f };

        for (int i = 0; i < length; i++)
        {
            float t = i / (float)SampleRate;
            float k = i / (float)length;
            float wave = 0f;
            for (int p = 0; p < partials.Length; p++)
                wave += Mathf.Sin(2f * Mathf.PI * partials[p] * t) / (p + 1);
            data[i] = wave * Mathf.Exp(-k * 7f) * 0.25f;
        }

        return data;
    }

    // Harsh end-of-game buzzer.
    private static float[] Buzzer()
    {
        int length = (int)(SampleRate * 0.8f);
        float[] data = new float[length];

        for (int i = 0; i < length; i++)
        {
            float t = i / (float)SampleRate;
            float k = i / (float)length;
            float wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 140f * t)) + 0.5f * Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 147f * t));
            float envelope = k < 0.9f ? 1f : (1f - k) * 10f;
            data[i] = wave * envelope * 0.2f;
        }

        return data;
    }
}
