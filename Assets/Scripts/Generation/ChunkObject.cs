/*
* File containing all the possible types that can be populated within the grid
* Contains base class ChunkObject
* And other types with relevant parameters
*/

using UnityEngine;

namespace BadWizards.ChunkData
{
//Base class for all chunk objects
public abstract class ChunkObject
{ 
    //Enumerator that contains all possible types
    public enum ChunkType
    {
        Empty,
        Road,
        Building
    }
    //Enumerator for the type of road chunk, determines what prefab and rotation algo to use
    public enum RoadIdentity
    {
        Straight,
        Bent,
        Intersection,
        End
    }
    //Value that should be read when querying what type a chunk is
    public ChunkType chunkType;
}
/// <summary>
/// Class for any chunk that should have a building
/// </summary>
public class BuildingChunk : ChunkObject
{
    //Currently unused
    public float height;
    //Bool for whether the chunk actually contains a building
    //Used for determining whether to populate a building on the chunk or not
    public bool occupied;

    //Constructor that sets default params
    public BuildingChunk()
    {
        chunkType = ChunkType.Building;
        occupied = false;
    }
}
/// <summary>
/// Class for any chunk that should have a road 
/// </summary>
public class RoadChunk : ChunkObject
{
    //Direction the road is going (0 if end)
    public Vector2Int nextDir {get; private set;}
    //Direction of split (if applicable)
    public Vector2Int branchDir {get; private set;}
    //Direction the road is coming from (0 if end)
    public Vector2Int currDir {get; private set;}
    //World space rotation of the road along y-axis in euler angles
    public int rotation {get; private set;}
    //Bool for whether the chunk is actually occupied
    public bool occupied;
    //Value that should be referenced when querying what the road type is (eg. straight, bent)
    public RoadIdentity identity;

    /// <summary>
    /// Constructor for a road chunk, takes in the direction the road is coming from, 
    /// the direction it's going to, and the direction of any split (if applicable)
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="next"></param>
    /// <param name="branch"></param>
    public RoadChunk(Vector2Int prev, Vector2Int next, Vector2Int branch)
    {
        //Assigning default values
        nextDir = next;
        branchDir = branch;
        chunkType = ChunkType.Road;
        occupied = false;
        currDir = prev;

        //Logic for determining road identity based on the directions of the road
        if (branch != Vector2Int.zero)
            identity = RoadIdentity.Intersection;
        else if (prev == Vector2Int.zero || next == Vector2Int.zero)
            identity = RoadIdentity.End;
        else if (prev != next)
            identity = RoadIdentity.Bent;
        else
            identity = RoadIdentity.Straight;

        //Deciding which algorithm to use for rotation determined by road identity
        //Then assigns the rotation to the object in euler angles
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
    //Straight logic
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
    //!!!NOT CURRENTLY FUNCTIONAL!!! End logic
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
    //Logic for bent roads (current direction != next direction)
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
    //Logic for roads that split
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

/// <summary>
/// Default object for any unassigned chunk
/// Should only be used as a placeholder
/// </summary>
public class EmptyChunk : ChunkObject
{
    public EmptyChunk()
    {
        chunkType = ChunkType.Empty;
    }
}
}

