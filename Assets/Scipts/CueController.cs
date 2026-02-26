using UnityEngine;

public class CueController : MonoBehaviour
{
    [Header("Scene References")]
    public Ball cueBall;
    public LineRenderer aimLine;
    public LineRenderer cueStickLine;

    [Header("Shot Settings")]
    public float maxForce = 12f;
    public float maxPullDistance = 2.5f;

    [Header("Aim Guide")]
    public float aimLineLength = 9f;
    public int maxReflections = 2;
    public LayerMask aimLayerMask;  

    [HideInInspector] public float currentPower;
    private bool isAiming;

    void Update()
    {
        if (!GameManager.Instance.CanShoot || cueBall == null) return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 cueBallPos = cueBall.transform.position;
        Vector2 toMouse   = mouseWorld - cueBallPos;
        Vector2 shootDir  = toMouse.normalized;

        
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Space))
            GameManager.Instance.TryUseAbility();

        if (Input.GetMouseButtonDown(0)) isAiming = true;

        if (isAiming)
        {
            currentPower = Mathf.Clamp01(toMouse.magnitude / maxPullDistance);
            DrawAimLine(cueBallPos, shootDir);
            DrawCueStick(cueBallPos, shootDir, toMouse.magnitude);

            if (Input.GetMouseButtonUp(0))
            {
                Shoot(shootDir);
                isAiming = false;
                HideLines();
            }
        }
        else
        {
            DrawAimLine(cueBallPos, shootDir);
            HideCueStick();
        }
    }

    void Shoot(Vector2 direction)
    {
        cueBall.rb.AddForce(direction * currentPower * maxForce, ForceMode2D.Impulse);
        GameManager.Instance.OnShotFired();
    }

    void DrawAimLine(Vector2 origin, Vector2 direction)
    {
        if (!aimLine) return;

        var points = new System.Collections.Generic.List<Vector3>();
        points.Add(origin);
        Vector2 pos = origin;
        Vector2 dir = direction;
        float remaining = aimLineLength;

        for (int i = 0; i <= maxReflections; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(pos + dir * 0.15f, dir, remaining, aimLayerMask);
            if (hit.collider != null)
            {
                points.Add(hit.point);
                if (hit.collider.GetComponent<Ball>() != null) break;
                remaining -= hit.distance;
                dir = Vector2.Reflect(dir, hit.normal);
                pos = hit.point + dir * 0.05f;
            }
            else
            {
                points.Add(pos + dir * remaining);
                break;
            }
        }

        aimLine.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
            aimLine.SetPosition(i, new Vector3(points[i].x, points[i].y, -0.1f));
    }

    void DrawCueStick(Vector2 cueBallPos, Vector2 shootDir, float pullDist)
    {
        if (!cueStickLine) return;
        float pull     = Mathf.Clamp(pullDist, 0, maxPullDistance);
        Vector2 tip    = cueBallPos - shootDir * (0.25f + pull);
        Vector2 butt   = tip - shootDir * 3.5f;
        cueStickLine.positionCount = 2;
        cueStickLine.SetPosition(0, new Vector3(tip.x,  tip.y,  -0.1f));
        cueStickLine.SetPosition(1, new Vector3(butt.x, butt.y, -0.1f));
    }

    void HideLines()   { HideCueStick(); if (aimLine) aimLine.positionCount = 0; }
    void HideCueStick(){ if (cueStickLine) cueStickLine.positionCount = 0; }

    public void SetCueBall(Ball ball) { cueBall = ball; }
}