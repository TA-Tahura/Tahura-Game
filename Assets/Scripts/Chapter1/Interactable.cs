using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// Titik interaksi: tanda seru saat aktif, label "Interact" saat pemain dekat, Enter untuk memicu.
public class Interactable : MonoBehaviour
{
    public float radius = 1.6f;
    public GameObject exclamation;
    public GameObject interactLabel;

    [HideInInspector] public Transform player;

    public event Action onInteract;

    bool available;

    public bool Available
    {
        get => available;
        set
        {
            available = value;
            if (exclamation != null) exclamation.SetActive(value);
            if (!value && interactLabel != null) interactLabel.SetActive(false);
        }
    }

    void Start()
    {
        if (exclamation != null) exclamation.SetActive(available);
        if (interactLabel != null) interactLabel.SetActive(false);
    }

    void Update()
    {
        if (!available || player == null) return;

        bool near = Mathf.Abs(player.position.x - transform.position.x) <= radius;
        bool dialogOpen = DialogueManager.Instance != null && DialogueManager.Instance.IsActive;
        bool paused = Time.timeScale < 0.01f;

        if (interactLabel != null) interactLabel.SetActive(near && !dialogOpen && !paused);

        if (near && !dialogOpen && !paused)
        {
            var kb = Keyboard.current;
            if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame))
                onInteract?.Invoke();
        }
    }
}
