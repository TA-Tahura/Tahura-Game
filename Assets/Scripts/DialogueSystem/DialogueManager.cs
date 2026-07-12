using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
[System.Serializable]
public class CharacterState
{
    public string characterName;
    public int dialogueState;
}
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
 
    public Image characterIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;
 
    [Header("Character States")]
    public List<CharacterState> CharacterAndStateMap = new List<CharacterState>();
    public Queue<DialogueLine> lines;

    [HideInInspector] public bool isDialogueActive;
 
    public float typingSpeed = 0.2f;
    
 
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
 
        lines = new Queue<DialogueLine>();
    }
 
    public void StartDialogue(StatefulDialogue dialogue)
    {
        isDialogueActive = true;
 
        lines.Clear();
 
        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }
 
        DisplayNextDialogueLine();
    }
 
    public void DisplayNextDialogueLine()
    {
        Debug.Log("lines count:"+lines.Count.ToString());
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }
 
        DialogueLine currentLine = lines.Dequeue();
 
        characterIcon.sprite = currentLine.character.characterIcon;
        characterName.text = currentLine.character.characterName;
 
        StopAllCoroutines();
 
        StartCoroutine(TypeSentence(currentLine));
    }
 
    IEnumerator TypeSentence(DialogueLine dialogueLine)
    {
        dialogueArea.text = "";
        foreach (char letter in dialogueLine.line.ToCharArray())
        {
            dialogueArea.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
 
    void EndDialogue()
    {
        isDialogueActive = false;
    }
    
    public int GetCharacterState(string characterName)
    {
        CharacterState character = CharacterAndStateMap.Find(c => c.characterName == characterName);
        return character != null ? character.dialogueState : -1;
    }
    
    public void SetCharacterState(string characterName, int dialogueState)
    {
        CharacterState character = CharacterAndStateMap.Find(c => c.characterName == characterName);
        
        if (character != null)
        {
            character.dialogueState = dialogueState;
        }
    }
}
