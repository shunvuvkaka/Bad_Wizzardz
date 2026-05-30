/*
* Uses the assigned grid data
* This script will attempt to place buildings with arbitrairy rotations with no overlap
* It will always find the most efficient way to populate the grid with given buildings
*/

using System.Collections.Generic;
using BadWizards.ChunkData;
using UnityEngine;

public class GridBuildings : MonoBehaviour
{
    [Tooltip("Array of all buildings that can be randomly placed on the grid")]
    public Building[] buildings;
    [Tooltip("Range of angles that a building can be randomly rotated to (e.g. -15 to 15)")]
    public Vector2Int angleRange;
    //Reference to the grid generator to access chunk data
    private GridGenerator gridGenerator;
    //Singleton instance for easy access from other scripts if needed
    public static GridBuildings Instance;
    //Dictionary containg each active coordinate and a reference to the relevant GameObject
    //Used to prevent overlap and for easy deletion
    public Dictionary<Vector2Int, GameObject> activeBuildings = new Dictionary<Vector2Int, GameObject>();
    [SerializeField]
    private bool debug;

    //Assigning references and subscribing to the event for when new chunks are generated
    void Awake()
    {
        GridGenerator.OnGenerateBuildings += GenerateBuildings;
        Instance = this;

        if (gridGenerator == null)
            gridGenerator = GridGenerator.Instance;
    }
    
    //Unsubscribing from event on deletion to prevet NullReference exceptions
    void OnDestroy()
    {
        GridGenerator.OnGenerateBuildings -= GenerateBuildings;
    }
    
    //Really makes sure we have a reference to grid generator
    void Update()
    {
        if (gridGenerator == null)
            gridGenerator = GridGenerator.Instance;
    }

    /// <summary>
    /// Main logic loop for building generation 
    /// Iterates through all chunks and attempts to place buildings on valid building chunks
    /// </summary>
    void GenerateBuildings()
    {
        //Iterating over all chunks
        foreach (var kvp in gridGenerator.chunks)
        {
            //Assigning values based on chunk data
            ChunkObject chunk = kvp.Value;
            Vector2Int coord = kvp.Key;

            BuildingChunk buildingChunk;

            //Visible iteration marker
            if (debug)
                Debug.DrawRay(new Vector3(coord.x, 5, coord.y), Vector3.up * 5, Color.orange);
            
            //Continues to next chunk if curent chunk is not assigned as building
            //If chunk is assigned as building, reads the chunk as a building type
            if (chunk.chunkType != ChunkObject.ChunkType.Building)
                continue;
            else
                buildingChunk = (BuildingChunk)chunk;

            //Continues to next chunk if its occupied \'-'/
            if (buildingChunk.occupied)
                continue;
            
            //Backup check to prevent overlap, continues if coord is occupied
            if (activeBuildings.ContainsKey(coord))
                continue;

            //Chooses what building to we will try and spawn
            Building buildingObject = SelectBuilding();

            //Assign internal space variable
            Vector2Int size = buildingObject.size;
            //Decide what direction will first be checked
            Vector2Int dir = BaseDirection();

            //Check to make sure the building can physically fit on the grid
            //Starts with the og direction, then check every other possible rotation until it finds one that works
            //The value remaining in {dir} is the rotation that was checked and found to be true
            //If all four primary directions are checked and none are valid, continue to next chunks
            if (!AuthenticateBuilding(size, ref dir, coord))
            {
                //Debug.Log("Failed Authentication");
                continue;
            }
            
            //Creating the physical building (could be pooled in future)
            GameObject building = Instantiate(buildingObject.building);

            //Assigning transform values to the new building based on the chunk data and the valid rotation
            building.transform.parent    =  transform;
            building.transform.position  =  SetPosition(dir, coord, gridGenerator.chunkSize);
            building.transform.rotation  =  ReinterperetDirection(dir) * RandomRotation();

            //Iterating over every chunk the new bulding occupies
            //Adds the building to the active dict with a reference to the gameobject
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int offset = ShiftDirection(x, y, dir) * gridGenerator.chunkSize;
                    Vector2Int checkPos = coord + offset;

                    activeBuildings.Add(checkPos, building);
                }
            }
        }
    }
    /// <summary>
    /// Authenticates whether a building can be placed at a given coordinate with a specific size and direction
    /// </summary>
    /// <param name="size"> The size of the building to authenticate </param>
    /// <param name="dir"> The first direction to check </param>
    /// <param name="coord"> The root coordinate </param>
    /// <returns> 
    /// <see langword="true"/> if the building can be placed, <see langword="false"/> otherwise
    /// Also writes {dir} as the first valid rotation found for the building
    /// </returns>
    bool AuthenticateBuilding(Vector2Int size, ref Vector2Int dir, Vector2Int coord)
    {
        //Iterating over all four possible rotations of the building, starting with the given direction
        for (int i = 0; i < 4; i++)
        {
            //Returns true if chunk is given rotation is checked to be true
            if (CheckChunk(size, dir, coord))
                return true;

            //Manipulateing the direction by 90 degrees to check the next rotation in the next loop
            dir = new Vector2Int(-dir.y, dir.x);
        }

        //No valid rotation was found after checking all four, returning false
        return false;
    }
    /// <summary>
    /// Checks specified direction with current size and coordinate
    /// </summary>
    /// <param name="size"> Size of the building to check </param>
    /// <param name="dir"> The direction to check </param>
    /// <param name="coord"> The root coordinate </param>
    /// <returns> <see langword="true"/> if placement is valid, <see langword="false"/> otherwise </returns>
    bool CheckChunk(Vector2Int size, Vector2Int dir, Vector2Int coord)
    {
        //Iterates over every chunk the building would occupy if placed with the given rotation
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                //Finding coordinate of the chunk to check
                Vector2Int offset = ShiftDirection(x, y, dir) * gridGenerator.chunkSize;
                Vector2Int checkPos = coord + offset;

                ChunkObject chunk;

                if (debug)
                    Debug.DrawRay(new Vector3(checkPos.x, 5, checkPos.y), Vector3.up * 5, Color.red);

                //If the building extends into an ungenerated chunk, return false
                if (!gridGenerator.chunks.TryGetValue(checkPos, out chunk))
                    return false;

                BuildingChunk buildingChunk;

                //If the chunk is not assigned as a building, return false
                //Else read the chunk as a building type
                if (chunk.chunkType != ChunkObject.ChunkType.Building)
                    return false;
                else
                    buildingChunk = (BuildingChunk)chunk;
                
                //If the chunk is already occupied, return false
                if (buildingChunk.occupied)
                    return false;
            }
        }

        //If everything is true, iterate over chunks once more to mark them as occupied
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                //Familiar grid reintepration tech
                Vector2Int offset = ShiftDirection(x, y, dir) * gridGenerator.chunkSize;
                Vector2Int checkPos = coord + offset;

                //We are aleady know for certain the indexed chunk is of type building, so we cast it directly
                BuildingChunk buildingChunk = (BuildingChunk)gridGenerator.chunks[checkPos];

                //Set chunk as occupied
                buildingChunk.occupied = true;
            }
        }

        if (!debug)
            return true;

        //Debug code
        Vector2Int testSize = ShiftDirection(size.x, size.y, dir) * gridGenerator.chunkSize;

        Vector3 testPos = SetPosition(dir, coord, gridGenerator.chunkSize);
        Vector2Int pos = new Vector2Int((int)testPos.x, (int)testPos.z);

        Debug.DrawLine(new Vector3(pos.x, 3, pos.y), new Vector3(pos.x + testSize.x, 3, pos.y), Color.darkBlue, 5f);
        Debug.DrawLine(new Vector3(pos.x + testSize.x, 3, pos.y), new Vector3(pos.x + testSize.x, 3, pos.y + testSize.y), Color.darkBlue, 5f);
        Debug.DrawLine(new Vector3(pos.x + testSize.x, 3, pos.y + testSize.y), new Vector3(pos.x, 3, pos.y + testSize.y), Color.darkBlue, 5f);
        Debug.DrawLine(new Vector3(pos.x, 3, pos.y + testSize.y), new Vector3(pos.x, 3, pos.y), Color.darkBlue, 5f);

        return true;
    }

    /// <summary>
    /// Simple random angle generator
    /// </summary>
    /// <returns><see cref="Vector2Int"/> in one of the 4 primary directions</returns>
    Vector2Int BaseDirection()
    {
        Vector2Int baseDir;

        switch (Random.Range(1, 5))
        {
            case 1:
                baseDir = Vector2Int.up;
                break;
            case 2:
                baseDir = Vector2Int.left;
                break;
            case 3:
                baseDir = Vector2Int.right;
                break;
            case 4:
                baseDir = Vector2Int.down;
                break;
            default:
                baseDir = Vector2Int.zero;
                break;
        }
        
        return baseDir;
    }

    /// <summary>
    /// Rotates a positive vector into desired direction
    /// </summary>
    /// <param name="x">Positive X magnitude</param>
    /// <param name="y">Positive Y magnitude</param>
    /// <param name="dir">Desired direction to rotate to</param>
    /// <returns><see cref="Vector2Int"/> representing the shifted direction</returns>
    Vector2Int ShiftDirection(int x, int y, Vector2Int dir)
    {
        Vector2Int baseDir;

        if (dir == Vector2Int.up)
            baseDir = new Vector2Int(x, y);
        else if (dir == Vector2Int.left)
            baseDir = new Vector2Int(-y, x);
        else if (dir == Vector2Int.down)
            baseDir = new Vector2Int(-x, -y);
        else
            baseDir = new Vector2Int(y, -x);

        return baseDir;
    }
    
    /// <summary>
    /// Converts normalized <see cref="Vector2Int"/> to <see cref="Quaternion"/> with corresponding rotation on the Y axis
    /// </summary>
    /// <param name="dir">Normalized Vector2Int</param>
    /// <returns><see cref="Quaternion"/> with desired rotation on the Y axis</returns>
    Quaternion ReinterperetDirection(Vector2Int dir)
    {
        Quaternion newDir;

        if (dir == Vector2Int.up)
            newDir = Quaternion.Euler(new Vector3(0, 0, 0)); 
        else if (dir == Vector2Int.left)
            newDir = Quaternion.Euler(new Vector3(0, -90, 0));
        else if (dir == Vector2Int.down)
            newDir = Quaternion.Euler(new Vector3(0, 180, 0));
        else
            newDir = Quaternion.Euler(new Vector3(0, 90, 0));
        
        return newDir;
    }
    
    /// <summary>
    /// Sets position as if the building was pivoted in the center of the chunk rather than the corner
    /// </summary>
    /// <param name="dir">Direction the building is facing</param>
    /// <param name="coord">Chunk coordinates</param>
    /// <param name="space">Size of the chunk</param>
    /// <returns>Vector3 world position of building</returns>
    Vector3 SetPosition(Vector2Int dir, Vector2Int coord, int space)
    {
        Vector3 newPos;

        if (dir == Vector2Int.up)
            newPos = new Vector3(coord.x, 0, coord.y); 
        else if (dir == Vector2Int.right)
            newPos = new Vector3(coord.x , 0, coord.y + space);
        else if (dir == Vector2Int.down)
            newPos = new Vector3(coord.x + space, 0, coord.y + space); 
        else
            newPos = new Vector3(coord.x + space, 0, coord.y); 
        
        return newPos;
    }

    /// <summary>
    /// Generates a random rotation within the assigned angle range for added visual variety
    /// </summary>
    /// <returns>Quaternion with Y axis rotation</returns>
    Quaternion RandomRotation() => Quaternion.Euler(new Vector3(0, Random.Range(angleRange.x, angleRange.y + 1), 0));

    /// <summary>
    /// Chooses a rnadom building
    /// </summary>
    /// <returns>Building from pre-determined list</returns>
    Building SelectBuilding() => buildings[Random.Range(0, buildings.Length)];
}
