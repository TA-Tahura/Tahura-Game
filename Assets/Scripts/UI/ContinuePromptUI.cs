using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContinuePromptUI : MonoBehaviour
{
    public static ContinuePromptUI Instance { get; private set; }

    [SerializeField] private Button continueButton;

    private CanvasGroup canvasGroup;
    private Action onContinueCallback;

    private void Awake()
    {
        if (Instance != null && Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
        continueButton.onClick.AddListener(HandleContinueClicked);
        HideImmediate();
    }

    /// <summary>
    /// Displays the prompt with dynamic text and a specific action to run on click.
    /// </summary>
    public void Show(Action onContinue)
    {
        onContinueCallback = onContinue;

        // Show and enable interactions
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HandleContinueClicked()
    {
        HideImmediate();
        
        // Execute whatever action was assigned
        onContinueCallback?.Invoke();
        onContinueCallback = null;
    }

    public void HideImmediate()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}