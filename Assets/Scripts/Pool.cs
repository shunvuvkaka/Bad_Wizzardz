using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    public static Pool Instance;
    [SerializeField] private Road proceduralRoad;
    public GameObject roadPrefab;
    [SerializeField] private List<GameObject> pooledRoad = new List<GameObject>();
    void Awake()
    {
        if (Instance == null) Instance = this;
        PrepareRoads(proceduralRoad.segmentsAhead + proceduralRoad.segmentsBehind + 10);
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

    public void ReturnRoad(GameObject road)
    {
        road.transform.parent = transform;

        for (int i = road.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(road.transform.GetChild(i).gameObject);
        }
        road.SetActive(false);
    }
}