
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform Player;
    public Transform lightningPrefab;


    public Rigidbody Fireballprefab;


    public LayerMask whatIsGround, whatIsPlayer;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float TimeBetweenAttacks;
    bool AlreadyAttacked;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    public float attackSpeed;
    public float damage;

    private void Awake()
    {
        Player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) 
        {
            Patroling();
        }
        if (playerInSightRange && !playerInAttackRange) 
        {
            ChasePlayer();
        }
        if (playerInSightRange && playerInAttackRange) 
        {
            AttackPlayer();
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

    private void ChasePlayer()
    {
        agent.SetDestination(Player.position);
    }
    private void AttackPlayer()
    {
        // Make sure the Enemy Doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(Player);

        if (!AlreadyAttacked) 
        {
            // Attock

            Attack();

            AlreadyAttacked = true;
            Invoke(nameof(ResetAttack), TimeBetweenAttacks);
        }
    }

    private void ResetAttack() 
    {
        AlreadyAttacked = false;
    }

    private void Attack() 
    {
        if (gameObject.tag == "Conjuration")
        {
            Rigidbody attack;
            attack = Instantiate(Fireballprefab, transform.position, transform.rotation) as Rigidbody;
            attack.AddForce(transform.forward * attackSpeed);
            Destroy(attack, 5);
        }
        if (gameObject.tag == "Evocation") 
        {
            Instantiate(lightningPrefab, Player.position, transform.rotation);
            Destroy(lightningPrefab, attackSpeed + 0.2f);
        }
    }
}
