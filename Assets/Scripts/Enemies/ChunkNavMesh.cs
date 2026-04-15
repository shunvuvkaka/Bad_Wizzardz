using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class ChunkNavMesh : MonoBehaviour
{
    public static ChunkNavMesh Instance;

    [Header("Settings")]
    public LayerMask navMeshLayer;
    public float rebuildPadding = 2f;
    public float rebuildDelay = 0.2f;

    private NavMeshData navMeshData;
    private AsyncOperation currentBuildOperation;

    private List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
    private List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();

    private HashSet<Chunk> activeChunks = new HashSet<Chunk>();
    private Queue<Chunk> toBuild = new Queue<Chunk>();

    private Bounds pendingBounds;
    private bool hasPendingBounds;

    private float rebuildTimer;

    private NavMeshBuildSettings buildSettings;

    void Awake()
    {
        Instance = this;

        navMeshData = new NavMeshData();
        NavMesh.AddNavMeshData(navMeshData);

        buildSettings = NavMesh.GetSettingsByID(0);
    }

    void Update()
    {
        if (hasPendingBounds)
        {

            if (rebuildTimer <= 0f)
            {
                RebuildNavMesh(pendingBounds);
                hasPendingBounds = false;
            }
        }

        rebuildTimer -= Time.deltaTime;
    }

    //call when a chunk loads
    public void RegisterChunk(Chunk chunk)
    {
        if (activeChunks.Add(chunk))
        {
            markups.Clear();
            chunk.BuildSources(navMeshLayer, markups);

            QueueRebuild(chunk.WorldBounds);
        }
    }

    //call when a chunk unloads
    public void UnregisterChunk(Chunk chunk)
    {
        if (activeChunks.Remove(chunk))
        {
            QueueRebuild(chunk.WorldBounds);
        }
    }

    //queue rebuild instead of doing it instantly
    void QueueRebuild(Bounds bounds)
    {
        bounds.Expand(rebuildPadding);

        if (!hasPendingBounds)
        {
            pendingBounds = bounds;
            hasPendingBounds = true;
        }
        else
        {
            pendingBounds.Encapsulate(bounds);
        }

        rebuildTimer = rebuildDelay;
    }

    // rebuild
    void RebuildNavMesh(Bounds bounds)
    {
        // Avoid overlapping builds
        if (currentBuildOperation != null && !currentBuildOperation.isDone)
            return;

        sources.Clear();

        // Merge only relevant chunk sources
        foreach (var chunk in activeChunks)
        {
            if (chunk.WorldBounds.Intersects(bounds))
            {
                sources.AddRange(chunk.CachedSources);
            }
        }

        currentBuildOperation = NavMeshBuilder.UpdateNavMeshDataAsync(
            navMeshData,
            buildSettings,
            sources,
            bounds
        );
    }


}