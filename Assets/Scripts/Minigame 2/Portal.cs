using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [SerializeField] private GameObject interactPoint;
    [SerializeField] private GameObject enterPoint;

    private Interactable _interactablePoint;
    private Interactable _enterInteractable;

    void Awake()
    {
        if (interactPoint != null)
            _interactablePoint = interactPoint.GetComponent<Interactable>();

        if (enterPoint != null)
            _enterInteractable = enterPoint.GetComponent<Interactable>();

        _interactablePoint.Available = true;
        _enterInteractable.Available = true;
        interactPoint.SetActive(true);
        enterPoint.SetActive(false);
    }

    void OnEnable()
    {
        if (_interactablePoint != null)
            _interactablePoint.onInteract += TriggerDialogue;

        if (_enterInteractable != null)
            _enterInteractable.onInteract += EnterPortal;
    }

    void OnDisable()
    {
        if (_interactablePoint != null)
            _interactablePoint.onInteract -= TriggerDialogue;

        if (_enterInteractable != null)
            _enterInteractable.onInteract -= EnterPortal;
    }

    void TriggerDialogue()
    {
        Debug.Log("This is a dialogue");
        interactPoint.SetActive(false);
        enterPoint.SetActive(true);
    }

    void EnterPortal()
    {
        StartCoroutine(EnterPortalCutscene());
    }

    IEnumerator EnterPortalCutscene()
    {
        PlayerState.CanControl = false;
        yield return ScreenFader.Instance?.FadeTo(ScreenFader.FaderLayer.WorldOverlay, Color.white, 1f, 1f);
        yield return new WaitForSeconds(1f);
        yield return JournalUI.Instance?.UnlockNextChapterSequence();
        yield return new WaitForSeconds(1f);
        ContinuePromptUI.Instance.Show(
            onContinue: () => SceneManager.LoadScene("MainMenu")
        );
    }
}