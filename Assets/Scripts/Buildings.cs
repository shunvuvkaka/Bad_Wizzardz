using System.Collections.Generic;
using UnityEngine;

public class Buildings : MonoBehaviour
{
    [Header("Building Params")]
    public float roadDist = 1;
    public float maxDist = 30;
    public float minBreadth = 5;
    public Material buildingMat;
    [Header("Building Size and Spacing")]
    public Vector2Int widthRange = new Vector2Int(5, 25);
    public Vector2Int spaceRange = new Vector2Int(1, 2);
    public Vector2 heightRange = new Vector2(5, 25);
    [Header("Generations")]
    public int generations = 5;
    public float buildingGap = 2;
    public Vector2 generationScope = new Vector2(0.7f, -0.7f);
    [Header("References")]
    [SerializeField] private Transform buildingParent; //parent object buildings should be assigned to

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
        //first preperatuins and referneces
        endBuffer = widthRange.y + spaceRange.y + 1;
        road = Road.Instance;
        roadDist += road.roadWidth / 2;
    }

    void LateUpdate()
    {
        //copy values from road script
        points = road.pointDict;
        normals = road.normalDict;

        //loop over the unchecked indexes on left and right sides creating initial buildings
        while (rIndex < road.globalIndex - endBuffer)
        {
            //rIndex is passed as reference type to be index based on building size and spacing
            InitialBuildings(ref rIndex, true);
        }

        while (lIndex < road.globalIndex - endBuffer)
        {
            //see previous comment
            InitialBuildings(ref lIndex, false);
        }

        //for each generation, create generative buildings
        if (rBuilds.Count > 0)
        {
            while (currGen < generations)
            {
                currGen++;

                //create branching buildings on both sides with references to the buildpoint list the require
                GenerativeBuildings(ref rBuilds, true);
                GenerativeBuildings(ref lBuilds, false);
            }
        }

        //reset generation for next frame
        currGen = 0;
        
    }
    void InitialBuildings(ref int index, bool right)
    {
        //randomly decide width, space and height
        int width = Mathf.RoundToInt(Random.Range(widthRange.x, widthRange.y));
        int space = Mathf.RoundToInt(Random.Range(spaceRange.x, spaceRange.y));
        float height = Random.Range(heightRange.x, heightRange.y);

        BuildPoints build = new BuildPoints
        {
            //create a new buildpoint based off road (note lp shares same y value as fp keeping them from slanting)
            fp = points[index],
            lp = new Vector3(points[index + width].x, points[index].y, points[index + width].z)
        };

        //same as points but for normals
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

        //break if building is too small/invalid
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

        //actually constructing the mesh
        ConstructBuilding(basePoints, height, right);

        //debug
        if (debugLines)
        {
            Debug.DrawRay(build.fp, build.fn * backDist, impoactCol, float.MaxValue);
            Debug.DrawRay(build.lp, build.ln * backDist, impoactCol, float.MaxValue);
        }

        //index the index by width and spacing
        index = index + width + space;
    }

    void GenerativeBuildings(ref List<BuildPoints> prevBuilds, bool right)
    {
        //copy the buildpoint list to an array and clear it to prevent one building from having multiple generations
        BuildPoints[] builds = new BuildPoints[prevBuilds.Count];
        prevBuilds.CopyTo(builds);
        prevBuilds.Clear();

        //iterating over each buildpoint in the array
        for (int j = 0; j < builds.Length; j++)
        {
            BuildPoints build = builds[j];

            //define original values before optimal search has began
            Vector3 normal = build.fn;
            Vector3 bfn = build.fn;
            Vector3 bln = build.fn;

            //random height within scope
            float height = Random.Range(heightRange.x, heightRange.y);

            //preset mins
            float backDist = 0;
            float maxArea = 0;

            //iterates between the minimum and max scope for searching the best building
            for (float i = generationScope.x; i > generationScope.y; i -= 0.05f)
            {
                //mutates the normals to become wider or narrower based on current search
                build.fn = (normal - new Vector3(i, 0, i)).normalized;
                build.ln = (normal + new Vector3(i, 0, i)).normalized;

                //calulating farthest edge distance
                float dist = CalculateDistance(build);

                //continue to next search attempt if building is guaranteed to be invalid
                if (dist - buildingGap < minBreadth)
                {
                    Debug.Log(i + " too high " + dist);
                    continue;
                }

                //calculating area of trapezium created from current values
                Vector3 q = build.fp + build.fn * dist;
                Vector3 t = build.lp + build.ln * dist;
                Vector3 p = build.fp + normal * dist;
                Vector3 qp = (p - q).normalized;

                //note that 1 - dot is used the same here as it is in intersection distance
                float dot = 1 - Vector3.Dot(qp, normal);
                float length = Vector3.Distance(build.fp, p) * dot;
                float a = Vector3.Distance(build.fp, build.lp);
                float b = Vector3.Distance(q, t);

                //simple formula for trapezium earlier, finally some 3rd grade math lol
                float area = length * 0.5f * (a + b);

                //recording paramaters for building with largest area (most efficient building)
                if (area > maxArea)
                {
                    backDist = dist;
                    maxArea = area;
                    bln = build.ln;
                    bfn = build.fn;
                }
            }

            //continue to next buildpoint set if all searches are invalid, killing this branch
            if (backDist == 0)
                continue;

            //set normals to best calculated ones
            build.fn = bfn;
            build.ln = bln;

            //creating points that will be converted to mesh
            BasePoints basePoints = new BasePoints
            (
                build.fp + build.fn * backDist,
                build.lp + build.ln * backDist,
                build.fp + build.fn * buildingGap,
                build.lp + build.ln * buildingGap
            );

            //debug
            if (debugLines)
            {
                Debug.DrawRay(build.fp, build.fn * backDist, impoactCol, float.MaxValue);
                Debug.DrawRay(build.lp, build.ln * backDist, impoactCol, float.MaxValue);
            }

            //constructing the mesh
            ConstructBuilding(basePoints, height, right);
        }
    }

    //helper function that returns how far the distance is, combining multiple checks
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

        //"1 - ..." as if road point is perpendicular already perfectly aligned
        float pDot = 1 - Vector2.Dot(pqDir, r);
        float qDot = 1 - Vector2.Dot(qpDir, s);

        //move along length in normal direction from point for intersection
        Vector2 intersection = p + t * r;

        //find and record the minimum length to intersection * disposition
        float distA = Vector2.Distance(p, intersection) * pDot;
        float distB = Vector2.Distance(q, intersection) * qDot;

        float min = Mathf.Min(distA, distB);

        //if the intersection is closer than max distance, return intersection distance
        if (min < maxDist + roadDist)
        {
            impoactCol = Color.blue;
            return min;
        }

        //if the intersection is too far away, just return max distance
        return maxDist + roadDist;
    }

    float RaycastDistance(BuildPoints build)
    {
        RaycastHit pHit;
        RaycastHit qHit;
    
        //move the raycast points slightly upwards to enseure they are not in line with other colliders
        build.fp += Vector3.up * 0.1f;
        build.lp += Vector3.up * 0.1f;

        //raycast from both points
        Physics.Raycast(build.fp, build.fn, out pHit, maxDist + roadDist);
        Physics.Raycast(build.lp, build.ln, out qHit, maxDist + roadDist);

        float min;

        //checking what rays hit and returning:
        //max distance if none hit,
        //the distance of the one ray that hit,
        //or the minimum distance of both rays if they both hit
        if (pHit.collider == null && qHit.collider == null)
            return maxDist + roadDist;
        else if (pHit.collider == null)
            min = qHit.distance;
        else if (qHit.collider == null)
            min = pHit.distance;
        else
            min = Mathf.Min(pHit.distance, qHit.distance);

        impoactCol = Color.red;

        //subtracting by desired gap between generative buildings
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
        
        //creating a new gameobject for each building
        GameObject go = new GameObject($" Building {rIndex}");
        go.transform.parent = buildingParent;

        //adding components neccessary for collision and mesh rendering
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        MeshCollider mc = go.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();

        //manually creating the mesh due to the complexity of 3D objects and the feablness of my mind
        //note that unlike my previous approach, this one has no triangles sharing vertices = consistent normals
        //also means there is 24 verts but performance impact is itty bitty
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

        //reversing triangle winding if on right side to prevent meshes being inside out
        if (right)
        {
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i];
                triangles[i] = triangles[i + 1];
                triangles[i + 1] = temp;
            }
        }

        //assigning values to mesh a calulating all numbers
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        //calulating the normal of the triangle on the the face facing away from the road
        Vector3 normal = Vector3.Cross(mesh.vertices[21] - mesh.vertices[20], mesh.vertices[22] - mesh.vertices[20]).normalized;

        //actually adds this building to the list that will later be used to create new generations of buildings
        if (currGen < generations)
        {
            //flip normal if right
            if (right)
                normal *= -1;

            //create a new buildpoints just like what was done for the road, but now with the meshes values instead!!!
            BuildPoints bp = new BuildPoints
            {
                fp = mesh.vertices[20],
                lp = mesh.vertices[21],
                fn = normal,
                ln = normal
            };

            //chooose which list to add to
            if (right)
                rBuilds.Add(bp);
            else
                lBuilds.Add(bp);
        }

        //assign values to components
        mf.mesh = mesh;
        mc.sharedMesh = mesh;

        mr.material = buildingMat;
        //turn off shadows
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
