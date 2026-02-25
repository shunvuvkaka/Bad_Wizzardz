using System;
using System.Collections.Generic;
using UnityEngine;

public class Buildings : MonoBehaviour
{
    [Header("Building Params")]
    public float roadDist;
    public float maxDist;
    public float minBreadth;
    public Material buildingMat;
    public int generations;
    [Header("Building Size and Spacing")]
    public Vector2Int widthRange;
    public Vector2Int spaceRange;
    public Vector2 heightRange;
    public float buildingGap;
    [Header("References")]
    [SerializeField] private Transform buildingParent;

    public bool debugLines;
    private Dictionary<int, Vector3> points;
    private Dictionary<int, Vector3> normals;
    [SerializeField] private List<BuildPoints> rBuilds = new List<BuildPoints>();
    [SerializeField] private List<BuildPoints> lBuilds = new List<BuildPoints>();
    private Road road;
    private int rIndex = 0;
    private int lIndex = 0;
    private int endBuffer;
    private Color impoactCol;
    private int currGen;

    void Start()
    {
        endBuffer = widthRange.y + spaceRange.y + 1;
        road = Road.Instance;
        roadDist += road.roadWidth / 2;
    }

    void LateUpdate()
    {
        points = road.pointDict;
        normals = road.normalDict;

        while (rIndex < road.globalIndex - endBuffer)
        {
            InitialBuildings(ref rIndex, true);
        }

        while (lIndex < road.globalIndex - endBuffer)
        {
            InitialBuildings(ref lIndex, false);
        }

        if (rBuilds.Count > 0)
        {
            while (currGen < generations)
            {
                currGen++;
                GenerativeBuildings(ref rBuilds, true);
                GenerativeBuildings(ref lBuilds, false);
            }
        }

        currGen = 0;
        
    }
    void InitialBuildings(ref int index, bool right)
    {
        int width = Mathf.RoundToInt(UnityEngine.Random.Range(widthRange.x, widthRange.y));
        int space = Mathf.RoundToInt(UnityEngine.Random.Range(spaceRange.x, spaceRange.y));
        float height = UnityEngine.Random.Range(heightRange.x, heightRange.y);

        BuildPoints build = new BuildPoints
        {
            fp = points[index],
            lp = points[index + width]
        };

        if (right)
        {
            //negative sign flipps normal to right
            build.fn = -normals[index];
            build.ln = -normals[index + width];
        }
        else
        {
            build.fn = normals[index];
            build.ln = normals[index + width];
        }

        //calulating farthest edge distance
        float backDist = CalculateDistance(build);

        if (backDist - roadDist < minBreadth)
        {
            index ++;
            return;
        }

        //creating points that will be converted to mesh
        BasePoints basePoints = new BasePoints
        (
            build.fp + build.fn * backDist,
            build.lp + build.ln * backDist,
            build.fp + build.fn * roadDist,
            build.lp + build.ln * roadDist
        );

        ConstructBuilding(basePoints, height, right);

        if (debugLines)
        {
            Debug.DrawRay(build.fp, build.fn * backDist, impoactCol, float.MaxValue);
            Debug.DrawRay(build.lp, build.ln * backDist, impoactCol, float.MaxValue);
        }

        index = index + width + space;
    }

    void GenerativeBuildings(ref List<BuildPoints> prevBuilds, bool right)
    {
        BuildPoints[] builds = new BuildPoints[prevBuilds.Count];
        prevBuilds.CopyTo(builds);
        prevBuilds.Clear();

        foreach (BuildPoints build in builds)
        {
            float height = UnityEngine.Random.Range(heightRange.x, heightRange.y);

            //calulating farthest edge distance
            float backDist = CalculateDistance(build);
            
            
            if (backDist - buildingGap < minBreadth)
            {
                continue;
            }
            

            //creating points that will be converted to mesh
            BasePoints basePoints = new BasePoints
            (
                build.fp + build.fn * backDist,
                build.lp + build.ln * backDist,
                build.fp + build.fn * buildingGap,
                build.lp + build.ln * buildingGap
            );

            if (debugLines)
            {
                Debug.DrawRay(build.fp, build.fn * backDist, impoactCol, float.MaxValue);
                Debug.DrawRay(build.lp, build.ln * backDist, impoactCol, float.MaxValue);
            }

            ConstructBuilding(basePoints, height, right);
        }
    }

    float CalculateDistance(BuildPoints build)
    {
        impoactCol = Color.black;

        //checks whether the intersect or collision of the back edge is closest
        float iDist = IntersectDistance(build);
        float rDist = RaycastDistance(build);

        return Mathf.Min(iDist, rDist);
    }

    float IntersectDistance(BuildPoints build)
    {
        //convert from 3d plane to 2d xy plane
        Vector2 p = new Vector2(build.fp.x, build.fp.z);
        Vector2 q = new Vector2(build.lp.x, build.lp.z);

        Vector2 r = new Vector2(build.fn.x, build.fn.z).normalized;
        Vector2 s = new Vector2(build.ln.x, build.ln.z).normalized;

        //cross between two normals (unity whyyyyyyy do u provide vector2 dot and not vector2 cross???)
        float rxs = r.x * s.y - r.y * s.x;

        //parrallel case
        if (Mathf.Abs(rxs) < 0.01f)
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

    float RaycastDistance(BuildPoints build)
    {
        RaycastHit pHit;
        RaycastHit qHit;

        build.fp += Vector3.up * 0.1f;
        build.lp += Vector3.up * 0.1f;

        Physics.Raycast(build.fp, build.fn, out pHit, maxDist + roadDist);
        Physics.Raycast(build.lp, build.ln, out qHit, maxDist + roadDist);

        float min;

        if (pHit.collider == null && qHit.collider == null)
            return maxDist + roadDist;
        else if (pHit.collider == null)
            min = qHit.distance;
        else if (qHit.collider == null)
            min = pHit.distance;
        else
            min = Mathf.Min(pHit.distance, qHit.distance);

        impoactCol = Color.red;

        return min - buildingGap;
    }

    void ConstructBuilding(BasePoints basePoints, float height, bool right)
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

        Vector3[] vertices = new Vector3[]
        {
            //bottom
            basePoints.br, //0
            basePoints.bl, //1
            basePoints.tl, //2
            basePoints.tr, //3

            //top
            topPoints.br,  //4
            topPoints.bl,  //5
            topPoints.tl,  //6
            topPoints.tr,  //7

            //front
            basePoints.bl, //8
            basePoints.br, //9
            topPoints.br,  //10
            topPoints.bl,  //11

            //back
            basePoints.tr, //12
            basePoints.tl, //13
            topPoints.tl,  //14
            topPoints.tr,  //15

            //left
            basePoints.tl, //16
            basePoints.bl, //17
            topPoints.bl,  //18
            topPoints.tl,  //19

            //right
            basePoints.br, //20
            basePoints.tr, //21
            topPoints.tr,  //22
            topPoints.br   //23
        };

        int[] triangles = new int[]
        {
            //bottom
            0,1,2,
            0,2,3,
            //top
            4,6,5,
            4,7,6,
            //front
            8,9,10,
            8,10,11,
            //back
            12,13,14,
            12,14,15,
            //left
            16,17,18,
            16,18,19,
            //right
            20,21,22,
            20,22,23
        };

        if (right)
        {
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i];
                triangles[i] = triangles[i + 1];
                triangles[i + 1] = temp;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Vector3 normal = Vector3.Cross(mesh.vertices[21] - mesh.vertices[20], mesh.vertices[22] - mesh.vertices[20]).normalized;

        if (currGen < generations)
        {

            if (right)
                normal *= -1;

            BuildPoints bp = new BuildPoints
            {
                fp = mesh.vertices[20],
                lp = mesh.vertices[21],
                fn = normal,
                ln = normal
            };

            if (right)
                rBuilds.Add(bp);
            else
                lBuilds.Add(bp);
        }

        mf.mesh = mesh;
        mc.sharedMesh = mesh;

        mr.material = buildingMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        //debug lines
        if (debugLines)
        {
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
}
