using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct MeshData : System.IDisposable
{
    public NativeArray<float3> vertices;
    public NativeArray<Vector2> uvs;
    public NativeArray<int> triangles;

    public MeshData(int verts, int quads)
    {
        int triCount = quads * 6;

        vertices = new NativeArray<float3>(verts, Allocator.Persistent);
        uvs = new NativeArray<Vector2>(verts, Allocator.Persistent);
        triangles = new NativeArray<int>(triCount, Allocator.Persistent);
    }

    public void Dispose()
    {
        if (vertices.IsCreated) 
            vertices.Dispose();
        if (uvs.IsCreated) 
            uvs.Dispose();
        if (triangles.IsCreated) 
            triangles.Dispose();
    }
} 