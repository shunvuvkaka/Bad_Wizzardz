using System.Collections.Generic;
using UnityEngine;
using System;
using BadWizards.ChunkData;

public class GridGenerator : MonoBehaviour
{
    [Header("Main")]
    public int viewDistance;
    public int buildingDistance;
    public int chunkSize;
    public Transform player;
    [Header("Road")]
    public float variationChance;
    public float splitChance;
    public float deathChance;
    public int minHeads;
    [Header("Buildings")]
    public int buildingIterations;
    [Header("Debug")]
    public Vector2Int playerChunk;
    public Dictionary<Vector2Int, ChunkObject> chunks = new Dictionary<Vector2Int, ChunkObject>();
    public Dictionary<Vector2Int, DirectionPair> roadHeads {get; private set;} = new Dictionary<Vector2Int, DirectionPair>();
    public HashSet<Vector2Int> activeChunks = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> toRemove = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> newRoads = new HashSet<Vector2Int>();
    public static Action OnNewChunks;
    public static Action OnGenerateBuildings;

    public static GridGenerator Instance;

    private const int MAX_DEPTH = 128;

    private int depth;
    private int buildingCount;
    

    enum ChunkState
    {
        Invalid,
        Empty,
        Populated
    }

    void Awake()
    {
        buildingCount = 0;
        //ensures viiewDistance is a multiple of chunkSize
        roadHeads.Add(new Vector2Int(0, 0), BaseDirection());

        int mod = viewDistance % chunkSize;
        viewDistance -= mod;
        buildingDistance -= mod;

        Instance = this;
    }


    void Update()
    {
        depth = 0;

        playerChunk = new Vector2Int(Mathf.FloorToInt(player.position.x / chunkSize), Mathf.FloorToInt(player.position.z / chunkSize)) * chunkSize;

        if (GenerateChunks())
        {
            buildingCount--;
            OnNewChunks?.Invoke();
            PopulateRoadChunks();
            PopulateBuildingChunks();

            if(buildingCount < 0)
            {
                OnGenerateBuildings?.Invoke();
                buildingCount = buildingIterations;
            }

        }

        RemoveChunks();

        DebugLines();
    }

    bool GenerateChunks()
    {
        bool newChunks = false;

        for (int x = -viewDistance; x <= viewDistance; x += chunkSize)
        {
            for (int z = -viewDistance; z <= viewDistance; z += chunkSize)
            {
                Vector2Int coord = new Vector2Int(playerChunk.x + x, playerChunk.y + z);

                if (!activeChunks.Contains(coord))
                {
                    newChunks = true;
                    activeChunks.Add(coord);
                    chunks.Add(coord, new EmptyChunk());
                }

                if (roadHeads.Count < minHeads)
                {
                    roadHeads.TryAdd(coord, BaseDirection());
                }
            }
        }

        return newChunks;
    }

    void RemoveChunks()
    {
        toRemove.Clear();

        foreach(Vector2Int coord in activeChunks)
        {   
            //no sqrt bc euclidean
            if (Mathf.Abs(coord.x - playerChunk.x) > viewDistance
                || Mathf.Abs(coord.y - playerChunk.y) > viewDistance)
            {
                toRemove.Add(coord);
            }
        }

        foreach(Vector2Int coord in toRemove)
        {   
            activeChunks.Remove(coord);
            chunks.Remove(coord);
            
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

    void PopulateRoadChunks()
    {
        newRoads.Clear();

        Vector2Int[] roadHeadCoordArray = new Vector2Int[roadHeads.Count];
        DirectionPair[] roadHeadDirArray = new DirectionPair[roadHeads.Count];

        int i = 0;

        foreach (var kvp in roadHeads)
        {
            roadHeadCoordArray[i] = kvp.Key;
            roadHeadDirArray[i] = kvp.Value;

            i++;
        }

        for (int j = 0; j < roadHeadCoordArray.Length; j++)
        {
            Vector2Int coord = roadHeadCoordArray[j];
            Vector2Int baseDir = roadHeadDirArray[j].baseDir;
            Vector2Int prevDir = roadHeadDirArray[j].prevDir;

            while (true)
            {
                if (!AdvanceRoad(ref coord, baseDir, prevDir, out prevDir))
                    break;
            }
        }

        if (newRoads.Count > 0 && depth < MAX_DEPTH)
        {
            //recursion limited by depth, may screw up some gen if broken because of depth, but better than crash
            PopulateRoadChunks();
            depth++;
        }
    }
    
    void PopulateBuildingChunks()
    {
        for (int x = -buildingDistance; x <= buildingDistance; x += chunkSize)
        {
            for (int z = -buildingDistance; z <= buildingDistance; z += chunkSize)
            {
                Vector2Int coord = new Vector2Int(playerChunk.x + x, playerChunk.y + z);


                //if this executes, something has gone very wrong
                if (!activeChunks.Contains(coord))
                    continue;

                ChunkObject chunkObject = chunks[coord];

                if (chunkObject is EmptyChunk)
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

    Vector2Int DirectionShift(Vector2Int baseDir, float chance)
    {
        if (UnityEngine.Random.value > chance)
            return baseDir;

        if (UnityEngine.Random.Range(0, 2) == 0)
            return new Vector2Int(-baseDir.y, baseDir.x);
        else
            return new Vector2Int(baseDir.y, -baseDir.x);
    }

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
