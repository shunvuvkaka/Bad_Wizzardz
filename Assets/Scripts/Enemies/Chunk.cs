using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Bounds WorldBounds;

    public void Build()
    {
        WorldBounds = new Bounds
        {
            center = transform.position,
            size = new Vector3(16, 100, 16)
        };

        ChunkNavMesh.Instance.RegisterChunk(this);
    }

    void OnDisable()
    {
        if (ChunkNavMesh.Instance != null)
            ChunkNavMesh.Instance.UnregisterChunk(this);
    }
}