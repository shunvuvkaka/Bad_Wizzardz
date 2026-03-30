
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.InputSystem.LowLevel;

public class PlacementPoints : MonoBehaviour
{
    [Header("Props")]
    public GameObject[] terrainProps;
    public GameObject[] roofProps;
    public GameObject[] enemiePrefabs;
    [Header("Paramaters")]
    [SerializeField] private float terrainChance = 0.15f;
    [SerializeField] private float roofChance = 0.15f;
    [SerializeField] private float minSpacing = 1f;
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private LayerMask roofLayer;
    [Header("Enemies")]
    [SerializeField] private float minDist = 20;
    [SerializeField] private float groundMaxDist = 200;
    [SerializeField] private float roofMaxDist = 200;
    [SerializeField] private int frequency = 200;
    public float spawnChane;
    public int maxEnemies;
    [Header("Debug")]
    [SerializeField] private bool debugLines = false;
    [SerializeField] private bool reccomendedDistances = true;
    private int currentInt;
    public List<Vector3> terrainSpanws = new List<Vector3>();
    public List<Vector3> roofSpawns = new List<Vector3>();
    public List<GameObject> enemies = new List<GameObject>();
    public static PlacementPoints Instance;

    void Awake()
    {
        currentInt = frequency;
        Instance = this;
        TerrainGenerator.onGenerate += CullSpawns;
        Road.onGenerate += CullSpawns;

        if (reccomendedDistances)
        {
            groundMaxDist = TerrainGenerator.Instance.viewDistance * TerrainGenerator.Instance.chunkSize;
            roofMaxDist = Buildings.Instance.removalDistance;
        }
    }
    void OnDestroy()
    {
        TerrainGenerator.onGenerate -= CullSpawns;
        Road.onGenerate -= CullSpawns;
    }
    void Update()
    {
        if (debugLines)
        {
            foreach (Vector3 point in terrainSpanws)
                Debug.DrawRay(point, Vector3.up * 2, Color.darkGoldenRod);

            foreach (Vector3 point in roofSpawns)
                Debug.DrawRay(point, Vector3.up * 2, Color.darkSalmon);
        }

        foreach (Vector3 point in terrainSpanws)
        {
            if (UnityEngine.Random.value < spawnChane && enemies.Count < maxEnemies)
            {
                GameObject enemy = Instantiate(enemiePrefabs[UnityEngine.Random.Range(0, 2)], point + transform.up * 2, Quaternion.identity);

                enemies.Add(enemy);
            }
        }
    }

    public void Propify(Vector3[] points, Transform parent, bool ground)
    {
        foreach (Vector3 point in points)
        {
            Collider[] colliders = Physics.OverlapSphere(point, minSpacing / 2, ground ? ~terrainLayer : ~roofLayer);

            if (colliders.Length == 0)
            {
                if (UnityEngine.Random.value < (ground ? terrainChance : roofChance))
                {
                    GameObject prop;

                    if (ground)
                        prop = terrainProps[Mathf.RoundToInt(UnityEngine.Random.Range(0, terrainProps.Length))];
                    else
                        prop = roofProps[Mathf.RoundToInt(UnityEngine.Random.Range(0, roofProps.Length))];
                
                    Instantiate(prop, point, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), parent);
                }

                if (currentInt == 0)
                {
                    if (ground)
                        terrainSpanws.Add(point);
                    else
                        roofSpawns.Add(point);
                    
                    currentInt = frequency;
                }
                else
                    currentInt--;
            }
        }
    }

    void CullSpawns()
    {
        float3 playerPos = Road.Instance.player.position;

        NativeArray<float3> jobTerrainSpawns = new NativeArray<float3>(terrainSpanws.Count, Allocator.Persistent);
        NativeArray<float3> nativeTerrainSpawns = new NativeArray<float3>(terrainSpanws.Count, Allocator.Persistent);

        for (int i = 0; i < terrainSpanws.Count; i++)
        {
            jobTerrainSpawns[i] = terrainSpanws[i];
        }

        CullJob floorJob = new CullJob
        {
            playerPos = playerPos,
            minDist = minDist,
            maxDist = groundMaxDist,
            spawns = jobTerrainSpawns,
            newSpawns = nativeTerrainSpawns
        };

        JobHandle floorHandle = floorJob.Schedule(jobTerrainSpawns.Length, 32);

        NativeArray<float3> jobRoofSpawns = new NativeArray<float3>(roofSpawns.Count, Allocator.Persistent);
        NativeArray<float3> nativeRoofSpawns = new NativeArray<float3>(roofSpawns.Count, Allocator.Persistent);

        for (int i = 0; i < roofSpawns.Count; i++)
        {
            jobRoofSpawns[i] = roofSpawns[i];
        }

        CullJob roofJob = new CullJob
        {
            playerPos = playerPos,
            minDist = minDist,
            maxDist = roofMaxDist,
            spawns = jobRoofSpawns,
            newSpawns = nativeRoofSpawns
        };

        JobHandle roofHandle = roofJob.Schedule(jobRoofSpawns.Length, 32);

        floorHandle.Complete();

        terrainSpanws.Clear();

        for (int i = 0; i < nativeTerrainSpawns.Length; i++)
        {
            if (nativeTerrainSpawns[i].x != 3939)
                terrainSpanws.Add(nativeTerrainSpawns[i]);
        }

        jobTerrainSpawns.Dispose();
        nativeTerrainSpawns.Dispose();

        roofHandle.Complete();

        roofSpawns.Clear();

        for (int i = 0; i < nativeRoofSpawns.Length; i++)
        {
            if (nativeRoofSpawns[i].x != 3939)
                roofSpawns.Add(nativeRoofSpawns[i]);
        }

        jobRoofSpawns.Dispose();
        nativeRoofSpawns.Dispose();
    }

    [BurstCompile]
    public struct CullJob : IJobParallelFor
    {
        [ReadOnly] public float3 playerPos;
        [ReadOnly] public float minDist;
        [ReadOnly] public float maxDist;
        [ReadOnly] public NativeArray<float3> spawns;
        [WriteOnly] public NativeArray<float3> newSpawns;

        public void Execute(int index)
        {
            float dist;

            dist = math.distance(playerPos, spawns[index]);

            if (dist > minDist && dist < maxDist)
                newSpawns[index] = spawns[index];
            else
                newSpawns[index] = new float3(3939, 0, 0);
        }
    }

}
