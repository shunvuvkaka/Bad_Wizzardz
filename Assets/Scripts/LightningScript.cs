using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class LightningScript : MonoBehaviour
{
    public PlayerStats playerStats;
    public EnemyAi EnemyAi;
    public GameObject Player;

    public GameObject RockPrefab;

    public float AttackTimer;
    public float AttackIntervals;
    public float YOffset;
    public float YOffset2;

    private bool Strike;

    private void Start()
    {
        EnemyAi = GameObject.Find("EvocationWizard").GetComponent<EnemyAi>();
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
        Player = GameObject.Find("Player");
        RockPrefab = GameObject.Find("Rock");
        AttackTimer = AttackIntervals;
    }
    private void Update()
    {
        {
            
            AttackTimer -= Time.deltaTime;
            if (AttackTimer <= 0)
            {
                Instantiate(RockPrefab, new Vector3(transform.position.x, transform.position.y + YOffset, transform.position.z), transform.rotation);
                if (gameObject.name != "WarningCircle")
                {
                    Destroy(gameObject);
                }
                AttackTimer = AttackIntervals;
            }
            if (AttackTimer >= 1)
            {
                if (gameObject.name != "WarningCircle")
                {
                    transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y - YOffset2, Player.transform.position.z);
                }
            }
        }
    }
 
}
