using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MenuController : MonoBehaviour
{
    [Header("=== PANELS ===")]
    public GameObject mainMenuPanel;
    public GameObject startPanel;
    public GameObject optionsPanel;

    [Header("=== HOW TO PLAY ===")]
    public GameObject howToPlayPanel;
    public GameObject[] howToPlayPages;   // halaman 1, 2, ...
    public GameObject htpPrevArrow;
    public GameObject htpNextArrow;

    [Header("=== SLIDERS ===")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("=== AUDIO (Opsional) ===")]
    public AudioMixer audioMixer;

    [Header("=== SCENE NAMES ===")]
    public string gameSceneName = "GameScene";
    public string creditsSceneName = "";

    const string SaveSlotKey = "HasSaveSlot";

    int htpPageIndex;

    void Start()
    {
        ShowMainMenu();

        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        if (bgmSlider != null) bgmSlider.value = savedBGM;
        if (sfxSlider != null) sfxSlider.value = savedSFX;

        ApplyBGMVolume(savedBGM);
        ApplySFXVolume(savedSFX);

        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    // === BUTTON CALLBACKS — MAIN MENU ===

    public void OnStartClick()
    {
        if (startPanel != null)
            startPanel.SetActive(true);
        else if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
    }

    public void OnOptionsClick()
    {
        mainMenuPanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OnCreditsClick()
    {
        if (!string.IsNullOrEmpty(creditsSceneName))
            SceneManager.LoadScene(creditsSceneName);
        else
            Debug.Log("Credits scene belum di-set.");
    }

    public void OnExitClick()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // === BUTTON CALLBACKS — START SUBMENU ===

    public void OnNewGameClick()
    {
        PlayerPrefs.SetInt(SaveSlotKey, 1);
        PlayerPrefs.SetInt("LoadSave", 0);       // mulai dari intro
        PlayerPrefs.SetInt("Chapter1_Stage", 0);
        PlayerPrefs.Save();
        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
        else
            Debug.LogWarning("Game scene name belum di-set di MenuController.");
    }

    public void OnContinueClick()
    {
        if (PlayerPrefs.GetInt(SaveSlotKey, 0) == 0)
        {
            Debug.Log("Belum ada save data — silakan New game.");
            return;
        }
        PlayerPrefs.SetInt("LoadSave", 1);       // lanjut dari save
        PlayerPrefs.Save();
        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
    }

    // === BUTTON CALLBACKS — BACK / OPTIONS ===

    public void OnBackClick()
    {
        // Back dari How to Play -> kembali ke Options.
        if (howToPlayPanel != null && howToPlayPanel.activeSelf)
        {
            howToPlayPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(true);
            return;
        }

        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
            PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
            PlayerPrefs.Save();
        }
        ShowMainMenu();
    }

    // === BUTTON CALLBACKS — HOW TO PLAY ===

    public void OnHowToPlayClick()
    {
        if (howToPlayPanel == null) return;
        if (optionsPanel != null) optionsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
        ShowHtpPage(0);
    }

    public void OnHtpNextClick() => ShowHtpPage(htpPageIndex + 1);
    public void OnHtpPrevClick() => ShowHtpPage(htpPageIndex - 1);

    void ShowHtpPage(int index)
    {
        if (howToPlayPages == null || howToPlayPages.Length == 0) return;
        htpPageIndex = Mathf.Clamp(index, 0, howToPlayPages.Length - 1);
        for (int i = 0; i < howToPlayPages.Length; i++)
            if (howToPlayPages[i] != null)
                howToPlayPages[i].SetActive(i == htpPageIndex);
        if (htpPrevArrow != null) htpPrevArrow.SetActive(htpPageIndex > 0);
        if (htpNextArrow != null) htpNextArrow.SetActive(htpPageIndex < howToPlayPages.Length - 1);
    }

    // === SLIDER ===

    void OnBGMChanged(float value)
    {
        ApplyBGMVolume(value);
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    void OnSFXChanged(float value)
    {
        ApplySFXVolume(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    void ApplyBGMVolume(float value)
    {
        if (audioMixer != null)
        {
            float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat("BGMVolume", dB);
        }
        else
        {
            AudioListener.volume = value;
        }
    }

    void ApplySFXVolume(float value)
    {
        if (audioMixer != null)
        {
            float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat("SFXVolume", dB);
        }
    }

    void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (startPanel != null) startPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }
}
