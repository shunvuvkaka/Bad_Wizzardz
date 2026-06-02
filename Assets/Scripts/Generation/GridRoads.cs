/*
* This script is responsible for generating roads on the grid. It listens for new chunks being generated and creates road GameObjects based on the chunk data.
* It uses a dictionary to keep track of active roads and ensures that roads are only generated in valid locations.
*/

using System.Collections.Generic;
using BadWizards.ChunkData;
using UnityEngine;

public class GridRoads : MonoBehaviour
{
    [Tooltip("List of possible road prefabs with their corresponding identities")]
    public RoadObject[] roads;
    [SerializeField] private GridGenerator gridGenerator;
    //Singleton instance for easy access from other scripts
    public static GridRoads Instance;
    //Dictionary for active roads indexed by world coordinates
    public Dictionary<Vector2Int, GameObject> activeRoads = new Dictionary<Vector2Int, GameObject>();
    
    //Init
    void Awake()
    {
        Instance = this;

        GridGenerator.OnNewChunks += GenerateRoads;
    }
    //Unsubscribe from event on destroy to prevent errors
    void OnDestroy()
    {
        GridGenerator.OnNewChunks -= GenerateRoads;
    }
    /// <summary>
    /// Generates roads based on current chunk data
    /// </summary>
    void GenerateRoads()
    {
        //Iterate over every chunk
        foreach (var kvp in gridGenerator.chunks)
        {
            //Get chunk data and coordinates
            ChunkObject chunk = kvp.Value;
            Vector2Int coord = kvp.Key;

            RoadChunk roadChunk;

            //Continue if not road, else read chunk as road
            if (chunk.chunkType != ChunkObject.ChunkType.Road)
                continue;
            else
                roadChunk = (RoadChunk)chunk;

            //Continue if occupied
            if (roadChunk.occupied)
            {
                continue;
            }
            
            //Backup check to ensure road is not aready oresent
            if (activeRoads.ContainsKey(coord))
            {
                continue;
            }

            Vector2Int nextChunk = coord + roadChunk.nextDir * gridGenerator.chunkSize;
            //Rotation is calculated in the constructor for road chunks
            int rotation = roadChunk.rotation;

            //Kills roads that run into invalid chunks or non-road chunks
            if (!gridGenerator.chunks.TryGetValue(nextChunk, out ChunkObject testChunk) && !gridGenerator.roadHeads.ContainsKey(nextChunk))
                roadChunk = new RoadChunk(roadChunk.currDir, Vector2Int.zero, Vector2Int.zero);

            if (testChunk != null && testChunk.chunkType != ChunkObject.ChunkType.Road && !gridGenerator.roadHeads.ContainsKey(nextChunk))
                roadChunk = new RoadChunk(roadChunk.currDir, Vector2Int.zero, Vector2Int.zero);

            //Assign road identity based on directions
            ChunkObject.RoadIdentity roadIdentity = roadChunk.identity;

            //Select road
            GameObject road = ChooseRoad(roadIdentity);

            //Assign transform values and occupy chunk
            road.transform.parent =  transform;
            road.transform.position = new Vector3(coord.x + gridGenerator.chunkSize / 2, 0, coord.y + gridGenerator.chunkSize / 2);
            road.transform.rotation = Quaternion.Euler(new Vector3(0, rotation, 0));
            roadChunk.occupied = true;
            activeRoads.Add(coord, road);
        }
    }

    //Really make sure we have a reference
    void Update()
    {
        if (gridGenerator == null)
            gridGenerator = GridGenerator.Instance;
    }

    /// <summary>
    /// Chooses a road prefab from valid identities at random
    /// </summary>
    /// <param name="identity">What road identity to choose</param>
    /// <returns><see cref="GameObject"/> representing the chosen road</returns>
    GameObject ChooseRoad(ChunkObject.RoadIdentity identity)
    {
        List<GameObject> candidateRoads = new List<GameObject>();

        foreach (RoadObject roadObject in roads)
        {
            if (roadObject.identity == identity)
                candidateRoads.Add(roadObject.road);
        }

        return Instantiate(candidateRoads[Random.Range(0, candidateRoads.Count)]);
    }
}
