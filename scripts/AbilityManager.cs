using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance { get; private set; }

    [Header("Boom Settings")]
    public float boomForce = 9f;
    public float boomRadius = 4f;

    [Header("LowFriction Settings")]
    public float lowFrictionDrag = 0.05f;

    private bool phantomActive = false;
    private Ball.BallType phantomOwnerType;
    private List<Ball> phantomIgnoredBalls = new List<Ball>();

    void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
    }

    public void ActivateAbility(AbilityType type, Ball.BallType ownerType, Ball cueBall)
    {
        switch (type)
        {
            case AbilityType.Phantom:   StartCoroutine(PhantomRoutine(ownerType, cueBall)); break;
            case AbilityType.LowFriction: StartCoroutine(LowFrictionRoutine());              break;
            case AbilityType.Boom:      BoomAbility(cueBall);                                break;
        }
    }

    IEnumerator PhantomRoutine(Ball.BallType ownerType, Ball cueBall)
    {
        phantomActive = true;
        phantomOwnerType = ownerType;
        phantomIgnoredBalls.Clear();

        Ball[] allBalls = FindObjectsByType<Ball>(FindObjectsSortMode.None);
        foreach (var b in allBalls)
        {
            bool isOpponent = (b.ballType != ownerType)
                           && (b.ballType != Ball.BallType.Cue)
                           && (b.ballType != Ball.BallType.Eight);
            if (isOpponent)
            {
                cueBall.SetPhantomAgainst(b, true);
                phantomIgnoredBalls.Add(b);
            }
        }

        UIManager.Instance?.ShowAbilityBanner("👻 PHANTOM ACTIVE", 2f);

        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => cueBall == null || cueBall.IsStopped());
        yield return new WaitForSeconds(0.2f);

        if (cueBall != null)
        {
            foreach (var b in phantomIgnoredBalls)
                if (b != null) cueBall.SetPhantomAgainst(b, false);
        }

        phantomIgnoredBalls.Clear();
        phantomActive = false;
    }

    IEnumerator LowFrictionRoutine()
    {
        Ball[] allBalls = FindObjectsByType<Ball>(FindObjectsSortMode.None);

        foreach (var b in allBalls)
        {
            b.rb.linearDamping  = lowFrictionDrag;
            b.rb.angularDamping = lowFrictionDrag;
        }

        UIManager.Instance?.ShowAbilityBanner("🧊 SLICK SHOT ACTIVE", 2f);

        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => {
            foreach (var b in allBalls)
                if (b != null && b.gameObject.activeSelf && !b.IsStopped()) return false;
            return true;
        });

        foreach (var b in allBalls)
        {
            if (b != null)
            {
                b.rb.linearDamping  = b.defaultLinearDrag;
                b.rb.angularDamping = b.defaultAngularDrag;
            }
        }
    }

    void BoomAbility(Ball cueBall)
    {
        if (cueBall == null) return;

        Vector2 origin = cueBall.transform.position;
        Ball[] allBalls = FindObjectsByType<Ball>(FindObjectsSortMode.None);

        foreach (var b in allBalls)
        {
            if (b == cueBall) continue;
            if (!b.gameObject.activeSelf) continue;

            Vector2 dir = (Vector2)b.transform.position - origin;
            float dist  = dir.magnitude;
            if (dist > boomRadius) continue;

            float strength = Mathf.Lerp(boomForce, 0f, dist / boomRadius);
            b.rb.AddForce(dir.normalized * strength, ForceMode2D.Impulse);
        }

        StartCoroutine(BoomVFXRoutine(origin));
        UIManager.Instance?.ShowAbilityBanner("💥 BOOM!", 1.5f);
    }

    IEnumerator BoomVFXRoutine(Vector2 origin)
    {
        
        GameObject ring = new GameObject("BoomRing");
        ring.transform.position = new Vector3(origin.x, origin.y, -0.5f);
        SpriteRenderer sr = ring.AddComponent<SpriteRenderer>();
        sr.sprite = MakeCircleSprite(64);
        sr.color  = new Color(1f, 0.6f, 0.1f, 0.7f);
        sr.sortingOrder = 10;

        float duration = 0.4f;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(0f, boomRadius * 2f, t);
            ring.transform.localScale = new Vector3(scale, scale, 1f);
            sr.color = new Color(1f, 0.6f, 0.1f, Mathf.Lerp(0.7f, 0f, t));
            yield return null;
        }

        Destroy(ring);
    }

    Sprite MakeCircleSprite(int res)
    {
        Texture2D tex = new Texture2D(res, res);
        Vector2 center = new Vector2(res / 2f, res / 2f);
        float outer = res / 2f;
        float inner = outer - 3f;

        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                float a = (d <= outer && d >= inner) ? 1f : 0f;
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }
}
