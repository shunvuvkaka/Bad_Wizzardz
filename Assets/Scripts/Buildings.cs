using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Buildings : MonoBehaviour
{
    [Header("Building Params")]
    public float roadDist;
    public float maxDist;
    public float minBreadth;
    [Header("Building Size and Spacing")]
    public Vector2Int widthRange;
    public Vector2Int spaceRange;
    public Vector2 heightRange;
    public float buildingGap;
    [Header("References")]
    [SerializeField] private Transform buildingParent;
    private Dictionary<int, Vector3> points;
    private Dictionary<int, Vector3> normals;
    private Road road;
    private int rIndex = 0;
    private int lIndex = 0;
    private Color impoactCol;

    void Start()
    {
        road = Road.Instance;
        roadDist += road.roadWidth / 2;
    }

    void LateUpdate()
    {
        points = road.pointDict;
        normals = road.normalDict;

        while (rIndex < road.globalIndex - 10)
        {
            int width = Mathf.RoundToInt(Random.Range(widthRange.x, widthRange.y));
            int space = Mathf.RoundToInt(Random.Range(spaceRange.x, spaceRange.y));
            float height = Random.Range(heightRange.x, heightRange.y);

            //negative sign flipps normal to right
            Vector3 fPoint = points[rIndex];
            Vector3 fNormal = -normals[rIndex];
            Vector3 lPoint = points[rIndex + width];
            Vector3 lNormal = -normals[rIndex + width];

            //calulating farthest edge distance
            float backDist = CalculateDistance(fPoint, fNormal, lPoint, lNormal);

            if (backDist < minBreadth)
            {
                rIndex ++;
                continue;
            }

            //creating points that will be converted to mesh
            BasePoints basePoints = new BasePoints
            (
                fPoint + fNormal * backDist,
                lPoint + lNormal * backDist,
                fPoint + fNormal * roadDist,
                lPoint + lNormal * roadDist
            );

            ConstructBuilding(basePoints, height);

            Debug.DrawRay(fPoint, fNormal * backDist, impoactCol, float.MaxValue);
            Debug.DrawRay(lPoint, lNormal * backDist, impoactCol, float.MaxValue);

            rIndex = rIndex + width + space;

        }

        /*
        while (lIndex < road.globalIndex - 10)
        {
            Debug.DrawRay(points[lIndex], normals[lIndex] * roadDist, Color.red, float.MaxValue);
            lIndex++;
        }
        */
    }

    float CalculateDistance(Vector3 p, Vector3 r, Vector3 q, Vector3 s)
    {
        impoactCol = Color.black;

        //checks whether the intersect or collision of the back edge is closest
        float iDist = IntersectDistance(p, r, q, s);
        float rDist = RaycastDistance(p, r, q, s);

        return Mathf.Min(iDist, rDist);
    }

    float IntersectDistance(Vector3 p, Vector3 r, Vector3 q, Vector3 s)
    {
        //convert from 3d plane to 2d xy plane
        p = new Vector2(p.x, p.z);
        q = new Vector2(q.x, q.z);

        r = new Vector2(r.x, r.z).normalized;
        s = new Vector2(s.x, s.z).normalized;

        //cross between two normals (unity whyyyyyyy do u provide vector2 dot and not vector2 cross???)
        float rxs = r.x * s.y - r.y * s.x;

        //parrallel case
        if (Mathf.Abs(rxs) < 0.0001f)
            return maxDist + roadDist;

        Vector2 qp = q - p;

        //solve how far the intersection is for both vectors
        float t = (qp.x * s.y - qp.y * s.x) / rxs;
        float u = (qp.x * r.y - qp.y * r.x) / rxs;

        //check if an intersection will occur behind
        if (t < 0 || u < 0)
            return maxDist + roadDist;

        //calculate how the road disposition affects vector
        Vector2 pqDir = (q - p).normalized;
        Vector2 qpDir = (p - q).normalized;

        //"1 - ..." as iff road point is perpendicular already perfectly aligned
        float pDot = 1 - Vector2.Dot(pqDir, r);
        float qDot = 1 - Vector2.Dot(qpDir, s);

        //move along length in normal direction from point for intersection
        Vector2 intersection = p + t * r;

        //find and record the minimum length to intersection * disposition
        float distA = Vector2.Distance(p, intersection) * pDot;
        float distB = Vector2.Distance(q, intersection) * qDot;

        float min = Mathf.Min(distA, distB);

        if (min < maxDist + roadDist)
        {
            impoactCol = Color.blue;
            return min;
        }

        return maxDist + roadDist;
    }

    float RaycastDistance(Vector3 p, Vector3 r, Vector3 q, Vector3 s)
    {
        RaycastHit pHit;
        RaycastHit qHit;

        p += Vector3.up * 0.1f;
        q += Vector3.up * 0.1f;

        Physics.Raycast(p, r, out pHit, maxDist + roadDist);
        Physics.Raycast(q, s, out qHit, maxDist + roadDist);

        if (pHit.collider == null && qHit.collider == null)
            return maxDist + roadDist;

        impoactCol = Color.red;

        float min = Mathf.Min(pHit.distance, qHit.distance);

        return min - buildingGap;
    }

    void ConstructBuilding(BasePoints basePoints, float height)
    {
        //simply moves the base points up by height
        BasePoints topPoints = new BasePoints
        (
            basePoints.br + (Vector3.up * height),
            basePoints.tr + (Vector3.up * height),
            basePoints.bl + (Vector3.up * height),
            basePoints.tl + (Vector3.up * height)
        );
        GameObject go = new GameObject($" Building {rIndex}");
        go.transform.parent = buildingParent;

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        MeshCollider mc = go.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();

        //oh boy we love some manually winded tris

        mesh.vertices = new Vector3[]{basePoints.br, basePoints.bl, basePoints.tr, basePoints.tl, 
                                      topPoints.br, topPoints.bl, topPoints.tr, topPoints.tl};
        
        mesh.triangles = new int[]
        {
            1, 4, 0,
            5, 4, 1,
            5, 1, 3,
            7, 5, 3,
            7, 3, 2,
            2, 6, 7,
            0, 4, 6,
            6, 2, 0,
            5, 7, 6,
            6, 4, 5
        };
        mesh.RecalculateNormals();

        mf.mesh = mesh;
        mc.sharedMesh = mesh;

        mr.material = road.roadMaterial;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        //debug lines
        Debug.DrawLine(basePoints.br, basePoints.bl, Color.cyan, float.MaxValue);
        Debug.DrawLine(basePoints.bl, basePoints.tl, Color.cyan, float.MaxValue);
        Debug.DrawLine(basePoints.tl, basePoints.tr, Color.cyan, float.MaxValue);
        Debug.DrawLine(basePoints.tr, basePoints.br, Color.cyan, float.MaxValue);

        Debug.DrawLine(topPoints.br, topPoints.bl, Color.cyan, float.MaxValue);
        Debug.DrawLine(topPoints.bl, topPoints.tl, Color.cyan, float.MaxValue);
        Debug.DrawLine(topPoints.tl, topPoints.tr, Color.cyan, float.MaxValue);
        Debug.DrawLine(topPoints.tr, topPoints.br, Color.cyan, float.MaxValue);

        Debug.DrawLine(basePoints.br, topPoints.br, Color.cyan, float.MaxValue);
        Debug.DrawLine(basePoints.bl, topPoints.bl, Color.cyan, float.MaxValue);
        Debug.DrawLine(basePoints.tl, topPoints.tl, Color.cyan, float.MaxValue);
        Debug.DrawLine(basePoints.tr, topPoints.tr, Color.cyan, float.MaxValue);
    }
}
