using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// Full-screen fade overlay. Lives on a top-sorted canvas.
public class ScreenFader : MonoBehaviour
{
    public Image overlay;

    public static ScreenFader Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        if (overlay != null)
        {
            // Scene starts fully black, Chapter1Flow fades in.
            overlay.color = Color.black;
            overlay.gameObject.SetActive(true);
        }
    }

    public Coroutine FadeTo(Color color, float alpha, float duration, Action onDone = null)
    {
        return StartCoroutine(FadeRoutine(color, alpha, duration, onDone));
    }

    IEnumerator FadeRoutine(Color color, float targetAlpha, float duration, Action onDone)
    {
        overlay.gameObject.SetActive(true);
        Color start = overlay.color;
        Color target = new Color(color.r, color.g, color.b, targetAlpha);
        // Start from the requested color if we are currently invisible.
        if (start.a < 0.01f) start = new Color(color.r, color.g, color.b, 0f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            overlay.color = Color.Lerp(start, target, Mathf.Clamp01(t / duration));
            yield return null;
        }
        overlay.color = target;
        if (targetAlpha <= 0.01f) overlay.gameObject.SetActive(false);
        onDone?.Invoke();
    }
}
