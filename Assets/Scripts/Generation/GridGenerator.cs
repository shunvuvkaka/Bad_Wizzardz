using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public class GridGenerator : MonoBehaviour
{
    public int viewDistance;
    public int chunkSize;
    public Transform player;

    [Header("Debug")]
    private Dictionary<Vector2Int, ChunkObject> chunks = new Dictionary<Vector2Int, ChunkObject>();
    [SerializeField] private List<Vector2Int> roadHeads = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> activeChunks = new List<Vector2Int>();
    private HashSet<Vector2Int> toRemove = new HashSet<Vector2Int>();
    [SerializeField] private Vector2Int playerChunk;
    private HashSet<Vector2Int> edges = new HashSet<Vector2Int>();
    public static Action OnNewChunks;

    enum ChunkState
    {
        Invalid,
        Emoty,
        Populated
    }

    void Awake()
    {
        //ensures viiewDistance is a multiple of chunkSize
        roadHeads.Add(new Vector2Int(0, 0));

        int mod = viewDistance % chunkSize;
        viewDistance -= mod;

        OnNewChunks += PopulateChunks;
    }
    void OnDestroy()
    {
        OnNewChunks -= PopulateChunks;
    }
    void Update()
    {
        playerChunk = new Vector2Int(Mathf.FloorToInt(player.position.x / chunkSize), Mathf.FloorToInt(player.position.z / chunkSize));

        if (GenerateChunks())
        {
            OnNewChunks?.Invoke();
            Debug.Log("Invoking");
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
                Vector2Int coord = new Vector2Int(playerChunk.x * chunkSize + x, playerChunk.y * chunkSize + z);
                
                if (x == -viewDistance || x == viewDistance || z == -viewDistance || z == viewDistance)
                    edges.Add(coord);

                if (!activeChunks.Contains(coord))
                {
                    newChunks = true;
                    activeChunks.Add(coord);
                    chunks.Add(coord, new EmptyChunk());
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
            //no sqrt
            if (Mathf.Abs(coord.x - playerChunk.x * chunkSize) > viewDistance
                || Mathf.Abs(coord.y - playerChunk.y * chunkSize) > viewDistance)
            {
                toRemove.Add(coord);
            }
        }

        foreach(Vector2Int coord in toRemove)
        {   
            activeChunks.Remove(coord);
            chunks.Remove(coord);

            roadHeads.Remove(coord);
        }
    }

    void PopulateChunks()
    {
        Vector2Int[] roadHeadsArray = roadHeads.ToArray();

        foreach (Vector2Int val in roadHeadsArray)
        {
            Vector2Int coord = val;

            Vector2Int baseDir;

            switch (UnityEngine.Random.Range(1, 3))
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
                default:
                    baseDir = Vector2Int.up;
                    break;
            }

            while (true)
            {
                if (!AdvanceRoad(ref coord, baseDir))
                    break;
            }
        }
    }

    bool AdvanceRoad(ref Vector2Int coord, Vector2Int baseDir)
    {
        ChunkObject chunkObject;

        Vector2Int newDir;

        ChunkState result;

        //this line is where we can set chance variations by setting dir to some other value of baseDir
        Vector2Int dir = baseDir;

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

        coord += newDir * chunkSize;

        roadHeads.Add(coord);

        chunks[coord] = new RoadChunk();

        return true;
    }

    ChunkState CheckChunk(Vector2Int dir, Vector2Int coord, out ChunkObject chunkObject, out ChunkState state)
    {
        chunkObject = null;

        dir *= chunkSize;

        ChunkObject testChunk = null;

        //Checks straight forward

        if(!chunks.TryGetValue(coord + dir, out chunkObject) || (Type)chunkObject.GetObject() != typeof(EmptyChunk))
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

        if(!chunks.TryGetValue(coord + dir + dir2, out testChunk) || (Type)testChunk.GetObject() != typeof(EmptyChunk))
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

        if(!chunks.TryGetValue(coord + dir + dir2, out testChunk) || (Type)testChunk.GetObject() != typeof(EmptyChunk))
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

        if(!chunks.TryGetValue(coord + dir + dir2, out testChunk) || (Type)testChunk.GetObject() != typeof(EmptyChunk))
        {
            if (testChunk == null)
            {
                state = ChunkState.Invalid;
                return ChunkState.Invalid;
            }

            state = ChunkState.Populated;
            return ChunkState.Populated;
        }

        state = ChunkState.Emoty;
        return ChunkState.Emoty;
    }

    void DebugLines()
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
                default:
                    color = Color.black;
                    break;
            }

            Debug.DrawLine(new Vector3(coord.x, 0, coord.y), new Vector3(coord.x + chunkSize, 0, coord.y), color);
            Debug.DrawLine(new Vector3(coord.x, 0, coord.y), new Vector3(coord.x, 0, coord.y + chunkSize), color);
        }

        foreach (Vector2Int coord in roadHeads)
        {
            Debug.DrawLine(new Vector3(coord.x, 0, coord.y), new Vector3(coord.x, 5, coord.y), Color.magenta);
        }
    }
    
}
