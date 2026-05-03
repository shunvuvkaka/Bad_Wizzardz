using System.Collections.Generic;
using BadWizards.ChunkData;
using UnityEngine;

public class GridBuildings : MonoBehaviour
{
    public Building[] buildings;
    public Vector2Int angleRange;
    private GridGenerator gridGenerator;
    public static GridBuildings Instance;
    public Dictionary<Vector2Int, GameObject> activeBuildings = new Dictionary<Vector2Int, GameObject>();

    void Awake()
    {
        GridGenerator.OnGenerateBuildings += GenerateBuildings;
        Instance = this;

        if (gridGenerator == null)
            gridGenerator = GridGenerator.Instance;
    }
    void OnDestroy()
    {
        GridGenerator.OnGenerateBuildings -= GenerateBuildings;
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

            Debug.DrawRay(new Vector3(coord.x, 5, coord.y), Vector3.up * 5, Color.orange);

            if (chunk.chunkType != ChunkObject.ChunkType.Building)
                continue;
            else
                buildingChunk = (BuildingChunk)chunk;

            if (buildingChunk.occupied)
            {
                continue;
            }
            
            if (activeBuildings.ContainsKey(coord))
            {
                continue;
            }

            Building buildingObject = SelectBuilding();

            Vector2Int size = buildingObject.size;
            Vector2Int dir = BaseDirection();
            //Vector2Int dir = Vector2Int.right;

            if (!AuthenticateBuilding(size, ref dir, coord))
            {
                Debug.Log("Failed Authentication");
                continue;
            }

            GameObject building = Instantiate(buildingObject.building);

            building.transform.parent    =  transform;
            building.transform.position  =  SetPosition(dir, coord, gridGenerator.chunkSize);
            building.transform.rotation  =  ReinterperetDirection(dir);
            building.transform.rotation *= RandomRotation();

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
    bool AuthenticateBuilding(Vector2Int size, ref Vector2Int dir, Vector2Int coord)
    {
        for (int i = 0; i < 4; i++)
        {
            if (CheckChunk(size, dir, coord))
                return true;

            dir = new Vector2Int(-dir.y, dir.x);
        }

        return false;
    }
    bool CheckChunk(Vector2Int size, Vector2Int dir, Vector2Int coord)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int offset = ShiftDirection(x, y, dir) * gridGenerator.chunkSize;
                Vector2Int checkPos = coord + offset;

                Debug.DrawRay(new Vector3(checkPos.x, 5, checkPos.y), Vector3.up * 5, Color.red);

                ChunkObject chunk;

                if (!gridGenerator.chunks.TryGetValue(checkPos, out chunk))
                    return false;

                BuildingChunk buildingChunk;

                if (chunk.chunkType != ChunkObject.ChunkType.Building)
                    return false;
                else
                    buildingChunk = (BuildingChunk)chunk;
                
                if (buildingChunk.occupied)
                    return false;
            }
        }

        //after everything is true, we just set the chunks to occupied
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int offset = ShiftDirection(x, y, dir) * gridGenerator.chunkSize;
                Vector2Int checkPos = coord + offset;

                BuildingChunk buildingChunk = (BuildingChunk)gridGenerator.chunks[checkPos];

                buildingChunk.occupied = true;
            }
        }

        Vector2Int testSize = ShiftDirection(size.x, size.y, dir) * gridGenerator.chunkSize;

        Vector3 testPos = SetPosition(dir, coord, gridGenerator.chunkSize);
        Vector2Int pos = new Vector2Int((int)testPos.x, (int)testPos.z);

        Debug.DrawLine(new Vector3(pos.x, 3, pos.y), new Vector3(pos.x + testSize.x, 3, pos.y), Color.darkBlue, 5f);
        Debug.DrawLine(new Vector3(pos.x + testSize.x, 3, pos.y), new Vector3(pos.x + testSize.x, 3, pos.y + testSize.y), Color.darkBlue, 5f);
        Debug.DrawLine(new Vector3(pos.x + testSize.x, 3, pos.y + testSize.y), new Vector3(pos.x, 3, pos.y + testSize.y), Color.darkBlue, 5f);
        Debug.DrawLine(new Vector3(pos.x, 3, pos.y + testSize.y), new Vector3(pos.x, 3, pos.y), Color.darkBlue, 5f);

        return true;
    }

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

    Quaternion RandomRotation() => Quaternion.Euler(new Vector3(0, Random.Range(angleRange.x, angleRange.y + 1), 0));

    Building SelectBuilding() => buildings[Random.Range(0, buildings.Length)];
}
