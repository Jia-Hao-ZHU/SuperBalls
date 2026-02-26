using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Screens")]
    public GameObject titleScreen;          
    public GameObject characterSelectScreen;

    [Header("Character Select UI")]
    public TextMeshProUGUI selectingPlayerText; 
    public GameObject[] characterCards;         
    public TextMeshProUGUI charNameText;        
    public TextMeshProUGUI charDescText;        
    public TextMeshProUGUI abilityNameText;     
    public TextMeshProUGUI abilityDescText;     
    public Image charPortrait;                  
    public Button confirmButton;                

    [Header("Character Data (assign 3 ScriptableObjects)")]
    public CharacterData[] characters = new CharacterData[3];

    [Header("Game Scene Name")]
    public string gameSceneName = "GameScene";

    
    private int selectingPlayer = 1;
    private int hoveredIndex    = 0;
    private int p1Selection     = -1;

    
    void Start()
    {
        EnsurePlayerData();
        ShowTitle();
    }

    void EnsurePlayerData()
    {
        if (PlayerData.Instance == null)
        {
            GameObject go = new GameObject("PlayerData");
            go.AddComponent<PlayerData>();
        }
    }

    public void OnPressStart()
    {
        titleScreen.SetActive(false);
        characterSelectScreen.SetActive(true);
        selectingPlayer = 1;
        BeginSelection();
    }

    
    void BeginSelection()
    {
        selectingPlayerText.text = $"Player {selectingPlayer} — Choose Your Character";
        hoveredIndex = 0;
        PreviewCharacter(0);
        UpdateCardHighlights();

        confirmButton.GetComponentInChildren<TextMeshProUGUI>().text =
            selectingPlayer == 1 ? "Confirm" : "Play!";
    }

    public void OnHoverCharacter(int index)
    {
        
        hoveredIndex = index;
        PreviewCharacter(index);
        UpdateCardHighlights();
    }

    public void OnClickCharacter(int index)
    {
        hoveredIndex = index;
        PreviewCharacter(index);
        UpdateCardHighlights();
    }

    public void OnConfirm()
    {
        if (hoveredIndex < 0 || hoveredIndex >= characters.Length) return;

        CharacterData chosen = characters[hoveredIndex];

        if (selectingPlayer == 1)
        {
            p1Selection = hoveredIndex;
            PlayerData.Instance.player1Character = chosen;
            selectingPlayer = 2;
            BeginSelection();
        }
        else
        {
            
            if (hoveredIndex == p1Selection)
            {
                selectingPlayerText.text = "Player 2 — Pick a different character!";
                return;
            }
            PlayerData.Instance.player2Character = chosen;
            PlayerData.Instance.ResetCharges();
            SceneManager.LoadScene(gameSceneName);
        }
    }

    void PreviewCharacter(int index)
    {
        if (index < 0 || index >= characters.Length || characters[index] == null) return;
        CharacterData c = characters[index];
        if (charNameText)    charNameText.text    = c.characterName;
        if (charDescText)    charDescText.text    = c.description;
        if (abilityNameText) abilityNameText.text = c.abilityName;
        if (abilityDescText) abilityDescText.text = c.abilityDescription;
        if (charPortrait)    charPortrait.color   = c.primaryColor;
        if (charPortrait && c.portrait) charPortrait.sprite = c.portrait;
    }

    void UpdateCardHighlights()
    {
        for (int i = 0; i < characterCards.Length; i++)
        {
            if (characterCards[i] == null) continue;
            Image img = characterCards[i].GetComponent<Image>();
            if (img == null) continue;

            bool isHovered   = i == hoveredIndex;
            bool isP1Locked  = selectingPlayer == 2 && i == p1Selection;

            img.color = isP1Locked  ? new Color(0.4f, 0.4f, 0.4f, 1f) :  
                        isHovered   ? characters[i]?.primaryColor ?? Color.white :
                                      new Color(0.2f, 0.2f, 0.2f, 1f);
        }
    }

    void ShowTitle()
    {
        titleScreen.SetActive(true);
        characterSelectScreen.SetActive(false);
    }
}