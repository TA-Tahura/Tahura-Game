using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Full-screen fade overlay. Lives on a top-sorted canvas.
public class ScreenFader : MonoBehaviour
{
    public enum FaderLayer
    {
        WorldOverlay, // Sits above world elements, below black screen
        TopOverlay    // Ultimate top-level black screen
    }

    [SerializeField] public Image overlay;
    [SerializeField] public Image worldOverlay;

    public static ScreenFader Instance { get; private set; }

    private readonly Dictionary<FaderLayer, Coroutine> _runningRoutines = new();

    void Awake()
    {
        Instance = this;
    }

    public Coroutine FadeTo(FaderLayer layer, Color color, float targetAlpha, float duration, Action onDone = null)
    {
        Image targetImage = layer == FaderLayer.WorldOverlay ? worldOverlay : overlay;

        // Stop existing routine on this layer to prevent conflicting lerps
        if (_runningRoutines.TryGetValue(layer, out var currentRoutine) && currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        Coroutine newRoutine = StartCoroutine(FadeRoutine(targetImage, color, targetAlpha, duration, () =>
        {
            _runningRoutines[layer] = null;
            onDone?.Invoke();
        }));

        _runningRoutines[layer] = newRoutine;
        return newRoutine;
    }

    public Coroutine FadeTo(Color color, float alpha, float duration, Action onDone = null)
    {
        return FadeTo(FaderLayer.TopOverlay, color, alpha, duration, onDone);
    }

IEnumerator FadeRoutine(Image img, Color color, float targetAlpha, float duration, Action onDone)
    {
        img.gameObject.SetActive(true);
        Color start = img.color;
        Color target = new Color(color.r, color.g, color.b, targetAlpha);

        if (start.a < 0.01f)
        {
            start = new Color(color.r, color.g, color.b, 0f);
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            img.color = Color.Lerp(start, target, Mathf.Clamp01(t / duration));
            yield return null;
        }

        img.color = target;

        if (targetAlpha <= 0.01f)
        {
            img.gameObject.SetActive(false);
        }

        onDone?.Invoke();
    }
}
