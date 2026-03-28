
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.AI;

public class SupportScript : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform Enemy;


    public LayerMask whatIsGround, whatIsEnemy;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Supporting
    public float TimeBetweenSupport;
    bool AlreadySupported;

    // Stas
    public float sightRange, SupportRange;
    public bool EnemyInSightRange, EnemyInSupportRange;
    public float SupportSpeed;
    public float HealthBoost;
    public float SpeedBoost;
    public float DamageBoost;

    private float RandomBoost;

    private void Awake()
    {
        Enemy = GameObject.Find("EnemyWizard").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //Check for sight and Support range
        EnemyInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsEnemy);
        EnemyInSupportRange = Physics.CheckSphere(transform.position, SupportRange, whatIsEnemy);

        if (EnemyInSightRange && !EnemyInSupportRange)
        {
            Patroling();
        }
        if (EnemyInSightRange && !EnemyInSupportRange)
        {
            ChaseEnemy();
        }
        if (EnemyInSightRange && EnemyInSupportRange)
        {
            SupportEnemy();
        }
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet) agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // walkPoint reached
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        // Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);


        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround)) walkPointSet = true;

    }

    private void ChaseEnemy()
    {
        agent.SetDestination(Enemy.position);
    }
    private void SupportEnemy()
    {
        // Make sure the Enemy Doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(Enemy);

        if (!AlreadySupported)
        {
            // Attock

            Support();

            AlreadySupported = true;
            Invoke(nameof(ResetSupport), TimeBetweenSupport);
        }
    }

    private void ResetSupport()
    {
        AlreadySupported = false;
    }

    private void Support()
    {
        RandomBoost = Random.Range(0, 3);
        if (RandomBoost == 1)
        {

        }
        else if (RandomBoost == 2)
        {

        }
        else if (RandomBoost == 3) 
        {
        
        }
        else
        {
         //   Debug.LogError("Number not known");
        }
    }
}
