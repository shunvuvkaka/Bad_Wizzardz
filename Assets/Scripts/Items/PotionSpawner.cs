using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
public class PotionSpawner : MonoBehaviour
{

    public GameObject[] Potions;
    public Transform Player;
    public Item_Interact ItemInteract;

    public float YPosition;
    public float timer;
    public float Intervals;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= Intervals)
        {
            // Spawn Potions around Player
            Instantiate(Potions[Random.Range(0, 2)], new Vector3(Player.position.x + Random.Range(-8, 4), YPosition, Player.position.z + Random.Range(-10, 4)), Player.rotation);
            ItemInteract.PotionAmount += 1;
            timer = 0;
        }

    }
}




