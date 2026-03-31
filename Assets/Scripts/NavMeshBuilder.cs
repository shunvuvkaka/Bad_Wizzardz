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
        Debug.Log("attempting Nav Mesh");

        NavMeshSurface surface = GameObject.Find("NavMesh").GetComponent<NavMeshSurface>();
        surface.AddData();
        surface.BuildNavMesh();
        Debug.Log("Created NavMesh");

        StartCoroutine(CreateNavMesh());
    }
    private void Update()
    {
        
    }

    private IEnumerator CreateNavMesh() 
    {
            yield return new WaitForSeconds(WaitTime);
            NavMeshSurface surface = GetComponent<NavMeshSurface>();
            surface.AddData();
            surface.BuildNavMesh();
    }


}
