using System.Collections.Generic;
using BadWizards.ChunkData;
using UnityEngine;

public class GridBuildings : MonoBehaviour
{
    public Building[] buildings;
    private GridGenerator gridGenerator;
    public static GridBuildings Instance;
    public Dictionary<Vector2Int, GameObject> activeBuildings = new Dictionary<Vector2Int, GameObject>();

    void Awake()
    {
        GridGenerator.OnNewChunks += GenerateBuildings;
        Instance = this;
    }
    void OnDestroy()
    {
        GridGenerator.OnNewChunks -= GenerateBuildings;
    }
    void Update()
    {
        if (gridGenerator == null)
            gridGenerator = GridGenerator.Instance;
    }

    void GenerateBuildings()
    {
        foreach (var kvp in gridGenerator.chunks)
        {
            ChunkObject chunk = kvp.Value;
            Vector2Int coord = kvp.Key;

            BuildingChunk buildingChunk;

            if (chunk.chunkType != ChunkObject.ChunkType.Building)
                continue;
            else
                buildingChunk = (BuildingChunk)chunk;

            if (buildingChunk.occupied)
                continue;

            GameObject building = Instantiate(buildings[0].building);

            buildingChunk.occupied = true;

            building.transform.parent = transform;
            building.transform.position = new Vector3(coord.x, 0, coord.y);

            activeBuildings.Add(coord, building);
        }
    }
}
