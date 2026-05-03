using UnityEngine;

namespace BadWizards.ChunkData {

public abstract class ChunkObject
{ 
    public enum ChunkType
    {
        Empty,
        Road,
        Building
    }

    public ChunkType chunkType;
}

public class BuildingChunk : ChunkObject
{
    public float height;
    public bool occupied;

    public BuildingChunk()
    {
        chunkType = ChunkType.Building;
        occupied = false;
    }
}

public class RoadChunk : ChunkObject
{
    public Vector2Int nextDir;
    public Vector2Int branchDir;
    public enum RoadIdentity
    {
        Straight,
        Bent,
        Intersection,
        End
    }

    public RoadIdentity identity {get; protected set;}

    public RoadChunk(Vector2Int prev, Vector2Int next, Vector2Int branch)
    {
        nextDir = next;
        branchDir = branch;
        chunkType = ChunkType.Road;

        if (branch != Vector2Int.zero)
            identity = RoadIdentity.Intersection;
        else if (prev == Vector2Int.zero || next == Vector2Int.zero)
            identity = RoadIdentity.End;
        else if (prev != next)
            identity = RoadIdentity.Bent;
        else
            identity = RoadIdentity.Straight;
    }
}

public class EmptyChunk : ChunkObject
{
    public EmptyChunk()
    {
        chunkType = ChunkType.Empty;
    }
}
}

