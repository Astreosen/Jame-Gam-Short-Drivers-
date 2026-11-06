using UnityEngine;

public class GearShifter : MonoBehaviour
{
    [Header("Gear Points (in this order: 1, 2, 3, 4, 5, R)")]
    public Transform[] gearPoints = new Transform[6];

    [Header("Settings")]
    public float snapDistance = 0.05f;
    public float maxSpeed = 3f;          

    [Header("Output")]
    public bool[] gearActive = new bool[6];
    public int currentGear = -1;  
    public float junctionLeeway = 0.04f;  


    private readonly int[,] segs =
    {
        {0, 6}, {6, 1},     
        {2, 7}, {7, 3},    
        {4, 8}, {8, 5},    
        {6, 7}, {7, 8}     
    };

    private Camera cam;
    private bool dragging;
    private Vector3 grabOffset;
    private int seg;       
    private float t;      

    void Start()
    {
        cam = Camera.main;

       
        Vector3[] n = GetNodes();
        float best = float.MaxValue;
        for (int s = 0; s < segs.GetLength(0); s++)
        {
            Vector3 a = n[segs[s, 0]];
            Vector3 b = n[segs[s, 1]];
            float pt = ProjectT(a, b, transform.position);
            float d = (Vector3.Lerp(a, b, pt) - transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                seg = s;
                t = pt;
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                dragging = hit.collider.transform.IsChildOf(transform);
                if (dragging && MouseOnGatePlane(out Vector3 p))
                {
                    grabOffset = transform.position - p;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (dragging && MouseOnGatePlane(out Vector3 mousePoint))
        {
            MoveAlongPath(mousePoint + grabOffset);
        }

        ApplyPosition();
    }

    void MoveAlongPath(Vector3 target)
    {
        Vector3[] n = GetNodes();
        float budget = maxSpeed * Time.deltaTime;

        for (int iter = 0; iter < 10; iter++)
        {
            Vector3 a = n[segs[seg, 0]];
            Vector3 b = n[segs[seg, 1]];
            float len = (b - a).magnitude;

            float desiredT = ProjectT(a, b, target);
            float step = Mathf.Clamp((desiredT - t) * len, -budget, budget);
            t += step / len;
            budget -= Mathf.Abs(step);

            float distAlong = t * len;
            int node;
            if (distAlong <= junctionLeeway) node = segs[seg, 0];
            else if (len - distAlong <= junctionLeeway) node = segs[seg, 1];
            else break;

            float currentDist = (Vector3.Lerp(a, b, desiredT) - target).magnitude;

            float bestDist = currentDist - 0.0001f;
            int bestSeg = -1;

            for (int s = 0; s < segs.GetLength(0); s++)
            {
                if (s == seg) continue;
                if (segs[s, 0] != node && segs[s, 1] != node) continue;

                Vector3 sa = n[segs[s, 0]];
                Vector3 sb = n[segs[s, 1]];
                float pt = ProjectT(sa, sb, target);
                float d = (Vector3.Lerp(sa, sb, pt) - target).magnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    bestSeg = s;
                }
            }

            if (bestSeg == -1) break;

            seg = bestSeg;
            t = segs[seg, 0] == node ? 0f : 1f;
        }
    }

    void ApplyPosition()
    {
        Vector3[] n = GetNodes();
        Vector3 pos = Vector3.Lerp(n[segs[seg, 0]], n[segs[seg, 1]], t);

        int nearest = -1;
        float nearestDist = snapDistance;
        for (int i = 0; i < 6; i++)
        {
            float d = Vector3.Distance(pos, n[i]);
            if (d < nearestDist)
            {
                nearestDist = d;
                nearest = i;
            }
        }

        if (nearest != -1) pos = n[nearest];

        SetGear(nearest);
        transform.position = pos;
    }

    void SetGear(int gear)
    {
        currentGear = gear;
        for (int i = 0; i < gearActive.Length; i++)
        {
            gearActive[i] = (i == gear);
        }
    }

    Vector3[] GetNodes()
    {
        Vector3[] n = new Vector3[9];
        for (int i = 0; i < 6; i++) n[i] = gearPoints[i].position;
        n[6] = (n[0] + n[1]) / 2f;
        n[7] = (n[2] + n[3]) / 2f;
        n[8] = (n[4] + n[5]) / 2f;
        return n;
    }

    float ProjectT(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        return Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
    }

    bool MouseOnGatePlane(out Vector3 point)
    {
        Vector3 normal = Vector3.Cross(
            gearPoints[1].position - gearPoints[0].position,
            gearPoints[2].position - gearPoints[0].position).normalized;

        Plane plane = new Plane(normal, gearPoints[0].position);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter))
        {
            point = ray.GetPoint(enter);
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    void OnDrawGizmos()
    {
        if (gearPoints == null || gearPoints.Length < 6) return;
        foreach (var g in gearPoints) if (g == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(gearPoints[0].position, gearPoints[1].position);
        Gizmos.DrawLine(gearPoints[2].position, gearPoints[3].position);
        Gizmos.DrawLine(gearPoints[4].position, gearPoints[5].position);
        Gizmos.DrawLine((gearPoints[0].position + gearPoints[1].position) / 2f,
                        (gearPoints[4].position + gearPoints[5].position) / 2f);
    }
}