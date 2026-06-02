using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyAi : Enemy, IDamageable
{
    public Transform model;
    public GameObject attack;
    public LayerMask whatIsGround, whatIsPlayer, whatIsWall;
    public EnemyMovement agent;

    [Header("Attacking")]
    public float attackSpeed;
    public float TimeBetweenAttacks;
    [SerializeField] bool AlreadyAttacked = false;
    public float damage;

    [Header("Paramaters")]
    public Vector3 destination;
    public float offset;

    [Header("Ugrades")]
    public float speedMod;
    public float damageMod;
    public float cooldownMod;
    private Vector3 direction;

    private void Update()
    {
        EnemyLogic();
    }
    protected override void Iniitialise()
    {
        base.Iniitialise();

        agent = transform.GetComponent<EnemyMovement>();
        
        agent.speed = moveSpeed;
        agent.acceleration = acceleration;
    }

    protected override EnemyState Scan()
    {
        EnemyState state = EnemyState.Patroling;
        Vector3 dir = (player.transform.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, dir, sightRange, whatIsPlayer))
        {
            state = EnemyState.Chasing;
            Physics.Raycast(transform.position, dir, out RaycastHit hit, attackRange, whatIsPlayer + whatIsWall);

            if (hit.collider != null && hit.transform.CompareTag("Player"))
            {
                Debug.DrawRay(transform.position, dir * hit.distance, Color.coral);
                state = EnemyState.Acting;
            }
        }

        return state;
    }

    protected override void Patroling() 
    {
        animator.SetBool("Walking", true);

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet) 
        {
            agent.SetDestination(walkPoint);
            agent.moving = true;
        }

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

    protected override void Chasing()
    {
        animator.SetBool("Walking", true);
        agent.SetDestination(player.position);
        agent.moving = true;
    }
    protected override void Acting()
    {
        animator.SetBool("Walking", false);
        // Make sure the Enemy Doesn't move
        agent.SetDestination(transform.position);
        agent.moving = false;

        Debug.DrawLine(model.position, player.position, Color.rebeccaPurple);

        direction = (player.position - model.position).normalized;

        transform.LookAt(player);
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
        
        GameObject go = Instantiate(attack, model.position + transform.forward * offset, Quaternion.LookRotation(direction));
        FireballDamage fireball = go.GetComponent<FireballDamage>();

        fireball.damage = damage + damageMod;
        fireball.player = player;
        fireball.speed = attackSpeed + speedMod;

        Destroy(go, 5);
    }

    public void Damage(float damage)
    {
        hit.pitch = Random.Range(pitchRange.x * 10, pitchRange.y * 10) / 10;
        hit.Play();
        animator.SetTrigger("Hit");
        health -= damage;

        if (health < 0)
        {
            particle.Play();
            animator.SetTrigger("Dead");
            Destroy(gameObject, 1f);
            
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.addScore += scoreValue;
        }
    }
}
