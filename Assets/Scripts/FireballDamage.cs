using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class FireballDamage : MonoBehaviour
{
    public PlayerStats playerStats;
    public EnemyAi EnemyAi;
    private float DestroyTime = 1;

    private void Start()
    {
        EnemyAi = GameObject.Find("EnemyWizard").GetComponent<EnemyAi>();
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerStats.Health -= EnemyAi.damage;

            Destroy(gameObject, DestroyTime);
        }
    }
}
