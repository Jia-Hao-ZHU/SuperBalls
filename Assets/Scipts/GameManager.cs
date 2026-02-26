using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core")]
    public CueController cueController;
    public GameObject cueBallPrefab;
    public Camera mainCamera;

    [Header("Turn Background Colors")]
    public Color player1BgColor = new Color(0.05f, 0.10f, 0.25f);
    public Color player2BgColor = new Color(0.25f, 0.05f, 0.05f);
    public float bgTransitionSpeed = 2f;

    [Header("UI")]
    public UIManager uiManager;

    public enum State { WaitingToShoot, BallsMoving, PlacingCueBall, GameOver }
    public State CurrentState { get; private set; } = State.WaitingToShoot;
    public bool CanShoot => CurrentState == State.WaitingToShoot;


    private int currentPlayer = 1;
    private Ball.BallType player1Type = Ball.BallType.Solid;
    private Ball.BallType player2Type = Ball.BallType.Stripe;

    private List<Ball> activeBalls = new List<Ball>();
    private List<Ball> pocketedThisTurn = new List<Ball>();
    private bool cueBallPocketed = false;
    private Ball firstBallHit = null;
    private bool firstHitLogged = false;
    private Color targetBgColor;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        RefreshBallList();
        targetBgColor = player1BgColor;
        if (mainCamera) mainCamera.backgroundColor = player1BgColor;
        uiManager?.Refresh(currentPlayer, player1Type, player2Type);
        uiManager?.SetStatus("Player 1 (blue) — aim and shoot! [Q] ability");
    }

    void Update()
    {
        if (mainCamera)
            mainCamera.backgroundColor = Color.Lerp(
                mainCamera.backgroundColor, targetBgColor, Time.deltaTime * bgTransitionSpeed);

        if (CurrentState == State.BallsMoving && AllStopped())
            StartCoroutine(HandleTurnEnd());

        if (CurrentState == State.PlacingCueBall)
            HandleCueBallPlacement();

        uiManager?.SetPower(cueController ? cueController.currentPower : 0f);
    }

    public void TryUseAbility()
    {
        if (CurrentState != State.WaitingToShoot) return;

        PlayerData pd = PlayerData.Instance;
        if (pd == null) return;

        CharacterData character = pd.GetCharacter(currentPlayer);
        if (character == null) return;

        if (!pd.UseCharge(currentPlayer))
        {
            uiManager?.SetStatus("No ability charges remaining!");
            return;
        }

        Ball cueBall = cueController.cueBall;
        Ball.BallType myType = CurrentType();

        AbilityManager.Instance?.ActivateAbility(character.abilityType, myType, cueBall);
        uiManager?.RefreshCharges(currentPlayer, pd.GetCharges(1), pd.GetCharges(2));
    }

    public void RegisterFirstHit(Ball ball)
    {
        if (!firstHitLogged && CurrentState == State.BallsMoving)
        {
            firstBallHit = ball;
            firstHitLogged = true;
        }
    }

    public void OnBallPocketed(Ball ball)
    {
        activeBalls.Remove(ball);

        if (ball.ballType == Ball.BallType.Cue)
        {
            cueBallPocketed = true;
            ball.gameObject.SetActive(false);
            return;
        }

        if (ball.ballType == Ball.BallType.Eight)
        {
            HandleEightBallPocketed();
            return;
        }

        pocketedThisTurn.Add(ball);
        ball.gameObject.SetActive(false);
    }

    public void OnShotFired()
    {
        CurrentState = State.BallsMoving;
        pocketedThisTurn.Clear();
        cueBallPocketed = false;
        firstBallHit = null;
        firstHitLogged = false;
    }

    bool AllStopped()
    {
        foreach (var b in activeBalls)
            if (b != null && b.gameObject.activeSelf && !b.IsStopped()) return false;
        return true;
    }

    IEnumerator HandleTurnEnd()
    {
        CurrentState = State.WaitingToShoot;
        yield return new WaitForSeconds(0.35f);

        bool foul = false;
        bool keepTurn = false;

        if (cueBallPocketed)
        {
            foul = true;
            uiManager?.SetStatus($"Scratch! Player {Opponent()} places the cue ball.");
        }
        else if (firstBallHit != null)
        {
            Ball.BallType myType = CurrentType();
            if (firstBallHit.ballType != myType && firstBallHit.ballType != Ball.BallType.Eight)
            {
                foul = true;
                uiManager?.SetStatus($"Foul — wrong ball hit! Player {Opponent()}'s turn.");
            }
        }
        else if (firstBallHit == null && activeBalls.Count > 1)
        {
            foul = true;
            uiManager?.SetStatus($"Foul — no ball hit! Player {Opponent()}'s turn.");
        }

        if (!foul && pocketedThisTurn.Count > 0)
        {
            bool pottedOwn = pocketedThisTurn.Exists(b => b.ballType == CurrentType());
            keepTurn = pottedOwn;
        }

        if (foul || !keepTurn) SwitchPlayer();

        if (cueBallPocketed)
            CurrentState = State.PlacingCueBall;
        else
            CurrentState = State.WaitingToShoot;

        if (!foul)
        {
            string color = currentPlayer == 1 ? "blue" : "red";
            string msg = keepTurn
                ? $"Player {currentPlayer} pots one — shoot again! [Q] ability"
                : $"Player {currentPlayer} ({color}) — your turn. [Q] ability";
            uiManager?.SetStatus(msg);
        }

        uiManager?.Refresh(currentPlayer, player1Type, player2Type);
        CheckWinHints();
    }

    void SwitchPlayer()
    {
        currentPlayer = Opponent();
        targetBgColor = currentPlayer == 1 ? player1BgColor : player2BgColor;
    }

    void CheckWinHints()
    {
        if (!activeBalls.Exists(b => b.ballType == CurrentType()))
            uiManager?.SetStatus($"Player {currentPlayer} — pot the 8-ball to win! [Q] ability");
    }

    void HandleEightBallPocketed()
    {
        bool mysDone = !activeBalls.Exists(b => b.ballType == CurrentType());
        string msg = mysDone
            ? $"Player {currentPlayer} wins!"
            : $"Player {currentPlayer} pots the 8-ball early — Player {Opponent()} wins!";
        EndGame(msg);
    }

    void HandleCueBallPlacement()
    {
        uiManager?.SetStatus($"Player {currentPlayer} — click to place the cue ball.");
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0f;

        if (Mathf.Abs(pos.x) > 4.6f || Mathf.Abs(pos.y) > 2.1f) return;

        Ball cueBall = cueController.cueBall;
        cueBall.transform.position = pos;
        cueBall.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        cueBall.GetComponent<Rigidbody2D>().angularVelocity = 0f;
        cueBall.gameObject.SetActive(true);
        activeBalls.Add(cueBall);

        CurrentState = State.WaitingToShoot;
        string color = currentPlayer == 1 ? "blue" : "red";
        uiManager?.SetStatus($"Player {currentPlayer} ({color}) — your turn. [Q] ability");
    }

    IEnumerator SetCueBallNextFrame(Ball ball)
    {
        yield return null;
        cueController.SetCueBall(ball);
    }

    void EndGame(string msg)
    {
        CurrentState = State.GameOver;
        uiManager?.ShowGameOver(msg);
    }

    void RefreshBallList()
    {
        activeBalls.Clear();
        activeBalls.AddRange(FindObjectsByType<Ball>(FindObjectsSortMode.None));
    }

    Ball.BallType CurrentType() => currentPlayer == 1 ? player1Type : player2Type;
    int Opponent() => currentPlayer == 1 ? 2 : 1;

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void GoToMenu() => SceneManager.LoadScene("MainMenu");
}