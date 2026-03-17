using UnityEngine;

public class LightningScript : MonoBehaviour
{
    public PlayerStats playerStats;
    public EnemyAi EnemyAi;
    private void Start()
    {
        EnemyAi = GameObject.Find("Evocation Wizard").GetComponent<EnemyAi>();
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player") 
        {
          playerStats.Health -= EnemyAi.damage;
          Destroy(gameObject);
        }
    }
}
