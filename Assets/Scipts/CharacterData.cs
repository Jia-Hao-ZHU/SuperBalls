using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Pool/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    [TextArea] public string description;
    public string abilityName;
    [TextArea] public string abilityDescription;
    public Color primaryColor;
    public AbilityType abilityType;
    public Sprite portrait; 
}

public enum AbilityType
{
    Phantom,

    LowFriction,

    Boom
}