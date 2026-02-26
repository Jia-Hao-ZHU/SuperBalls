using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject ballPrefab;

    [Header("Rack Settings")]
    public Vector2 rackTip = new Vector2(2.5f, 0f);
    public float rowSpacingX = 0.312f;
    public float colSpacingY = 0.37f;

    [Header("Ball Colors")]
    public Color solidColor = new Color(0.3f, 0.6f, 1.0f);
    public Color stripeColor = new Color(1.0f, 0.3f, 0.3f);

    // Front ball is now the 8-ball
    private (int number, Ball.BallType type)[] rackLayout =
    {
        (8, Ball.BallType.Eight),   // FRONT BALL

        (1, Ball.BallType.Solid),
        (9, Ball.BallType.Stripe),

        (2, Ball.BallType.Solid),
        (10, Ball.BallType.Stripe),
        (3, Ball.BallType.Solid),

        (11, Ball.BallType.Stripe),
        (4, Ball.BallType.Solid),
        (12, Ball.BallType.Stripe),
        (5, Ball.BallType.Solid),

        (13, Ball.BallType.Stripe),
        (6, Ball.BallType.Solid),
        (14, Ball.BallType.Stripe),
        (7, Ball.BallType.Solid),
        (15, Ball.BallType.Stripe)
    };

    private int[] rowCounts = { 1, 2, 3, 4, 5 };

    void Start()
    {
        SpawnRack();
    }

    void SpawnRack()
    {
        int ballIndex = 0;

        for (int row = 0; row < rowCounts.Length; row++)
        {
            int count = rowCounts[row];
            float startY = -(count - 1) * colSpacingY / 2f;

            for (int col = 0; col < count; col++)
            {
                float x = rackTip.x + row * rowSpacingX;
                float y = rackTip.y + startY + col * colSpacingY;

                GameObject go = Instantiate(ballPrefab, new Vector3(x, y, 0f), Quaternion.identity);
                go.name = $"Ball_{rackLayout[ballIndex].number}";

                Ball ball = go.GetComponent<Ball>();
                ball.ballNumber = rackLayout[ballIndex].number;
                ball.ballType = rackLayout[ballIndex].type;

                // Assign color using Ball's method
                if (ball.ballType == Ball.BallType.Solid)
                    ball.SetColor(solidColor);
                else if (ball.ballType == Ball.BallType.Stripe)
                    ball.SetColor(stripeColor);
                else if (ball.ballType == Ball.BallType.Eight)
                    ball.SetColor(Color.black);

                ballIndex++;
            }
        }
    }
}