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

    void Awake()
    {
        FindPlayer();
    }

    void Start()
    {
        if (exclamation != null) exclamation.SetActive(available);
        if (interactLabel != null) interactLabel.SetActive(false);
    }

    void FindPlayer()
    {
        if (player != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    void Update()
    {
        if (!PlayerState.CanControl) return;
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        } 
        if (!available) return;

        bool near = Mathf.Abs(player.position.x - transform.position.x) <= radius && Mathf.Abs(player.position.y - transform.position.y) <= 5;
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

    void OnDrawGizmosSelected()
{
    Gizmos.color = Color.yellow;
    // Draws the detection box based on your X radius and Y limit (5)
    Gizmos.DrawWireCube(transform.position, new Vector3(radius * 2f, 10f, 0.1f));
}
}
