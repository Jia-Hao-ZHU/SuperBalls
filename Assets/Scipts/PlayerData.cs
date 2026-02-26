using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("Player Selections (set by menu)")]
    public CharacterData player1Character;
    public CharacterData player2Character;

    
    [HideInInspector] public int player1AbilityCharges = 3;
    [HideInInspector] public int player2AbilityCharges = 3;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public CharacterData GetCharacter(int player) =>
        player == 1 ? player1Character : player2Character;

    public int GetCharges(int player) =>
        player == 1 ? player1AbilityCharges : player2AbilityCharges;

    public bool UseCharge(int player)
    {
        if (player == 1 && player1AbilityCharges > 0) { player1AbilityCharges--; return true; }
        if (player == 2 && player2AbilityCharges > 0) { player2AbilityCharges--; return true; }
        return false;
    }

    public void ResetCharges()
    {
        player1AbilityCharges = 3;
        player2AbilityCharges = 3;
    }
}