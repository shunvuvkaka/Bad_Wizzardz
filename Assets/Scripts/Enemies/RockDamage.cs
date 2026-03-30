using UnityEngine;

public class RockDamage : MonoBehaviour
{

    public PlayerStats playerStats;
    public EnemyAi EnemyAi;
    public float damage;
    public float DestroyTime = 1f;

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
            PlayerMovement player = other.gameObject.GetComponent<PlayerMovement>();
            player.Damage(damage);

            Destroy(gameObject, DestroyTime);
        }
    }
}
