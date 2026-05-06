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
    public Vector2Int currDir;
    public int rotation;
    public enum RoadIdentity
    {
        Straight,
        Bent,
        Intersection,
        End
    }
    public bool occupied;

    public RoadIdentity identity;

    public RoadChunk(Vector2Int prev, Vector2Int next, Vector2Int branch)
    {
        nextDir = next;
        branchDir = branch;
        chunkType = ChunkType.Road;
        occupied = false;
        currDir = prev;

        if (branch != Vector2Int.zero)
            identity = RoadIdentity.Intersection;
        else if (prev == Vector2Int.zero || next == Vector2Int.zero)
            identity = RoadIdentity.End;
        else if (prev != next)
            identity = RoadIdentity.Bent;
        else
            identity = RoadIdentity.Straight;

        switch (identity)
        {
            case RoadIdentity.Intersection:
                rotation = JunctionRotation(currDir, branchDir, nextDir);
                break;
            case RoadIdentity.Straight:
                rotation = StraightRotation(currDir);
                break;
            case RoadIdentity.Bent:
                rotation = BentRotaion(currDir, nextDir);
                break;
            case RoadIdentity.End:
                rotation = EndRotation(nextDir, currDir);
                break;
        }
    }

    int StraightRotation(Vector2Int currDir)
    {
        int rotation;

        if (currDir == Vector2Int.down)
            rotation = 180;
        else if (currDir == Vector2Int.right)
            rotation = 90;
        else if (currDir == Vector2Int.up)
            rotation = 0;
        else
            rotation = -90;

        return rotation;
    }
    int EndRotation(Vector2Int nextDir, Vector2Int currDir)
    {
        int rotation;

        if (nextDir != Vector2Int.zero)
        {
            if (nextDir == Vector2Int.down)
                rotation = 0;
            else if (nextDir == Vector2Int.right)
                rotation = -90;
            else if (nextDir == Vector2Int.up)
                rotation = 180;
            else
                rotation = 90;
        }
        else
        {
            if (currDir == Vector2Int.down)
                rotation = 0;
            else if (currDir == Vector2Int.right)
                rotation = 90;
            else if (currDir == Vector2Int.up)
                rotation = 180;
            else
                rotation = -90;
        }

        return rotation;
    }

    int BentRotaion(Vector2Int currDir, Vector2Int nextDir)
    {
        int rotation;

        if (currDir == Vector2Int.up && nextDir == Vector2Int.right
            || currDir == Vector2Int.left && nextDir == Vector2Int.down)
            rotation = 0;
        else if (currDir == Vector2Int.up && nextDir == Vector2Int.left
                || currDir == Vector2Int.right && nextDir == Vector2Int.down)
            rotation = 90;
        else if (currDir == Vector2Int.down && nextDir == Vector2Int.left
                || currDir == Vector2Int.right && nextDir == Vector2Int.up)
            rotation = 180;
        else 
            rotation = -90;
        

        return rotation;
    }

    //ts looks like one of those thumbnails for code reviews of games with bad code with a caption like "MASSIVE logic BOMBS!!!!"
    int JunctionRotation(Vector2Int currDir, Vector2Int shiftDir, Vector2Int nextDir)
    {
        int rotation;

        if ((currDir == Vector2Int.up && shiftDir == Vector2Int.right && nextDir == Vector2Int.up)
            || (currDir == Vector2Int.left && (shiftDir == Vector2Int.down || shiftDir == Vector2Int.up) && nextDir != Vector2Int.left)
            || (currDir == Vector2Int.down && shiftDir == Vector2Int.right && nextDir == Vector2Int.down))
            rotation = 0;
        else if ((currDir == Vector2Int.right && shiftDir == Vector2Int.down && nextDir == Vector2Int.right)
            || (currDir == Vector2Int.up && (shiftDir == Vector2Int.left || shiftDir == Vector2Int.right) && nextDir != Vector2Int.up)
            || (currDir == Vector2Int.left && shiftDir == Vector2Int.down && nextDir == Vector2Int.left))
            rotation = 90;
        else if ((currDir == Vector2Int.up && shiftDir == Vector2Int.left && nextDir == Vector2Int.up)
            || (currDir == Vector2Int.right && (shiftDir == Vector2Int.up || shiftDir == Vector2Int.down) && nextDir != Vector2Int.right)
            || (currDir == Vector2Int.down && shiftDir == Vector2Int.left && nextDir == Vector2Int.down))
            rotation = 180;
        else 
            rotation = -90;
        

        return rotation;
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

