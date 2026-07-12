using UnityEngine;

public class DialogueStarter : MonoBehaviour
{
    public DialogueTrigger dialogueTrigger;
    private bool wasDialogueActive = false;

    private void Update()
    {
        if (DialogueManager.Instance != null)
        {
            if (wasDialogueActive && !DialogueManager.Instance.isDialogueActive)
            {
                EndCinematic();
            }
            wasDialogueActive = DialogueManager.Instance.isDialogueActive;
        }
    }

    public void StartCinematic()
    {
        dialogueTrigger.TriggerDialogue();
    }

    public void EndCinematic()
    {
        // No animations to undo in 2D — dialogue simply ends.
        // Kept as a hook in case you want to re-enable player input,
        // play a close animation, etc. later.
    }
}