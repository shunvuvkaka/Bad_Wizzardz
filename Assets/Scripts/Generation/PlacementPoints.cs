using System.Collections.Generic;
using UnityEngine;

public class PlacementPoints : MonoBehaviour
{
    [Header("Props")]
    public GameObject[] terrainProps;
    public GameObject[] roofProps;
    [Header("Paramaters")]
    [SerializeField] private float terrainChance = 0.15f;
    [SerializeField] private float roofChance = 0.15f;
    [SerializeField] private float minSpacing = 1f;
    [SerializeField] private int terrainLayer;
    [SerializeField] private int roofLayer;
    public Dictionary<Transform, Vector3> terrainSpawns = new Dictionary<Transform, Vector3>();
    public Dictionary<Transform, Vector3> roofSpawns = new Dictionary<Transform, Vector3>();
    public static PlacementPoints Instance;

    void Awake()
    {
        Instance = this;
    }

    public void Propify(Vector3[] points, Transform parent, bool ground)
    {
        foreach (Vector3 point in points)
        {
            Collider[] colliders = Physics.OverlapSphere(point, minSpacing / 2, ground ? terrainLayer : roofLayer);

            if (colliders.Length == 0)
            {
                if (Random.value < (ground ? terrainChance : roofChance))
                {
                    GameObject prop;

                    if (ground)
                        prop = terrainProps[Mathf.RoundToInt(Random.Range(0, terrainProps.Length))];
                    else
                        prop = roofProps[Mathf.RoundToInt(Random.Range(0, roofProps.Length))];
                
                    Instantiate(prop, point, Quaternion.identity, parent);
                }

                //enemy spawnPoint code here
            }
        }
    }

}
