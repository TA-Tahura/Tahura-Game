using UnityEngine;

public class TriggerDialoguePrompt : MonoBehaviour
{
    [SerializeField] private GameObject DialoguePromptObject;
    private bool _playerInRange = false;
    public DialogueStarter dialogueStarter;

    void Update()
    {
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Player pressed E inside trigger!");
            dialogueStarter.StartCinematic();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            DialoguePromptObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            DialoguePromptObject.SetActive(false);
        }
    }
}