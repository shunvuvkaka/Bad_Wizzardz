using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine.Rendering;
using System.Collections;
using System;

public class Road : MonoBehaviour
{
    [Header("Player Tracking")]
    public Transform player;

    [Header("Road Settings")]
    public Material roadMaterial;
    public PhysicsMaterial roadPhysics;
    public int segmentLength = 1;
    public float roadWidth = 6f;

    [Header("Turning Logic")]
    public float turnChance = 0.15f;
    public float maxTurnAngle = 45f;
    public float curveSmoothness = 0.2f;

    [Header("Boundaries")]
    public float distance;
    public float height;
    public Material borderMat;
    public int borderFrequency;

    [Header("Render Distance")]
    public int segmentsAhead = 500;
    public int segmentsBehind = 100;
    public static event Action onGenerate;
    public static Road Instance;

    //internal state
    private float currentAngle = 0f;
    public List<Vector3> roadPoints = new List<Vector3>();
    public Dictionary<int, Vector3> pointDict = new Dictionary<int, Vector3>();
    public Dictionary<int, Vector3> normalDict = new Dictionary<int, Vector3>();
    private Dictionary<int, GameObject> roadSegments = new Dictionary<int, GameObject>();
    public List<Vector3> pointNormals = new List<Vector3>();
    private NativeArray<float3> jobPointNormals;
    private NativeArray<float3> jobRoadPoints;
    private int closestRoadIndex;
    private Coroutine segmentCoroutine;
    private int roadIndexOffset = 0;
    public int neededPoints;
    public int globalIndex = 0;

    private float tall;

    //going backwards is screwed dont try please :pray :pray

    void Awake()
    {
        Instance = this;
    }
    void Update()
    {
        UpdateClosestIndex();
        TrimRoadPoints();

        int localClosest = closestRoadIndex - roadIndexOffset;
        localClosest = Mathf.Clamp(localClosest, 0, roadPoints.Count);

        neededPoints = localClosest + segmentsAhead;

        while (roadPoints.Count < neededPoints + 1)
            AddNextRoadPoint();

        if (segmentCoroutine == null)
            segmentCoroutine = StartCoroutine(SmoothSegmentGeneration());

    }

    IEnumerator SmoothSegmentGeneration()
    {
        yield return null;

        jobRoadPoints = new NativeArray<float3>(roadPoints.Count, Allocator.TempJob);
        jobPointNormals = new NativeArray<float3>(roadPoints.Count, Allocator.TempJob);

        for (int i = 0; i < roadPoints.Count; i++)
            jobRoadPoints[i] = roadPoints[i];

        CalculatePointNormals calculatePoint = new CalculatePointNormals
        {
            nativePointNormals = jobPointNormals,
            nativeRoadPoints = jobRoadPoints,
            direction = new float3(0, 1, 0)
        };
        JobHandle handle = calculatePoint.Schedule(roadPoints.Count, 32);

        pointNormals.Clear();

        handle.Complete();

        for (int i = 0; i < jobPointNormals.Length; i++)
            pointNormals.Add(jobPointNormals[i]);

        jobPointNormals.Dispose();
        jobRoadPoints.Dispose();

        GenerateVisibleSegments();

        segmentCoroutine = null;
    }

    void AddNextRoadPoint()
    {
        Vector3 lastPoint = roadPoints.Count > 0 ? roadPoints[^1] : Vector3.zero;

        //currently the only codde that dictates direction of road, its just simple rng
        if (UnityEngine.Random.value < turnChance)
        {
            float turn = UnityEngine.Random.Range(-maxTurnAngle, maxTurnAngle);
            currentAngle = Mathf.Lerp(currentAngle, currentAngle + turn, curveSmoothness);
            currentAngle = Mathf.Clamp(currentAngle, -50, 50);
        }

        //direction in world space
        Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward;
        Vector3 nextPoint = lastPoint + direction * segmentLength;

        //TODO
        float height = 1.1f;
        tall++;

        nextPoint.y = height;

        roadPoints.Add(nextPoint);
        onGenerate?.Invoke();
    }
    void TrimRoadPoints()
    {
        int minIndex = closestRoadIndex - segmentsBehind - 2;
        int removeCount = minIndex - roadIndexOffset;

        if (removeCount <= 0)
            return;

        removeCount = Mathf.Min(removeCount, roadPoints.Count - 2);

        roadPoints.RemoveRange(0, removeCount);
        pointNormals.RemoveRange(0, removeCount);

        roadIndexOffset += removeCount;
        closestRoadIndex -= removeCount;
    }

    void GenerateVisibleSegments()
    {
        int playerIndex = closestRoadIndex - roadIndexOffset;

        for (int i = playerIndex - segmentsBehind; i < playerIndex + segmentsAhead; i++)
        {
            int worldIndex = i + roadIndexOffset;

            if (i < 0 || i >= roadPoints.Count - 1)
                continue;

            if (!roadSegments.ContainsKey(worldIndex))
            {
                GameObject segment = CreateRoadSegment(roadPoints[i], roadPoints[i + 1], worldIndex);
                roadSegments[worldIndex] = segment;
                pointDict.Add(worldIndex, roadPoints[i]);
                normalDict.Add(worldIndex, pointNormals[i]);
                globalIndex ++;
            }
        }

        //remove segments
        List<int> toRemove = new List<int>();
        foreach (var kvp in roadSegments)
        {
            if (kvp.Key < closestRoadIndex - segmentsBehind)
            {
                Pool.Instance.ReturnRoad(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }

        //needs to be done in a seperate loop due to coroutine timings. 
        //slower processors that take longer to finsish the coroutine in more than 4 frames may cause problems as the dictionary may be edited whils the coroutine is still running.
        //maybe add profiling to alter timings?

        foreach (int index in toRemove)
        {
            roadSegments.Remove(index);
            pointDict.Remove(index);
            normalDict.Remove(index);
        }
    }

    GameObject CreateRoadSegment(Vector3 a, Vector3 b, int index)
    {
        Vector3 leftA; 
        Vector3 rightA;
        int localIndex = index - roadIndexOffset;
        Vector3 normalA = pointNormals[localIndex];

        if (index > 0 && roadSegments.ContainsKey(index - 1))
        {
            // Stitch from previous segment
            Mesh prevMesh = roadSegments[index - 1].transform.GetChild(0).GetComponent<MeshFilter>().mesh;
            leftA = prevMesh.vertices[2]; 
            rightA = prevMesh.vertices[3];
        }
        else
        {
            //fallback if there is no previous segment. should ONLY be first segment
            leftA = a - normalA * (roadWidth / 2f);
            rightA = a + normalA * (roadWidth / 2f);
        }

        GameObject go = Pool.Instance.GetPooledRoad();
        go.transform.SetParent(transform, true);
        go.name = $"Road Segment {index}";

        RoadSegment segment = go.transform.GetChild(0).GetComponent<RoadSegment>();
        GameObject wallGO  = go.transform.GetChild(1).gameObject;

        Vector3 normalB = pointNormals[localIndex + 1];
        Vector3 leftB = b - normalB * (roadWidth / 2f);
        Vector3 rightB = b + normalB * (roadWidth / 2f);

        segment.ApplyMesh(leftA, rightA, leftB, rightB, roadMaterial, roadPhysics);

        if (index % borderFrequency != 0)
        {
            go.SetActive(true);
            return go;
        }

        Mesh wall = new Mesh();

        if (index > borderFrequency && roadSegments.ContainsKey(index - 1))
        {
            // Stitch from previous segment
            Mesh prevMesh = roadSegments[index - borderFrequency].transform.GetChild(1).GetComponent<MeshFilter>().mesh;
            leftA = prevMesh.vertices[1]; 
            rightA = prevMesh.vertices[5];
        }
        else
        {
            //fallback if there is no previous segment. should ONLY be first segment
            leftA -= normalA * distance;
            rightA += rightA + normalA * distance;
        }

        wall.vertices = new Vector3[]
        {
            //LEFT SIDE

            leftA,                                            //bottom left corner,  0
            leftB - normalA * distance,                       //bottom right corner, 1
            leftA + Vector3.up * height,                      //top left corner,     2
            leftB - normalA * distance + Vector3.up * height, //top right corner,    3

            //RIGHT SIDE

            rightA,                                            //bottom right corner,4
            rightB + normalA * distance,                       //bottom left corner, 5
            rightA + Vector3.up * height,                      //top right corner,   6
            rightB + normalA * distance + Vector3.up * height, //top left corner,    7
        };

        wall.triangles = new int[]
        {
            //LEFT

            0, 1, 3,
            3, 2, 0,

            //RIGHT

            4, 6, 7,
            7, 5, 4
        };

        wall.RecalculateNormals();

        MeshFilter mf = wallGO.GetComponent<MeshFilter>();
        MeshCollider mc = wallGO.GetComponent<MeshCollider>();
        MeshRenderer mr = wallGO.GetComponent<MeshRenderer>();

        mf.mesh = wall;
        mc.sharedMesh = wall;
        mr.material = borderMat;
        //mc.convex = true;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        
        go.SetActive(true);

        return go;

    }

    void UpdateClosestIndex()
    {
        int localIndex = closestRoadIndex - roadIndexOffset;
        localIndex = Mathf.Clamp(localIndex, 0, roadPoints.Count - 1);

        while (localIndex + 1 < roadPoints.Count &&
            Vector3.SqrMagnitude(roadPoints[localIndex + 1] - player.position) <
            Vector3.SqrMagnitude(roadPoints[localIndex] - player.position))
        {
            localIndex++;
        }

        closestRoadIndex = localIndex + roadIndexOffset;
    }
    [BurstCompile]

    //i dont remember why i made a job to calculate normals, but its porbably faster and it works soooooo
    private struct CalculatePointNormals : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> nativeRoadPoints;
        [ReadOnly] public float3 direction;
        [WriteOnly] public NativeArray<float3> nativePointNormals;

        public void Execute(int index)
        {
            float3 dir;
            if (index == 0)
                dir = math.normalize(nativeRoadPoints[index + 1] - nativeRoadPoints[index]);
            else if (index == nativeRoadPoints.Length - 1)
                dir = math.normalize(nativeRoadPoints[index] - nativeRoadPoints[index - 1]);
            else
            {
                float3 forward = math.normalize(nativeRoadPoints[index + 1] - nativeRoadPoints[index]);
                float3 back = math.normalize(nativeRoadPoints[index] - nativeRoadPoints[index - 1]);
                dir = math.normalize((forward + back) * 0.5f);
            }

            float3 normal = math.cross(dir, direction);
            nativePointNormals[index] = normal;
        }
    }
    /*
    private NativeArray<float3> EnsureArray(NativeArray<float3> array, int length)
    {
        if (array.IsCreated)
        {
            if (array.Length != length)
            {
                array.Dispose();
                array = new NativeArray<float3>(length, Allocator.Persistent);
            }
        }
        else
        {   
            array = new NativeArray<float3>(length, Allocator.Persistent);
        }
        return array;
    }
    */
    void OnDestroy()
    {
        if (jobPointNormals.IsCreated)
            jobPointNormals.Dispose();
        if (jobRoadPoints.IsCreated)
            jobRoadPoints.Dispose();
    }
}


