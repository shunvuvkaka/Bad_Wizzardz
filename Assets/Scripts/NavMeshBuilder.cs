using UnityEngine;
using Unity.AI;
using Unity.AI.Navigation;
using System.Collections;

public class NavMeshBuilder : MonoBehaviour
{

    public float WaitTime;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NavMeshSurface surface = GameObject.Find("NavMesh").GetComponent<NavMeshSurface>();
        surface.AddData();
        surface.BuildNavMesh();
    }
    private void Update()
    {
        StartCoroutine(CreateNavMesh());
    }

    private IEnumerator CreateNavMesh() 
    {
        while (true)
        {
            yield return new WaitForSeconds(WaitTime);
            NavMeshSurface surface = GetComponent<NavMeshSurface>();
            surface.AddData();
            surface.BuildNavMesh();
        }
    }


}
