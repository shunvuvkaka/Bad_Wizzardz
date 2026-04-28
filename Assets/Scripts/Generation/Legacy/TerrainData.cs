
using Unity.Collections;
using UnityEngine;

public struct TerrainData : System.IDisposable
{
    public NativeArray<Vector3> vertices;
    public NativeArray<Vector2> uvs;
    public int[] triangles;

    public TerrainData(int resolution)
    {
        int vertCount = resolution * resolution;
        int quadCount = (resolution - 1) * (resolution - 1);
        int triCount = quadCount * 6;

        vertices = new NativeArray<Vector3>(vertCount, Allocator.Persistent);
        uvs = new NativeArray<Vector2>(resolution * resolution, Allocator.Persistent);
        triangles = new int[triCount];
    }

    public void Dispose()
    {
        if (vertices.IsCreated) 
            vertices.Dispose();
        if (uvs.IsCreated) 
            uvs.Dispose();
    }
} 

