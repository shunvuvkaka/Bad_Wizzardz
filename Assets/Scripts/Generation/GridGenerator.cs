using System.Collections.Generic;
using UnityEngine;
using System;
using BadWizards.ChunkData;

public class GridGenerator : MonoBehaviour
{
    [Header("Main")]
    [Tooltip("View distance in world units, should be a multiple of chunk size for best results")]
    public int viewDistance;
    [Tooltip("Building generation distance in world units, should be a multiple of chunk size for best results")]
    public int buildingDistance;
    [Tooltip("Size of each chunk in world units")]
    public int chunkSize;
    [Tooltip("Reference to player")]
    public Transform player;
    [Header("Road")]
    [Tooltip("Fractional chance the road turns left or right")]
    [Range(0, 1)]
    public float variationChance;
    [Tooltip("Fractional chance the road splits into two")]
    [Range(0, 1)]
    public float splitChance;
    [Tooltip("Fractional chance the road ends")]
    [Range(0, 1)]
    public float deathChance;
    [Tooltip("Minimum number of active road heads at a time, higher values will result in more roads but may cause stuttering")]
    public int minHeads;
    [Header("Buildings")]
    [Tooltip("Number of iterations to run building population on when new chunks are generated, higher values will result in more dense building population but may cause stuttering")]
    public int buildingIterations;
    [Header("Debug")]
    //Current chun
    public Vector2Int playerChunk;
    //Dictionary containg all active chunks, indexed by their coordinates in world space (not grid space ik its stupid)
    public Dictionary<Vector2Int, ChunkObject> chunks = new Dictionary<Vector2Int, ChunkObject>();
    //Dictionary containg all road heads chunks, indexed by their coordinates in world space (not grid space ik its stupid)
    public Dictionary<Vector2Int, DirectionPair> roadHeads {get; private set;} = new Dictionary<Vector2Int, DirectionPair>();
    //Hashset of active chunk coordinates for easy lookup
    public HashSet<Vector2Int> activeChunks = new HashSet<Vector2Int>();
    //Hashset of chunk coordinates that need to be removed, cleared every frame
    private HashSet<Vector2Int> toRemove = new HashSet<Vector2Int>();
    //Hashset of road chunk coordinates that need to be generated, cleared every frame
    private HashSet<Vector2Int> newRoads = new HashSet<Vector2Int>();
    //Event called when new chunks are generated
    public static Action OnNewChunks;
    //Event called when new buildings should be generated, separate from OnNewChunks to allow for less frequent building population which is more expensive than road population
    public static Action OnGenerateBuildings;

    //Singleton instance for easy access from other scripts
    public static GridGenerator Instance;

    //Limit for recursive road population to prevent infinite loops in edge cases, may cause some gen issues if hit but better than crashing
    //Probs will never be hit unless something has gone very wrong
    private const int MAX_DEPTH = 128;

    //Recursion counter
    private int depth;
    //COunter for when to generate buildings
    private int buildingCount;
    
    //Enum for returning multiple values from road population function
    enum ChunkState
    {
        Invalid,
        Empty,
        Populated
    }
    //Init
    void Awake()
    {
        buildingCount = 0;
        //ensures viiewDistance is a multiple of chunkSize

        //Initial road head, ensures there is always at least one road head to expand from
        //Think of it like planting a seed
        roadHeads.Add(new Vector2Int(0, 0), BaseDirection());

        int mod = viewDistance % chunkSize;
        viewDistance -= mod;
        buildingDistance -= mod;

        Instance = this;
    }

    //Main loop
    void Update()
    {
        //Reset recursion counter
        depth = 0;

        //Calculates player chunk by dividing player position by chunk size and flooring it, then multiplying back by chunk size to get world space coordinates of the chunk the player is in
        playerChunk = new Vector2Int(Mathf.FloorToInt(player.position.x / chunkSize), Mathf.FloorToInt(player.position.z / chunkSize)) * chunkSize;

        //Only executes if new chunks where generated
        if (GenerateChunks())
        {
            //Decreases building counter
            buildingCount--;
            //Invoke event
            OnNewChunks?.Invoke();

            //Populate chunks
            PopulateRoadChunks();
            PopulateBuildingChunks();

            //Generate buildings if counter is below zero
            if(buildingCount < 0)
            {
                OnGenerateBuildings?.Invoke();
                buildingCount = buildingIterations;
            }

        }
        //pretty self explanatory

        RemoveChunks();

        DebugLines();
    }

    /// <summary>
    /// Generates new chunks within the view distance and adds them to the active chunk list, also adds new road heads as needed
    /// </summary>
    /// <returns><see langword="true"/> if new chunks were generated, <see langword="false"/> otherwise</returns>
    bool GenerateChunks()
    {
        bool newChunks = false;

        //Iterate through all coordinates within view distance
        for (int x = -viewDistance; x <= viewDistance; x += chunkSize)
        {
            for (int z = -viewDistance; z <= viewDistance; z += chunkSize)
            {
                Vector2Int coord = new Vector2Int(playerChunk.x + x, playerChunk.y + z);

                //If the coordinate is not already in the active chunk list, add it and create a new empty chunk at that coordinate
                if (!activeChunks.Contains(coord))
                {
                    newChunks = true;
                    activeChunks.Add(coord);
                    chunks.Add(coord, new EmptyChunk());
                }

                //Additionally, try to add a new road head at the edge of the map if there is not enough
                if (roadHeads.Count < minHeads)
                {
                    roadHeads.TryAdd(coord, BaseDirection());
                }
            }
        }

        return newChunks;
    }

    /// <summary>
    /// Removes chunks outside of the view distance
    /// </summary>
    void RemoveChunks()
    {
        toRemove.Clear();

        //Iterate through all active chunks, marking ones outside of view distance for removal
        foreach(Vector2Int coord in activeChunks)
        {   
            //no sqrt bc 1D aura
            if (Mathf.Abs(coord.x - playerChunk.x) > viewDistance
                || Mathf.Abs(coord.y - playerChunk.y) > viewDistance)
            {
                toRemove.Add(coord);
            }
        }

        //Iterate through all marked chunks
        //Done in separate loop to avoid modifying collection while iterating
        foreach(Vector2Int coord in toRemove)
        {   
            activeChunks.Remove(coord);
            chunks.Remove(coord);
            
            //Remove ground, building, and road gameobjects if they exist, also removes road heads if they exist
            if (GridTerrain.Instance.activeGround.TryGetValue(coord, out GameObject go))
            {
                go.SetActive(false);
                GridTerrain.Instance.activeGround.Remove(coord);
            }

            if (GridBuildings.Instance.activeBuildings.TryGetValue(coord, out go))
            {
                Destroy(go);
                GridBuildings.Instance.activeBuildings.Remove(coord);
            }

            if (GridRoads.Instance.activeRoads.TryGetValue(coord, out go))
            {
                Destroy(go);
                GridRoads.Instance.activeRoads.Remove(coord);
            }

            roadHeads.Remove(coord);
        }
    }
    /// <summary>
    /// Populates road chunks by iterating through all road heads and attempting to advance the road in its current direction, if that fails it will try to turn left or right, if all options are blocked it will be removed as a road head, new road heads are added to the list of new roads to be populated on the next iteration, this process is recursive until there are no new roads to add or the maximum depth is reached
    /// </summary>
    void PopulateRoadChunks()
    {
        newRoads.Clear();

        //Convert road heads to arrays for iteration, this is done to avoid modifying the collection
        Vector2Int[] roadHeadCoordArray = new Vector2Int[roadHeads.Count];
        DirectionPair[] roadHeadDirArray = new DirectionPair[roadHeads.Count];

        //Index tracker
        int i = 0;

        //Iterating by kvp for ease of access
        foreach (var kvp in roadHeads)
        {
            roadHeadCoordArray[i] = kvp.Key;
            roadHeadDirArray[i] = kvp.Value;

            i++;
        }

        //Iterate through all road heads, attempting to advance the road and adding new road heads
        for (int j = 0; j < roadHeadCoordArray.Length; j++)
        {
            //Current road head data
            Vector2Int coord = roadHeadCoordArray[j];
            Vector2Int baseDir = roadHeadDirArray[j].baseDir;
            Vector2Int prevDir = roadHeadDirArray[j].prevDir;

            //Continues until the road can no longer be advanced
            while (true)
            {
                if (!AdvanceRoad(ref coord, baseDir, prevDir, out prevDir))
                    break;
            }
        }

        //Iterate over all the new road heads that were added via recursion
        if (newRoads.Count > 0 && depth < MAX_DEPTH)
        {
            //recursion limited by depth, may screw up some gen if broken because of depth, but better than crash
            PopulateRoadChunks();
            depth++;
        }
    }

    /// <summary>
    /// Assigns building chunks to all empty chunks within the building distance
    /// </summary>
    void PopulateBuildingChunks()
    {
        //Iterate through all coordinates within building distance
        for (int x = -buildingDistance; x <= buildingDistance; x += chunkSize)
        {
            for (int z = -buildingDistance; z <= buildingDistance; z += chunkSize)
            {
                Vector2Int coord = new Vector2Int(playerChunk.x + x, playerChunk.y + z);

                //if this executes, something has gone very wrong
                if (!activeChunks.Contains(coord))
                    continue;

                ChunkObject chunkObject = chunks[coord];

                //Makes chunk a building one
                if (chunkObject.chunkType == ChunkObject.ChunkType.Empty)
                {
                    chunks[coord] = new BuildingChunk();
                }
            }
        }
    }
    bool AdvanceRoad(ref Vector2Int coord, Vector2Int baseDir, Vector2Int prevDir, out Vector2Int currDir)
    {
        ChunkObject chunkObject;

        Vector2Int newDir;

        ChunkState result;

        currDir = Vector2Int.zero;

        //this line is where we can set chance variations by setting dir to some other value of baseDir
        Vector2Int dir = DirectionShift(baseDir, variationChance);

        if (CheckChunk(dir, coord, out chunkObject, out result) != ChunkState.Populated)
        {
            newDir = dir;

            if (result == ChunkState.Invalid)
                return false;
        }
        else if (CheckChunk(new Vector2Int (-dir.y, dir.x), coord, out chunkObject, out result) != ChunkState.Populated)
        {
            newDir = new Vector2Int (-dir.y, dir.x);

            if (result == ChunkState.Invalid)
                return false;
        }
        else if (CheckChunk(new Vector2Int (dir.y, -dir.x), coord, out chunkObject, out result) != ChunkState.Populated)
        {
            newDir = new Vector2Int (dir.y, -dir.x);

            if (result == ChunkState.Invalid)
                return false;
        }
        else
        {
            return false;
        }
    
        roadHeads.Remove(coord);

        Vector2Int shiftDir = Vector2Int.zero;

        if (UnityEngine.Random.value < splitChance)
        {
            newRoads.Add(coord);
            shiftDir = DirectionShift(baseDir, 1);
            roadHeads.Add(coord, new DirectionPair(shiftDir, prevDir));
        }

        if (UnityEngine.Random.value < deathChance)
        {
            if (chunks[coord] is not RoadChunk)
                chunks[coord] = new RoadChunk(prevDir, Vector2Int.zero, shiftDir);
            
            return false;
        }

        if (chunks[coord] is not RoadChunk)
            chunks[coord] = new RoadChunk(prevDir, newDir, shiftDir);

        coord += newDir * chunkSize;
        currDir = newDir;

        roadHeads.TryAdd(coord, new DirectionPair(baseDir, currDir));

        return true;
    }

    /// <summary>
    /// Randomly shifts the direction of the road based on the assigned variation chance, this is what creates more natural looking roads that don't just go straight forever, it will randomly turn left or right based on the variation chance, if the chance is set to 0.5, there is an equal chance to turn left, right, or go straight, if the chance is set to 0.25, there is a 25% chance to turn left, 25% chance to turn right, and 50% chance to go straight, etc
    /// </summary>
    /// <param name="baseDir">The original direction of the road head</param>
    /// <param name="chance">The fractional chance to turn left or right</param>
    /// <returns><see cref="Vector2Int"/> representing the new direction of the road head after applying variation</returns>
    Vector2Int DirectionShift(Vector2Int baseDir, float chance)
    {   
        //Random chance to turn
        if (UnityEngine.Random.value > chance)
            return baseDir;

        //90 degree rotation in either direction, decided randomly
        if (UnityEngine.Random.Range(0, 2) == 0)
            return new Vector2Int(-baseDir.y, baseDir.x);
        else
            return new Vector2Int(baseDir.y, -baseDir.x);
    }

    /// <summary>
    /// Random starting direction
    /// </summary>
    /// <returns><see cref="Vector2Int"/> randomly normalized</returns>
    DirectionPair BaseDirection()
    {
        Vector2Int baseDir;

        switch (UnityEngine.Random.Range(1, 5))
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
                baseDir = Vector2Int.up;
                break;
        }
        
        return new DirectionPair(baseDir, Vector2Int.zero);
    }

    /// <summary>
    /// Checks the chunk in the given direction from the given coordinates this is used to determine if a road can be placed in that chunk, if the chunk is invalid, it cannot be used for road placement, if it is empty, it can be used for road placement and will be populated with a road chunk, if it is populated, it cannot be used for road placement but does not necessarily mean it is invalid as there may already be a road there or it may be a building chunk that can coexist with a road chunk
    /// </summary>
    /// <param name="dir">The direction to check</param>
    /// <param name="coord">The root coordinates</param>
    /// <param name="chunkObject">The chunk object at the specified coordinates</param>
    /// <param name="state">The state of the chunk</param>
    /// <returns><see cref="ChunkState"/> (invalid, empty, populated) and <see cref="ChunkObject"/> </returns>
    ChunkState CheckChunk(Vector2Int dir, Vector2Int coord, out ChunkObject chunkObject, out ChunkState state)
    {
        chunkObject = null;

        dir *= chunkSize;

        ChunkObject testChunk = null;

        //Checks straight forward

        if(!chunks.TryGetValue(coord + dir, out chunkObject) || chunkObject.chunkType != ChunkObject.ChunkType.Empty)
        {
            if (chunkObject == null)
            {
                state = ChunkState.Invalid;
                return ChunkState.Invalid;
            }

            state = ChunkState.Populated;
            return ChunkState.Populated;
        }

        //checks to make sure next tile is not adjacent to other roads
        
        Vector2Int dir2 = dir;

        if(!chunks.TryGetValue(coord + dir + dir2, out testChunk) || testChunk.chunkType != ChunkObject.ChunkType.Empty)
        {
            if (testChunk == null)
            {
                state = ChunkState.Invalid;
                return ChunkState.Invalid;
            }

            state = ChunkState.Populated;
            return ChunkState.Populated;
        }
        
        dir2 = new Vector2Int(-dir.y, dir.x);

        if(!chunks.TryGetValue(coord + dir + dir2, out testChunk) || testChunk.chunkType != ChunkObject.ChunkType.Empty)
        {
            if (testChunk == null)
            {
                state = ChunkState.Invalid;
                return ChunkState.Invalid;
            }

            state = ChunkState.Populated;
            return ChunkState.Populated;
        }

        dir2 = new Vector2Int(dir.y, -dir.x);

        if(!chunks.TryGetValue(coord + dir + dir2, out testChunk) || testChunk.chunkType != ChunkObject.ChunkType.Empty)
        {
            if (testChunk == null)
            {
                state = ChunkState.Invalid;
                return ChunkState.Invalid;
            }

            state = ChunkState.Populated;
            return ChunkState.Populated;
        }

        state = ChunkState.Empty;
        return ChunkState.Empty;
    }

    /// <summary>
    /// Debug
    /// </summary>
    public void DebugLines()
    {
        foreach (var kvp in chunks)
        {
            Vector2Int coord = kvp.Key;
            ChunkObject chunk = kvp.Value;
            
            Color color;

            switch (chunk)
            {
                case EmptyChunk:
                    color = Color.white;
                    break;
                case RoadChunk:
                    color = Color.indianRed;
                    break;
                case BuildingChunk:
                    BuildingChunk buildingChunk = (BuildingChunk)chunk;
                    if (buildingChunk.occupied)
                        color = Color.blueViolet;
                    else
                        color = Color.cyan;
                    break;
                default:
                    color = Color.black;
                    break;
            }

            Debug.DrawLine(new Vector3(coord.x, 0, coord.y), new Vector3(coord.x + chunkSize, 0, coord.y), color);
            Debug.DrawLine(new Vector3(coord.x, 0, coord.y), new Vector3(coord.x, 0, coord.y + chunkSize), color);

            if (chunk.chunkType == ChunkObject.ChunkType.Road)
            {
                RoadChunk roadChunk = (RoadChunk)kvp.Value;

                switch (roadChunk.identity)
                {
                    case RoadChunk.RoadIdentity.Straight:
                        color = Color.green;
                        break;
                    case RoadChunk.RoadIdentity.Bent:
                        color = Color.yellow;
                        break;
                    case RoadChunk.RoadIdentity.End:
                        color = Color.red;
                        break;
                    case RoadChunk.RoadIdentity.Intersection:
                        color = Color.blue;
                        break;
                }

                Debug.DrawLine(new Vector3(coord.x, 5, coord.y), new Vector3(coord.x + roadChunk.nextDir.x * chunkSize, 5, coord.y + roadChunk.nextDir.y * chunkSize), color);
                Debug.DrawLine(new Vector3(coord.x, 5, coord.y), new Vector3(coord.x + roadChunk.branchDir.x * chunkSize, 5, coord.y + roadChunk.branchDir.y * chunkSize), color);
            }
        }

        foreach (var kvp in roadHeads)
        {
            Vector2Int coord = kvp.Key;
            Vector2Int dir = kvp.Value.baseDir * 4;

            Debug.DrawLine(new Vector3(coord.x, 0, coord.y), new Vector3(coord.x, 5, coord.y), Color.magenta);
            Debug.DrawLine(new Vector3(coord.x, 5, coord.y), new Vector3(coord.x + dir.x, 5, coord.y + dir.y), Color.yellowNice);
        }
    }
    /// <summary>
    /// Struct for road heads with original and previous directions
    /// </summary>
    public struct DirectionPair
    {
        public Vector2Int baseDir;
        public Vector2Int prevDir;

        public DirectionPair(Vector2Int norm, Vector2Int prev)
        {
            baseDir = norm;
            prevDir = prev;
        }
    }
}
