using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea] public string text;
    public Sprite portrait;
    public bool portraitOnRight;

    public DialogueLine(string speaker, string text, Sprite portrait = null, bool portraitOnRight = false)
    {
        this.speaker = speaker;
        this.text = text;
        this.portrait = portrait;
        this.portraitOnRight = portraitOnRight;
    }
}

/// Dialog ala visual novel: kotak dialog + nama + portrait, maju dengan Space.
public class DialogueManager : MonoBehaviour
{
    [Header("UI refs")]
    public GameObject panelRoot;
    public TMP_Text nameText;
    public TMP_Text bodyText;
    public Image portraitLeft;
    public Image portraitRight;
    public GameObject nextHint;

    public bool IsActive { get; private set; }

    public static DialogueManager Instance { get; private set; }

    const float CharsPerSecond = 45f;

    Coroutine running;

    void Awake()
    {
        Instance = this;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void Show(IList<DialogueLine> lines, Action onDone)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(RunLines(lines, onDone));
    }

    IEnumerator RunLines(IList<DialogueLine> lines, Action onDone)
    {
        IsActive = true;
        panelRoot.SetActive(true);

        foreach (var line in lines)
        {
            nameText.text = line.speaker;
            portraitLeft.gameObject.SetActive(line.portrait != null && !line.portraitOnRight);
            portraitRight.gameObject.SetActive(line.portrait != null && line.portraitOnRight);
            if (line.portrait != null)
            {
                var img = line.portraitOnRight ? portraitRight : portraitLeft;
                img.sprite = line.portrait;
                // Samakan tinggi portrait, lebar mengikuti aspek sprite (pivot bawah).
                const float portraitHeight = 900f;
                float aspect = line.portrait.rect.width / line.portrait.rect.height;
                img.rectTransform.sizeDelta = new Vector2(portraitHeight * aspect, portraitHeight);
            }

            // Ketik per karakter, Space untuk skip lalu lanjut.
            bodyText.text = "";
            if (nextHint != null) nextHint.SetActive(false);
            yield return null; // jangan telan input Space dari frame sebelumnya

            float shown = 0f;
            while (shown < line.text.Length)
            {
                if (AdvancePressed()) { shown = line.text.Length; break; }
                shown += CharsPerSecond * Time.unscaledDeltaTime;
                bodyText.text = line.text.Substring(0, Mathf.Min(line.text.Length, Mathf.FloorToInt(shown)));
                yield return null;
            }
            bodyText.text = line.text;
            if (nextHint != null) nextHint.SetActive(true);

            yield return null;
            while (!AdvancePressed()) yield return null;
        }

        panelRoot.SetActive(false);
        IsActive = false;
        running = null;
        onDone?.Invoke();
    }

    static bool AdvancePressed()
    {
        var kb = Keyboard.current;
        if (kb == null) return false;
        return kb.spaceKey.wasPressedThisFrame;
    }
}
