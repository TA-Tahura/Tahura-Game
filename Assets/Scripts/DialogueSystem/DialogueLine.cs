using UnityEngine;

[CreateAssetMenu(fileName = "DialogueLine", menuName = "ScriptableObjects/Dialogue/DialogueLine")]
public class DialogueLine : ScriptableObject
{
    public Character character;
    [TextArea(3, 10)]
    public string line;
}
