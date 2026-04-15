using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Chunk : MonoBehaviour
{
    public Bounds WorldBounds;
    public List<NavMeshBuildSource> CachedSources = new List<NavMeshBuildSource>();
    public void Build()
    {
        WorldBounds = new Bounds
        {
            center = transform.position,
            size = new Vector3(16, 100, 16)
        };

        ChunkNavMesh.Instance.RegisterChunk(this);
    }

    public void BuildSources(LayerMask navMeshLayer, List<NavMeshBuildMarkup> markups)
    {
        CachedSources.Clear();

        NavMeshBuilder.CollectSources(
            WorldBounds,
            navMeshLayer,
            NavMeshCollectGeometry.PhysicsColliders,
            0,
            markups,
            CachedSources
        );
    }

    void OnDisable()
    {
        if (ChunkNavMesh.Instance != null)
            ChunkNavMesh.Instance.UnregisterChunk(this);
    }
}