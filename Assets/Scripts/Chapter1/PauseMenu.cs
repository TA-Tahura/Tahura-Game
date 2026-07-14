using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Menu pause (Esc): Save, Save and back to menu, Options, Back to menu, Back.
public class PauseMenu : MonoBehaviour
{
    public GameObject panel;
    public GameObject optionsSubPanel;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public TMP_Text feedbackText;
    public string menuSceneName = "MainMenu";

    public Chapter1Flow flow;

    float feedbackTimer;

    void Start()
    {
        panel.SetActive(false);
        if (optionsSubPanel != null) optionsSubPanel.SetActive(false);

        float bgm = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        if (bgmSlider != null)
        {
            bgmSlider.value = bgm;
            bgmSlider.onValueChanged.AddListener(v => { if (bgmSource != null) bgmSource.volume = v; PlayerPrefs.SetFloat("BGMVolume", v); });
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = sfx;
            sfxSlider.onValueChanged.AddListener(v => { if (sfxSource != null) sfxSource.volume = v; PlayerPrefs.SetFloat("SFXVolume", v); });
        }
        if (bgmSource != null) bgmSource.volume = bgm;
        if (sfxSource != null) sfxSource.volume = sfx;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            if (panel.activeSelf) Resume();
            else Pause();
        }

        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.unscaledDeltaTime;
            if (feedbackTimer <= 0f && feedbackText != null) feedbackText.text = "";
        }
    }

    void Pause()
    {
        panel.SetActive(true);
        if (optionsSubPanel != null) optionsSubPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnSave()
    {
        DoSave();
        ShowFeedback("Progress tersimpan!");
    }

    public void OnSaveAndMenu()
    {
        DoSave();
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void OnOptions()
    {
        if (optionsSubPanel != null) optionsSubPanel.SetActive(!optionsSubPanel.activeSelf);
    }

    public void OnBackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    void DoSave()
    {
        PlayerPrefs.SetInt("HasSaveSlot", 1);
        if (flow != null) flow.SaveProgress();
        PlayerPrefs.Save();
    }

    void ShowFeedback(string msg)
    {
        if (feedbackText == null) return;
        feedbackText.text = msg;
        feedbackTimer = 2f;
    }
}
