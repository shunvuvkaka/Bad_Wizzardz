using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Roofs : MonoBehaviour
{
    public float spacing;
    public Transform roofParent;
    public Material[] roofMats;
    public float maxHeight;
    public float inclination;
    public static Roofs Instance;
    public PhysicsMaterial roofPhys;
    [HideInInspector] public Dictionary<Vector2, GameObject> roofs = new Dictionary<Vector2, GameObject>();


    public void Awake()
    {
        Instance = this;
    }

    public void CreateRoofs(ref List<BasePoints> points, bool right)
    {
        int roofCount = points.Count;

        NativeArray<BasePoints> jobPoints = new NativeArray<BasePoints>(roofCount, Allocator.TempJob);
        NativeArray<MeshRange> ranges = new NativeArray<MeshRange>(roofCount, Allocator.TempJob);

        //computing the range for mesh creation to read from
        int totalVerts = 0;
        int totalTris = 0;

        for (int i = 0; i < roofCount; i++)
        {
            jobPoints[i] = points[i];

            var p = points[i];

            float lzDist = math.distance(p.bl, p.tl);
            float rzDist = math.distance(p.br, p.tr);
            float xDist = math.distance(p.bl, p.br);

            int xCount = (int)math.floor(xDist / spacing);
            int zCount = math.max((int)math.floor(lzDist / spacing), (int)math.floor(rzDist / spacing));

            float lSpacing = math.distance(p.bl, p.tl) / zCount;
            float rSpacing = math.distance(p.br, p.tr) / zCount;

            Vector3 lSideDir = (p.tl - p.bl).normalized;
            Vector3 rSideDir = (p.tr - p.br).normalized;

            int vertCount = (xCount + 1) * (zCount + 1);
            int triCount = xCount * zCount * 6;

            ranges[i] = new MeshRange
            {
                vertexStart = totalVerts,
                vertexCount = vertCount,
                triangleStart = totalTris,
                triangleCount = triCount,
                xCount = xCount,
                zCount = zCount,
                lSpacing = lSpacing,
                rSpacing = rSpacing,
                lSideDir = lSideDir,
                rSideDir = rSideDir
            };

            totalVerts += vertCount;
            totalTris += triCount;
        }

        //all verts and triangles for all meshes are stored in one array
        NativeArray<float3> allVertices = new NativeArray<float3>(totalVerts, Allocator.TempJob);
        NativeArray<int> allTriangles = new NativeArray<int>(totalTris, Allocator.TempJob);

        //scheduling job
        GenerateRoofs job = new GenerateRoofs
        {
            basePoints = jobPoints,
            ranges = ranges,
            spacing = spacing,
            vertices = allVertices,
            triangles = allTriangles,
            right = right,
            maxHeight = maxHeight,
            inclination = inclination
        };

        JobHandle handle = job.Schedule(roofCount, 32);
        handle.Complete();

        //building meshes
        for (int i = 0; i < roofCount; i++)
        {
            MeshRange range = ranges[i];

            GameObject go = new GameObject("Roof");
            go.transform.parent = roofParent;
            go.layer = 6;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            MeshCollider mc = go.AddComponent<MeshCollider>();

            Mesh mesh = new Mesh();

            Vector3[] verts = new Vector3[range.vertexCount];
            int[] tris = new int[range.triangleCount];

            for (int v = 0; v < range.vertexCount; v++)
                verts[v] = allVertices[range.vertexStart + v];

            for (int t = 0; t < range.triangleCount; t++)
                tris[t] = allTriangles[range.triangleStart + t] - range.vertexStart;

            roofs.Add(new Vector2(verts[0].x, verts[0].z), go);

            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            PlacementPoints.Instance.Propify(verts, go.transform, false);

            Material roofMat = roofMats[UnityEngine.Random.Range(0, roofMats.Length)];

            mf.mesh = mesh;
            mr.material = roofMat;
            mc.sharedMesh = mesh;
            mc.material = roofPhys;
            mc.convex = true;
        }

        //cleanup
        jobPoints.Dispose();
        ranges.Dispose();
        allVertices.Dispose();
        allTriangles.Dispose();

        points.Clear();
    }

    [BurstCompile]
    public struct GenerateRoofs : IJobParallelFor
    {
        [ReadOnly] public NativeArray<BasePoints> basePoints;
        [ReadOnly] public NativeArray<MeshRange> ranges;
        [ReadOnly] public float spacing;
        [ReadOnly] public bool right;
        [ReadOnly] public float maxHeight;
        [ReadOnly] public float inclination;
        [NativeDisableParallelForRestriction] public NativeArray<float3> vertices;
        [NativeDisableParallelForRestriction] public NativeArray<int> triangles;
        public void Execute(int index)
        {
            BasePoints p = basePoints[index];
            MeshRange range = ranges[index];

            int vertIndex = range.vertexStart;
            int triIndex = range.triangleStart;

            int xCount = range.xCount;
            int zCount = range.zCount;

            //generate vertices
            for (int z = 0; z <= zCount; z++)
            {
                float3 start = p.bl + range.lSpacing * z * range.lSideDir;
                float3 end = p.br + range.rSpacing * z * range.rSideDir;

                float xStep = math.distance(start, end) / xCount;
                float3 dir = math.normalize(end - start);


                for (int x = 0; x <= xCount; x++)
                {
                    float3 pos = start + x * xStep * dir;

                    pos += new float3(0, Height(z, x, zCount, xCount), 0);            

                    vertices[vertIndex++] = pos;
                }
            }

            //generate triangles
            if (right)
            {
                for (int z = 0; z < zCount; z++)
                {
                    for (int x = 0; x < xCount; x++)
                    {
                        int i = range.vertexStart + z * (xCount + 1) + x;
                    
                        triangles[triIndex++] = i;
                        triangles[triIndex++] = i + xCount + 1;
                        triangles[triIndex++] = i + 1;

                        triangles[triIndex++] = i + 1;
                        triangles[triIndex++] = i + xCount + 1;
                        triangles[triIndex++] = i + xCount + 2;
                    }
                }
            }
            else
            {
                for (int z = 0; z < zCount; z++)
                {
                    for (int x = 0; x < xCount; x++)
                    {
                        int i = range.vertexStart + z * (xCount + 1) + x;

                        triangles[triIndex++] = i;
                        triangles[triIndex++] = i + 1;
                        triangles[triIndex++] = i + xCount + 1;

                        triangles[triIndex++] = i + xCount + 1;
                        triangles[triIndex++] = i + 1;
                        triangles[triIndex++] = i + xCount + 2;
                    }
                }
            }
        }

        float Height(int z, int x, int zCount, int xCount)
        {
            float zMid = zCount / 2f;
            float xMid = xCount / 2f;

            float zHeight;
            float xHeight;

            if (z > zMid)
                zHeight = zCount - z * inclination;
            else
                zHeight = z * inclination;

            if (x > xMid)
                xHeight = xCount - x * inclination;
            else
                xHeight = x * inclination;
            
            xHeight = math.clamp(xHeight, 0.01f, maxHeight);
            zHeight = math.clamp(zHeight, 0.01f, maxHeight);

            return math.min(xHeight, zHeight);
        }
    }
}
