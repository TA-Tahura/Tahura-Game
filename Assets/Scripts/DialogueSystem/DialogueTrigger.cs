using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatefulDialogue
{
    public int stateID;
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}

public class DialogueTrigger : MonoBehaviour
{
    public List<StatefulDialogue> dialogues = new List<StatefulDialogue>();
    public int currentStateId = 0;
    [Header("NPC Name")]
    public string npcName;
    private void Update()
    {
        int newStateID = DialogueManager.Instance.GetCharacterState(npcName);
        SetDialogueState(newStateID);
    }

    public void TriggerDialogue()
    {
        Debug.Log($"TriggerDialogue called. Current state ID: {currentStateId}");
        Debug.Log($"Total dialogues available: {dialogues.Count}");
        for (int i = 0; i < dialogues.Count; i++)
        {
            Debug.Log($"Dialogue {i}: State ID = {dialogues[i].stateID}, Lines count = {dialogues[i].dialogueLines.Count}");
        }
        
        StatefulDialogue dialogue = GetDialogueForState(currentStateId);
        if (dialogue != null)
        {
            Debug.Log($"Found dialogue for state {currentStateId} with {dialogue.dialogueLines.Count} lines");
            DialogueManager.Instance.StartDialogue(dialogue);
        }
        else
        {
            Debug.LogError($"No dialogue found for state ID: {currentStateId}");
        }
    }
    
    private StatefulDialogue GetDialogueForState(int stateId)
    {
        foreach (StatefulDialogue dialogue in dialogues)
        {
            if (dialogue.stateID == stateId)
            {
                return dialogue;
            }
        }
        return null;
    }
    
    public void SetDialogueState(int newStateId)
    {
        Debug.Log($"Dialogue state changed from {currentStateId} to {newStateId}");
        currentStateId = newStateId;
    }
}