using System.Collections.Generic;
using BadWizards.ChunkData;
using UnityEngine;
using static BadWizards.ChunkData.RoadChunk;

public class GridRoads : MonoBehaviour
{
    public RoadObject[] roads;
    [SerializeField] private GridGenerator gridGenerator;
    public static GridRoads Instance;
    public Dictionary<Vector2Int, GameObject> activeRoads = new Dictionary<Vector2Int, GameObject>();
    void Awake()
    {
        Instance = this;

        GridGenerator.OnNewChunks += GenerateRoads;
    }
    void OnDestroy()
    {
        GridGenerator.OnNewChunks -= GenerateRoads;
    }

    void GenerateRoads()
    {
        foreach (var kvp in gridGenerator.chunks)
        {
            ChunkObject chunk = kvp.Value;
            Vector2Int coord = kvp.Key;

            RoadChunk roadChunk;

            if (chunk.chunkType != ChunkObject.ChunkType.Road)
                continue;
            else
                roadChunk = (RoadChunk)chunk;

            if (roadChunk.occupied)
            {
                continue;
            }
            
            if (activeRoads.ContainsKey(coord))
            {
                continue;
            }

            Vector2Int nextChunk = coord + roadChunk.nextDir * gridGenerator.chunkSize;
            int rotation = roadChunk.rotation;

            if (!gridGenerator.chunks.TryGetValue(nextChunk, out ChunkObject testChunk) && !gridGenerator.roadHeads.ContainsKey(nextChunk))
                roadChunk = new RoadChunk(roadChunk.currDir, Vector2Int.zero, Vector2Int.zero);

            if (testChunk.chunkType != ChunkObject.ChunkType.Road && !gridGenerator.roadHeads.ContainsKey(nextChunk))
                roadChunk = new RoadChunk(roadChunk.currDir, Vector2Int.zero, Vector2Int.zero);

            RoadIdentity roadIdentity = roadChunk.identity;

            GameObject road = ChooseRoad(roadIdentity);

            road.transform.parent =  transform;
            road.transform.position = new Vector3(coord.x + gridGenerator.chunkSize / 2, 0, coord.y + gridGenerator.chunkSize / 2);
            road.transform.rotation = Quaternion.Euler(new Vector3(0, rotation, 0));
            roadChunk.occupied = true;
            activeRoads.Add(coord, road);
        }
    }

    void Update()
    {
        if (gridGenerator == null)
            gridGenerator = GridGenerator.Instance;
    }
    GameObject ChooseRoad(RoadIdentity identity)
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
