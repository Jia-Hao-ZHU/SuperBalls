using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Status")]
    public TextMeshProUGUI statusText;

    [Header("Player 1 Panel")]
    public TextMeshProUGUI p1NameText;
    public TextMeshProUGUI p1TypeText;
    public TextMeshProUGUI p1AbilityText;
    public TextMeshProUGUI p1ChargesText;
    public Image p1Panel;

    [Header("Player 2 Panel")]
    public TextMeshProUGUI p2NameText;
    public TextMeshProUGUI p2TypeText;
    public TextMeshProUGUI p2AbilityText;
    public TextMeshProUGUI p2ChargesText;
    public Image p2Panel;

    [Header("Power Bar")]
    public Slider powerSlider;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;

    [Header("Ability Banner")]
    public TextMeshProUGUI abilityBannerText;

    [Header("Highlight Colors")]
    public Color activePlayerColor = new Color(1f, 1f, 1f, 0.12f);
    public Color inactivePlayerColor = new Color(0.3f, 0.3f, 0.3f, 0.06f);

    void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (abilityBannerText) abilityBannerText.gameObject.SetActive(false);


        PlayerData pd = PlayerData.Instance;
        if (pd != null)
        {
            if (p1AbilityText && pd.player1Character)
                p1AbilityText.text = pd.player1Character.abilityName;
            if (p2AbilityText && pd.player2Character)
                p2AbilityText.text = pd.player2Character.abilityName;

            if (p1NameText && pd.player1Character)
                p1NameText.text = pd.player1Character.characterName;
            if (p2NameText && pd.player2Character)
                p2NameText.text = pd.player2Character.characterName;
        }

        RefreshCharges(0, 3, 3);
    }


    public void Refresh(int currentPlayer, Ball.BallType p1Type, Ball.BallType p2Type)
    {
        if (p1Panel) p1Panel.color = currentPlayer == 1 ? activePlayerColor : inactivePlayerColor;
        if (p2Panel) p2Panel.color = currentPlayer == 2 ? activePlayerColor : inactivePlayerColor;

        if (p1TypeText) p1TypeText.text = "Blue balls";
        if (p2TypeText) p2TypeText.text = "Red balls";

        PlayerData pd = PlayerData.Instance;
        string p1name = pd?.player1Character?.characterName ?? "Player 1";
        string p2name = pd?.player2Character?.characterName ?? "Player 2";
        if (p1NameText) p1NameText.text = (currentPlayer == 1 ? "▶ " : "   ") + p1name;
        if (p2NameText) p2NameText.text = (currentPlayer == 2 ? "▶ " : "   ") + p2name;
    }

    public void RefreshCharges(int currentPlayer, int p1Charges, int p2Charges)
    {
        if (p1ChargesText) p1ChargesText.text = ChargeString(p1Charges);
        if (p2ChargesText) p2ChargesText.text = ChargeString(p2Charges);
    }

    public void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
    }

    public void SetPower(float value)
    {
        if (powerSlider) powerSlider.value = value;
    }

    public void ShowGameOver(string msg)
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (gameOverText) gameOverText.text = msg;
    }

    public void ShowAbilityBanner(string msg, float duration)
    {
        if (abilityBannerText) StartCoroutine(BannerRoutine(msg, duration));
    }

    IEnumerator BannerRoutine(string msg, float duration)
    {
        abilityBannerText.text = msg;
        abilityBannerText.gameObject.SetActive(true);

        float half = duration * 0.25f;
        float elapsed = 0f;
        Color baseColor = abilityBannerText.color;


        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            abilityBannerText.color = new Color(baseColor.r, baseColor.g, baseColor.b, elapsed / half);
            yield return null;
        }

        yield return new WaitForSeconds(duration * 0.5f);


        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            abilityBannerText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - elapsed / half);
            yield return null;
        }

        abilityBannerText.gameObject.SetActive(false);
    }

    string ChargeString(int charges)
    {

        string s = "";
        for (int i = 0; i < 3; i++)
            s += i < charges ? "●" : "○";
        return s;
    }
}