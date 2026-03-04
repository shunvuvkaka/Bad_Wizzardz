using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Diagnostics;

public class TerrainGenerator : MonoBehaviour
{
    public Transform player;
    public int viewDistance = 3;
    public int chunkSize = 16;

    [Header("Generation")]
    public float height;
    public Material chunkMaterial;

    private Dictionary<Vector2Int, GameObject> activeChunks;
    private HashSet<Vector2Int> chunksBeingGenerated;
    private bool generating;
    private Queue<Vector2Int> chunksToGenerate = new Queue<Vector2Int>();
    private HashSet<Vector2Int> updatedChunks;
    private List<Vector2Int> toRemove;
    static readonly List<Vector3> roadPointsBuffer = new List<Vector3>();
    private HashSet<Vector2Int> queuedChunks;
    private bool isQuiting;
    private List<Vector2Int> candidateChunks = new List<Vector2Int>();
    private Stopwatch stopwatch= new Stopwatch();
    [SerializeField] private float targetFPS = 120f;
    [SerializeField] private float budget;


    void Awake()
    {
        Application.targetFrameRate = Mathf.RoundToInt(targetFPS);

        //0.5 means half of frame time will be allocated to terrain generation
        budget = 0.5f / targetFPS;

        int maxVisibleChunks = (viewDistance * 2 + 1);
        maxVisibleChunks *= maxVisibleChunks;

        activeChunks = new Dictionary<Vector2Int, GameObject>(maxVisibleChunks);
        updatedChunks = new HashSet<Vector2Int>(maxVisibleChunks);
        chunksBeingGenerated = new HashSet<Vector2Int>(maxVisibleChunks);
        queuedChunks = new HashSet<Vector2Int>(maxVisibleChunks);
        toRemove = new List<Vector2Int>(maxVisibleChunks);
    }

    void Update()
    {
        generating = true;

        stopwatch.Restart();

        if (isQuiting)
            return;

        //continues generating terrain unitl there is no more or budget esceeded
        while (stopwatch.Elapsed.TotalSeconds < budget && generating)
            GenerateTerrain();
        
        stopwatch.Stop();
    }

    void GenerateTerrain()
    {
        Vector2Int playerChunk = new Vector2Int(Mathf.FloorToInt(player.position.x / chunkSize), Mathf.FloorToInt(player.position.z / chunkSize));

        candidateChunks.Clear();

        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector2Int offset = new Vector2Int(x, z);
                if (offset.sqrMagnitude > viewDistance * viewDistance)
                    continue;

                Vector2Int coord = new Vector2Int(playerChunk.x + x, playerChunk.y + z);
                updatedChunks.Add(coord);

                if (!activeChunks.ContainsKey(coord) && !chunksBeingGenerated.Contains(coord) && !queuedChunks.Contains(coord))
                {
                    candidateChunks.Add(coord);
                }
            }
        }

        candidateChunks.Sort((a, b) =>
        {
            float da = (a - playerChunk).sqrMagnitude;
            float db = (b - playerChunk).sqrMagnitude;
            return da.CompareTo(db);
        });

        foreach (var coord in candidateChunks)
        {
            chunksBeingGenerated.Add(coord);
            queuedChunks.Add(coord);
            chunksToGenerate.Enqueue(coord);
        }


        foreach (var kvp in activeChunks)
        {
            if (!updatedChunks.Contains(kvp.Key))
            {
                kvp.Value.SetActive(false);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            activeChunks.Remove(key);
        }

        if (chunksToGenerate.Count > 0)
        {
            Vector2Int coord = chunksToGenerate.Dequeue();
            queuedChunks.Remove(coord);

            var meshData = GenerateMeshDataJob(coord);
            CreateChunkObject(coord, meshData);
            chunksBeingGenerated.Remove(coord);
            meshData.Dispose();
        }
        else
        {
            generating = false;
        }

        updatedChunks.Clear();
        toRemove.Clear();
    }

    TerrainData GenerateMeshDataJob(Vector2Int coord)
    {
        int resolution = chunkSize + 1;
        var meshData = new TerrainData(resolution);

        var terrainJob = new TerrainJob
        {
            resolution = resolution,
            height = height,
            vertices = meshData.vertices.Reinterpret<float3>(),
        };

        JobHandle handle = terrainJob.Schedule(resolution * resolution, 32);


        int triIndex = 0;
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int i = z * resolution + x;

                meshData.triangles[triIndex++] = i;
                meshData.triangles[triIndex++] = i + resolution;
                meshData.triangles[triIndex++] = i + 1;

                meshData.triangles[triIndex++] = i + 1;
                meshData.triangles[triIndex++] = i + resolution;
                meshData.triangles[triIndex++] = i + resolution + 1;
            }
        }

        handle.Complete();

        return meshData;
    }

    void CreateChunkObject(Vector2Int coord, TerrainData data)
    {
        GameObject chunk = Pool.Instance.GetPooledTerrain();
        chunk.transform.name = $"Chunk {coord.x} {coord.y}";
        chunk.transform.position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        MeshRenderer mr = chunk.GetComponent<MeshRenderer>();
        MeshFilter mf = chunk.GetComponent<MeshFilter>();
        mr.material = chunkMaterial;

        Mesh mesh = new Mesh();

        mesh.SetVertices(data.vertices, 0, data.vertices.Length);
        mesh.triangles = data.triangles;
        mesh.RecalculateNormals();

        mf.mesh = mesh;
        activeChunks[coord] = chunk;
    }

    void OnApplicationQuit()
    {
        chunksToGenerate.Clear();
    }


    [BurstCompile]
    public struct TerrainJob : IJobParallelFor
    {
        [ReadOnly] public int resolution;
        [ReadOnly] public float height;
        [WriteOnly] public NativeArray<float3> vertices;

        public void Execute(int index)
        {
            int x = index % resolution;
            int z = index / resolution;

            vertices[index] = new float3(x, height, z);
        }
    }
}


