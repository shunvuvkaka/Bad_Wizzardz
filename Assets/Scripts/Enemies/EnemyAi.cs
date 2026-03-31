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
    public AudioSource hit;
    public Vector2 pitchRange;

    public GameObject Fireballprefab;


    public LayerMask whatIsGround, whatIsPlayer, whatIsWall;

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

    [Header("Ugrades")]
    public float speedMod;
    public float damageMod;
    public float cooldownMod;
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
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRange / 2, whatIsPlayer);

        foreach (Collider collider in colliders)
        {
            playerInSightRange = true;
            Vector3 dir = (collider.transform.position - transform.position).normalized;
            float dist = Vector3.Distance(collider.transform.position, transform.position);
            RaycastHit hit;
            Physics.Raycast(transform.position, dir, out hit, dist);

            Debug.DrawRay(transform.position, dir * dist, Color.coral);

            if (dist < attackRange / 2 && hit.transform.tag == "Player")
            {
                playerInAttackRange = true;
                break;
            }
        }

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
            Invoke(nameof(ResetAttack), TimeBetweenAttacks + cooldownMod);
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
            FireballDamage fireball = go.GetComponent<FireballDamage>();

            fireball.damage = damage + damageMod;
            fireball.player = Player;
            fireball.speed = attackSpeed + speedMod;

            Destroy(go, 5);
        }
        if (gameObject.tag == "Evocation") 
        {
            
            Instantiate(WarningPrefab, new Vector3(Player.position.x, Player.position.y + Offset, Player.position.z), Quaternion.identity);
            
        }
    }

    public void Damage(float damage)
    {
        hit.pitch = Random.Range(pitchRange.x * 10, pitchRange.y * 10) / 10;
        hit.Play();
        animator.SetTrigger("Hit");
        health -= damage;
        if (health < 0)
        {
            GameplayManager.Instance.addScore += 2000;
            particle.Play();
            PlacementPoints.Instance.enemies.Remove(gameObject);
            animator.SetTrigger("Dead");
            Destroy(gameObject, 1f);
        }
    }
}
