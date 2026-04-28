using System;

public interface ChunkObject
{ 
    public object GetObject() => GetType();
}

public struct BuildingChunk : ChunkObject
{
    public float height;

    public BuildingChunk(float h = 0)
    {
        height = h;
    }
}

public struct RoadChunk : ChunkObject
{
    public float height;

    public RoadChunk(float h = 0)
    {
        height = h;
    }
}

public struct EmptyChunk : ChunkObject
{

}

