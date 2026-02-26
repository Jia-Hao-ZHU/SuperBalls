using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    public enum BallType { Cue, Solid, Stripe, Eight }

    [Header("Identity")]
    public int ballNumber = 0;
    public BallType ballType = BallType.Solid;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public CircleCollider2D col;

    
    [HideInInspector] public float defaultLinearDrag;
    [HideInInspector] public float defaultAngularDrag;

    private const float StopThreshold = 0.04f;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        defaultLinearDrag  = rb.linearDamping;
        defaultAngularDrag = rb.angularDamping;
    }

    public bool IsStopped()
    {
        return rb.linearVelocity.magnitude < StopThreshold;
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pocket"))
            GameManager.Instance.OnBallPocketed(this);
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ballType == BallType.Cue)
        {
            Ball other = collision.gameObject.GetComponent<Ball>();
            if (other != null)
                GameManager.Instance.RegisterFirstHit(other);
        }
    }

    
    public void SetColor(Color color)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = color;
    }

    
    public void SetPhantomAgainst(Ball other, bool ignore)
    {
        if (other == null) return;
        Physics2D.IgnoreCollision(col, other.col, ignore);
    }
}