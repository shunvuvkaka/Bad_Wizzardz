using System.Collections;
using UnityEngine;

public class TargetPosition : MonoBehaviour
{

    public Transform Player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = new Vector3(Player.position.x + Random.Range(0, 4), 10f, Player.position.z + Random.Range(-2, 4));
        StartCoroutine(ChangePosition());
    }

    private IEnumerator ChangePosition() 
    {
        yield return new WaitForSeconds(5);
        transform.position = new Vector3(Player.position.x + Random.Range(0, 4), 10f, Player.position.z + Random.Range(-2, 4));
    }
}
