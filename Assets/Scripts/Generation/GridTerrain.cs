using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Diagnostics;


//DOCS COMING SOON!!!
public class GridTerrain : MonoBehaviour
{
    public GridGenerator gridGenerator;
    public int chunkDivisions;
    public Material groundMat;
    private int spaceSize = 4;
    public Dictionary<Vector2Int, GameObject> activeGround = new Dictionary<Vector2Int, GameObject>();
    public static GridTerrain Instance;
    private Queue<Vector2Int> toGenerate = new Queue<Vector2Int>();
    private Stopwatch stopwatch = new Stopwatch();

    private const float TIME_BUDGET = 4;
    void Awake()
    {
        GridGenerator.OnNewChunks += CreateGround;

        Instance = this;
    }

    void OnDestroy()
    {
        GridGenerator.OnNewChunks -= CreateGround;
    }

    void Update()
    {
        if (gridGenerator == null)
            gridGenerator = GridGenerator.Instance;

        stopwatch.Start();

        while (toGenerate.Count > 0 && stopwatch.ElapsedMilliseconds < TIME_BUDGET)
        {
            Vector2Int coord = toGenerate.Dequeue();

            if (!gridGenerator.chunks.ContainsKey(coord))
                continue;

            if (activeGround.ContainsKey(coord))
                continue;

            GenerateMesh(coord);
        }

        stopwatch.Reset();
    }

    void CreateGround()
    {
        Vector2Int center = gridGenerator.playerChunk;

        List<Vector2Int> coords = new List<Vector2Int>();

        foreach (var kvp in gridGenerator.chunks)
        {
            Vector2Int coord = kvp.Key;

            if (activeGround.ContainsKey(coord))
                continue;

            coords.Add(coord);
        }

        coords.Sort((a, b) =>
        {
            int da = (a - center).sqrMagnitude;
            int db = (b - center).sqrMagnitude;
            return da.CompareTo(db);
        });

        foreach (var coord in coords)
        {
            if (!toGenerate.Contains(coord))
                toGenerate.Enqueue(coord);
        }
    }

    void GenerateMesh(Vector2Int coord)
    {
        TerrainData data = GenerateMeshData();

        GameObject go = Pool.Instance.GetPooledTerrain();

        go.transform.position = new Vector3(coord.x, 0, coord.y);

        go.transform.name = $"Chunk {coord}";

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        MeshFilter mf = go.GetComponent<MeshFilter>();
        MeshCollider mc = go.GetComponent<MeshCollider>();

        Mesh mesh = mf.mesh;
        
        mesh.SetVertices(data.vertices, 0, data.vertices.Length);
        mesh.triangles = data.triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        mf.mesh = mesh;
        mc.sharedMesh = mesh;

        mr.material = groundMat;

        activeGround.Add(coord, go);

        data.Dispose();
    }

    TerrainData GenerateMeshData()
    {
        int resolution = spaceSize + 1;
        TerrainData terrainData = new TerrainData(resolution);

        var terrainJob = new TerrainJob
        {
            resolution = resolution,
            height = 0,
            spaceSize = spaceSize,
            vertices = terrainData.vertices.Reinterpret<float3>(),
        };

        JobHandle handle = terrainJob.Schedule(resolution * resolution, 32);

        int triIndex = 0;

        for (int x = 0; x < chunkDivisions; x++)
        {
            for (int y = 0; y < chunkDivisions; y++)
            {
                int i = y * resolution + x;

                terrainData.triangles[triIndex++] = i;
                terrainData.triangles[triIndex++] = i + resolution;
                terrainData.triangles[triIndex++] = i + 1;
                
                terrainData.triangles[triIndex++] = i + 1;
                terrainData.triangles[triIndex++] = i + resolution;
                terrainData.triangles[triIndex++] = i + resolution + 1;
            }
        }

        handle.Complete();

        return terrainData;

    }
    //overkill atm, will prove useful for dynamic heght calculations

    [BurstCompile]
    public struct TerrainJob : IJobParallelFor
    {
        [ReadOnly] public int resolution;
        [ReadOnly] public float height;
        [ReadOnly] public int spaceSize;
        [WriteOnly] public NativeArray<float3> vertices;

        public void Execute(int index)
        {
            int x = index % resolution * spaceSize;
            int z = index / resolution * spaceSize;

            vertices[index] = new float3(x, height, z);
        }
    }
}
