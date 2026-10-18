using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Ease { Linear, OutCubic, OutBack, InQuad, InOutSine }

// Tiny tween runner for the HUD's animations. Runs on unscaled time so the pause menu still animates
// while the game is frozen. Tweens die with their target; Kill(target) stops a target's tweens.
public class UiAnim : MonoBehaviour
{
    class Tween
    {
        public UnityEngine.Object target;
        public float delay, duration, time;
        public Ease ease;
        public bool loop;           // ping-pong forever
        public Action<float> apply;
        public Action done;
    }

    static UiAnim runner;
    readonly List<Tween> tweens = new List<Tween>();

    static UiAnim Runner
    {
        get
        {
            if (runner == null) runner = new GameObject("UiAnim").AddComponent<UiAnim>();
            return runner;
        }
    }

    // Generic tween: apply(eased 0..1) every frame. By default it starts with apply(0) right away so a
    // delayed element already sits at its start pose; applyNow = false leaves it untouched until the delay ends.
    public static void Play(UnityEngine.Object target, float duration, Action<float> apply, float delay = 0f,
        Ease ease = Ease.OutCubic, bool loop = false, Action done = null, bool applyNow = true)
    {
        if (applyNow) apply(0f);
        Runner.tweens.Add(new Tween { target = target, duration = Mathf.Max(0.0001f, duration), delay = delay, ease = ease, loop = loop, apply = apply, done = done });
    }

    public static void Kill(UnityEngine.Object target)
    {
        if (runner != null) runner.tweens.RemoveAll(t => t.target == target);
    }

    // Uniform scale from -> 1 (or to). Buttons scale through their UiHover so hover keeps working.
    public static void ScaleIn(Transform t, float from, float duration, float delay = 0f, Ease ease = Ease.OutBack, float to = 1f,
        bool applyNow = true)
    {
        var hover = t.GetComponent<UiHover>();
        Play(t, duration, k =>
        {
            float s = Mathf.LerpUnclamped(from, to, k);
            if (hover != null) hover.appear = s;
            else t.localScale = new Vector3(s, s, 1f);
        }, delay, ease, applyNow: applyNow);
    }

    // Slides in from (current position + offset) to the current position.
    public static void SlideIn(RectTransform rt, Vector2 offset, float duration, float delay = 0f, Ease ease = Ease.OutCubic)
    {
        Vector2 end = rt.anchoredPosition;
        Play(rt, duration, k => rt.anchoredPosition = Vector2.LerpUnclamped(end + offset, end, k), delay, ease);
    }

    public static void RotateIn(RectTransform rt, float fromZ, float duration, float delay = 0f, Ease ease = Ease.OutBack)
    {
        float end = rt.localEulerAngles.z;
        if (end > 180f) end -= 360f;
        Play(rt, duration, k => rt.localEulerAngles = new Vector3(0f, 0f, Mathf.LerpUnclamped(fromZ, end, k)), delay, ease);
    }

    public static void Fade(CanvasGroup group, float from, float to, float duration, float delay = 0f, Ease ease = Ease.OutCubic)
    {
        Play(group, duration, k => group.alpha = Mathf.LerpUnclamped(from, to, k), delay, ease);
    }

    // Endless gentle drift around the current pose: offset/angle at the far end, one way per halfPeriod.
    public static void Loop(RectTransform rt, Vector2 offset, float angle, float halfPeriod)
    {
        Vector2 basePos = rt.anchoredPosition;
        float baseAngle = rt.localEulerAngles.z;
        Play(rt, halfPeriod, k =>
        {
            float s = k * 2f - 1f;   // -1..1
            rt.anchoredPosition = basePos + offset * s;
            rt.localEulerAngles = new Vector3(0f, 0f, baseAngle + angle * s);
        }, 0f, Ease.InOutSine, loop: true);
    }

    // Idle "breathing" scale on a button (1 -> amount and back).
    public static void Pulse(UiHover hover, float amount, float halfPeriod, float delay = 0f)
    {
        Play(hover, halfPeriod, k => hover.pulse = Mathf.Lerp(1f, amount, k), delay, Ease.InOutSine, loop: true);
    }

    // Quick 1 -> amount -> 1 pop.
    public static void Punch(Transform t, float amount, float duration)
    {
        Play(t, duration, k =>
        {
            float s = 1f + (amount - 1f) * Mathf.Sin(k * Mathf.PI);
            t.localScale = new Vector3(s, s, 1f);
        }, 0f, Ease.Linear);
    }

    // Side-to-side shake around wherever the element is when the shake starts.
    public static void Shake(RectTransform rt, float amplitude, float duration, int times, float delay = 0f)
    {
        Vector2? basePos = null;
        Play(rt, duration, k =>
        {
            if (basePos == null) basePos = rt.anchoredPosition;
            rt.anchoredPosition = basePos.Value + new Vector2(Mathf.Sin(k * times * Mathf.PI * 2f) * amplitude * (1f - k), 0f);
        }, delay, Ease.Linear, applyNow: false);
    }

    // Counts a number up (or down) into a text.
    public static void Count(Text text, float from, float to, float duration, Func<int, string> format, float delay = 0f, Action done = null)
    {
        Play(text, duration, k => text.text = format(Mathf.RoundToInt(Mathf.LerpUnclamped(from, to, k))), delay, Ease.OutCubic, done: done);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        for (int i = tweens.Count - 1; i >= 0; i--)
        {
            if (i >= tweens.Count) continue;   // a callback killed tweens
            var tw = tweens[i];
            if (tw.target == null) { tweens.RemoveAt(i); continue; }
            if (tw.delay > 0f) { tw.delay -= dt; continue; }
            tw.time += dt;
            float p = tw.time / tw.duration;
            if (tw.loop)
            {
                tw.apply(Evaluate(tw.ease, Mathf.PingPong(p, 1f)));
                continue;
            }
            tw.apply(Evaluate(tw.ease, Mathf.Clamp01(p)));
            if (p >= 1f)
            {
                tweens.Remove(tw);
                tw.done?.Invoke();
            }
        }
    }

    static float Evaluate(Ease ease, float t)
    {
        switch (ease)
        {
            case Ease.OutCubic: return 1f - Mathf.Pow(1f - t, 3f);
            case Ease.OutBack:
                const float c1 = 1.70158f, c3 = c1 + 1f;
                return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            case Ease.InQuad: return t * t;
            case Ease.InOutSine: return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
            default: return t;
        }
    }
}
