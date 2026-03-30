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
            rebuildTimer -= Time.deltaTime;

            if (rebuildTimer <= 0f)
            {
                RebuildNavMesh(pendingBounds);
                hasPendingBounds = false;
            }
        }
    }

    //call when a chunk loads
    public void RegisterChunk(Chunk chunk)
    {
        if (activeChunks.Add(chunk))
        {
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
        //cancel previous build if still running
        if (currentBuildOperation != null && !currentBuildOperation.isDone)
        {
            currentBuildOperation = null;
        }

        sources.Clear();

        /*
        foreach (var chunk in activeChunks)
        {
            CollectSources(chunk.WorldBounds, sources);
        }
        */

        CollectSources(bounds, sources);

        currentBuildOperation = NavMeshBuilder.UpdateNavMeshDataAsync(
            navMeshData,
            buildSettings,
            sources,
            bounds
        );
    }

    //collect geometry from a chunk
    void CollectSources(Bounds bounds, List<NavMeshBuildSource> sources)
    {
        NavMeshBuilder.CollectSources(
        bounds,
        navMeshLayer,
        NavMeshCollectGeometry.PhysicsColliders,
        0,
        new List<NavMeshBuildMarkup>(),
        sources
    );
}
}