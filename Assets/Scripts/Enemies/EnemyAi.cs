using UnityEngine;

public class EnemyAi : Enemy, IDamageable
{
    public Transform model;
    public GameObject attack;
    public LayerMask whatIsGround, whatIsPlayer, whatIsWall;

    [Header("Attacking")]
    public float attackSpeed;
    public float TimeBetweenAttacks;
    [SerializeField] bool AlreadyAttacked = false;
    public float damage;

    [Header("Paramaters")]
    public Vector3 destination;
    public float Offset;

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

        /*
        agent.speed = moveSpeed;
        agent.acceleration = moveSpeed;
        */

    }

    protected override EnemyState Scan()
    {
        EnemyState state = EnemyState.Patroling;
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRange / 2, whatIsPlayer);

        foreach (Collider collider in colliders)
        {
            state = EnemyState.Chasing;
            Vector3 dir = (collider.transform.position - transform.position).normalized;
            float dist = Vector3.Distance(collider.transform.position, transform.position);
            RaycastHit hit;
            Physics.Raycast(transform.position, dir, out hit, dist);

            Debug.DrawRay(transform.position, dir * dist, Color.coral);

            if (dist < attackRange / 2 && hit.transform.tag == "Player")
            {
                state = EnemyState.Acting;
                break;
            }
        }

        return state;
    }

    protected override void Patroling() 
    {
        animator.SetBool("Walking", true);

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet) destination = walkPoint;

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // walkPoint reached
        if (distanceToWalkPoint.magnitude < 1f) 
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint() 
    {
        // Calculate random point in range    public Transform animHolder;
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);


        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround)) walkPointSet = true;

    }

    protected override void Chasing()
    {
        animator.SetBool("Walking", true);
        //agent.SetDestination(player.position);
    }
    protected override void Acting()
    {
        animator.SetBool("Walking", false);
        // Make sure the Enemy Doesn't move
        //agent.SetDestination(transform.position);

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
        
        GameObject go = Instantiate(attack, model.position + transform.forward * 2, Quaternion.LookRotation(direction));
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
            GameplayManager.Instance.addScore += scoreValue;
            particle.Play();
            animator.SetTrigger("Dead");
            Destroy(gameObject, 1f);
        }
    }
}
