using UnityEngine;

public class RockDamage : MonoBehaviour
{

    public PlayerStats playerStats;
    public EnemyAi EnemyAi;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        EnemyAi = GameObject.Find("EvocationWizard").GetComponent<EnemyAi>();
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player") 
        {
            playerStats.Health -= EnemyAi.damage;
        }
    }
}
