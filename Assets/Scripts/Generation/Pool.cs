using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    public static Pool Instance;
    [SerializeField] private Road proceduralRoad;
    [SerializeField] private TerrainGenerator terrainGenerator;
    public GameObject roadPrefab;
    public GameObject terrainPrefab;
    [SerializeField] private List<GameObject> pooledRoad = new List<GameObject>();
    [SerializeField] private List<GameObject> pooledTerrain = new List<GameObject>();
    void Awake()
    {
        if (Instance == null) 
            Instance = this;
        
        PrepareRoads(proceduralRoad.segmentsAhead + proceduralRoad.segmentsBehind + 10);
        PrepareTerrain(terrainGenerator.viewDistance + 1);
    }

    void PrepareRoads(int instantiations)
    {
        for (int i = 0; i <= instantiations; i++)
        {
            GameObject go = Instantiate(roadPrefab, transform);
            go.SetActive(false);
            pooledRoad.Add(go);
        }
    }

    void PrepareTerrain(int viewDistance)
    {
        int instantiations = (int)Mathf.Pow((viewDistance * 2), 2) + viewDistance * 4;

        for (int i = 0; i <= instantiations; i++)
        {
            GameObject go = Instantiate(terrainPrefab, transform);
            go.SetActive(false);
            pooledTerrain.Add(go);
        }
    }
    public GameObject GetPooledRoad()
    {
        for (int i = 0; i < pooledRoad.Count; i++)
        {
            if (pooledRoad[i].activeInHierarchy == false)
            {
                return pooledRoad[i];
            }
        }
        Debug.LogError("Road Pool empty!");
        GameObject go = Instantiate(roadPrefab, transform);
        pooledRoad.Add(go);
        return go;
    }
    public GameObject GetPooledTerrain()
    {
        for (int i = 0; i < pooledTerrain.Count; i++)
        {
            if (pooledTerrain[i].activeInHierarchy == false)
            {
                pooledTerrain[i].SetActive(true);
                return pooledTerrain[i];
            }
        }
        Debug.LogError("Terrain Pool empty!");
        return Instantiate(terrainPrefab, transform);
    }

    public void ReturnRoad(GameObject road)
    {
        road.transform.parent = transform;

        road.SetActive(false);
    }
}