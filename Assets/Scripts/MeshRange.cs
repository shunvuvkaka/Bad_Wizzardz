using UnityEngine;

public struct MeshRange
{
    public int vertexStart;
    public int vertexCount;
    public int triangleStart;
    public int triangleCount;

    public int xCount;
    public int zCount;
    public float lSpacing;
    public float rSpacing;
    public Vector3 rSideDir;
    public Vector3 lSideDir;
}