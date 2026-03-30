using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : Enemy, IDamageable
{
    public NavMeshAgent agent;

    public Transform Player;
    public Transform model;
    public GameObject WarningPrefab;
    public GameObject RockPrefab;
    public Transform animHolder;
    public ParticleSystem particle;
    private Animator animator; 


    public GameObject Fireballprefab;


    public LayerMask whatIsGround, whatIsPlayer;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    public float maxDist;

    // Attacking
    public float TimeBetweenAttacks;
    [SerializeField] bool AlreadyAttacked;
    public float damage;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    public float attackSpeed;
    public float Offset;
    public float health;
    public float MaxHealth;
    private Vector3 direction;

    private void Awake()
    {
        Player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        AlreadyAttacked = false;
        TerrainGenerator.onGenerate += SearchWalkPoint;
        animator = animHolder.GetComponent<Animator>();
        health = MaxHealth;
    }
    void OnDestroy()
    {
        TerrainGenerator.onGenerate -= SearchWalkPoint;
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

        if (Vector3.Distance(transform.position, Player.position) > maxDist)
        {
            PlacementPoints.Instance.enemies.Remove(gameObject);
            Destroy(gameObject);
        }
    }

    private void Patroling() 
    {
        animator.SetBool("Walking", true);
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
        animator.SetBool("Walking", true);
        agent.SetDestination(Player.position);
    }
    private void AttackPlayer()
    {
        animator.SetBool("Walking", false);
        // Make sure the Enemy Doesn't move
        agent.SetDestination(transform.position);

        Debug.DrawLine(model.position, Player.position, Color.rebeccaPurple);

        direction = (Player.position - model.position).normalized;

        transform.LookAt(Player);
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);

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
        animator.SetTrigger("Attack");
        if (gameObject.tag == "Conjuration")
        {
            GameObject go = Instantiate(Fireballprefab, model.position + transform.forward * 2, Quaternion.LookRotation(direction));
            Rigidbody attack = go.GetComponent<Rigidbody>();
            FireballDamage fireball = go.GetComponent<FireballDamage>();

            fireball.damage = damage;
            fireball.player = Player;
            fireball.speed = attackSpeed;

            Destroy(go, 5);
        }
        if (gameObject.tag == "Evocation") 
        {
            
            Instantiate(WarningPrefab, new Vector3(Player.position.x, Player.position.y + Offset, Player.position.z), Quaternion.identity);
            
        }
    }

    public void Damage(float damage)
    {
        animator.SetTrigger("Hit");
        health -= damage;
        if (health < 0)
        {
            particle.Play();
            PlacementPoints.Instance.enemies.Remove(gameObject);
            animator.SetTrigger("Dead");
            Destroy(gameObject, 1f);
        }
    }
}
