using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Dialogue/Character")]
public class Character : ScriptableObject
{
    public string characterName;
    public Sprite characterIcon;
}
