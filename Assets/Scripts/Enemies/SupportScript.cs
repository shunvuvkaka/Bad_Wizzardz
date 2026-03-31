using UnityEngine;

public class SupportScript : Enemy, IDamageable
{

    public Transform Enemy;
    public LayerMask whatIsEnemy;
    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    public float moveSpeed;
    public Rigidbody rb;
    public Transform animHolder;
    public LineRenderer lr;
    private Transform player;
    private Animator animator; 
    public ParticleSystem particle;
    public AudioSource hit;
    public Vector2 pitchRange;
    private EnemyAi ai;

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
    public float maxHealth;
    public float health;
    public float maxDist;

    private float RandomBoost;
    bool moving = false;
    void Awake()
    {
        player = GameObject.Find("Player").transform;
        transform.position += Vector3.up * 10;
        animator = animHolder.GetComponent<Animator>();
        health = maxHealth;
    }

    private void FixedUpdate()
    {
        //Check for sight and Support range
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRange / 2, whatIsEnemy);

        foreach (Collider collider in colliders)
        {
            EnemyInSightRange = true;
            Enemy = collider.transform;

            if (Vector3.Distance(collider.transform.position, transform.position) < SupportRange / 2)
            {
                EnemyInSupportRange = true;
                break;
            }
        }

        if (!EnemyInSightRange && !EnemyInSupportRange)
        {
            lr.positionCount = 0;
            Patroling();
        }
        if (EnemyInSightRange && !EnemyInSupportRange && Enemy != null)
        {
            lr.positionCount = 0;
            ChaseEnemy();
        }
        if (EnemyInSightRange && EnemyInSupportRange && Enemy != null)
        {
            SupportEnemy();
        }

        if (moving)
            Move(walkPoint);
        else
            animator.SetBool("Moving", false);

        if (Vector3.Distance(transform.position, player.position) > maxDist)
        {
            PlacementPoints.Instance.enemies.Remove(gameObject);
            Destroy(gameObject);
        }
    }
    void Move(Vector3 movePoint)
    {
        animator.SetBool("Moving", true);
        transform.LookAt(movePoint);

        rb.AddForce(transform.forward * moveSpeed, ForceMode.Acceleration);
    }

    private void Patroling()
    {
        if (!walkPointSet) 
        {
            moving = false;
            SearchWalkPoint();
        }

        if (walkPointSet) moving = true;

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
        float randomY = Random.Range(-walkPointRange, walkPointRange);


        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y + randomY, transform.position.z + randomZ);

        Vector3 direction = (walkPoint - transform.position).normalized;

        if (!Physics.Raycast(transform.position, direction)) walkPointSet = true;

    }

    private void ChaseEnemy()
    {
        walkPoint = Enemy.position;

        moving = true;
    }
    private void SupportEnemy()
    {
        transform.LookAt(Enemy);

        lr.positionCount = 2;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, Enemy.position);

        moving = false;

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
        animator.SetTrigger("Attack");
        RandomBoost = Random.Range(0, 3);
        ai = Enemy.GetComponent<EnemyAi>();

        if (ai.health < ai.MaxHealth - HealthBoost)
        {
            ai.health += HealthBoost;
        }

        if (RandomBoost == 1)
        {
            ai.speedMod = SpeedBoost;

        }
        else if (RandomBoost == 2)
        {
            ai.damageMod = DamageBoost;
        }
        else if (RandomBoost == 3) 
        {
            ai.cooldownMod = -0.5f;
        }
        else
        {
         //   Debug.LogError("Number not known");
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
            if (ai != null)
            {
                ai.speedMod = 0;
                ai.damageMod = 0;
                ai.cooldownMod = 0;
            }
            GameplayManager.Instance.addScore += 1000;
            particle.Play();
            PlacementPoints.Instance.enemies.Remove(gameObject);
            animator.SetTrigger("Dead");
            Destroy(gameObject, 1f);
        }
    }
}
