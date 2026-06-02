using UnityEngine;

public class SupportScript : Enemy, IDamageable
{
    [Header("References")]
    public Transform Enemy;
    public LayerMask whatIsEnemy;
    public LineRenderer lr;
    private EnemyAi ai;

    [Header("Stats")]
    public float TimeBetweenSupport;
    bool AlreadySupported;
    public float SupportSpeed;
    public float HealthBoost;
    public float SpeedBoost;
    public float DamageBoost;

    private float RandomBoost;
    bool moving = false;
    protected override void Iniitialise()
    {
        base.Iniitialise();

        transform.position += Vector3.up * 10;
    }

    private void FixedUpdate()
    {
        EnemyLogic();
    }

    protected override void EnemyLogic()
    {
        base.EnemyLogic();

        if (moving)
            Move(walkPoint);
        else
            animator.SetBool("Moving", false);
    }

    protected override EnemyState Scan()
    {
        //Check for sight and Support range
        EnemyState state = EnemyState.Patroling;
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRange / 2, whatIsEnemy);

        foreach (Collider collider in colliders)
        {
            Vector3 dir = (collider.transform.position - transform.position).normalized;
            float dist = Vector3.Distance(collider.transform.position, transform.position);

            Debug.DrawRay(transform.position, dir * dist, Color.deepPink);

            if (Physics.Raycast(transform.position, dir, dist, ~whatIsEnemy))
                continue;
            
            Enemy = collider.transform;

            if (Enemy == null)
                break;

            state = EnemyState.Chasing;

            if (dist < attackRange / 2)
            {
                state = EnemyState.Acting;
                break;
            }
        }

        return state;
    }
    void Move(Vector3 movePoint)
    {
        animator.SetBool("Moving", true);
        transform.LookAt(movePoint);

        rb.AddForce(transform.forward * moveSpeed, ForceMode.Acceleration);
    }

    protected override void Patroling()
    {
        lr.positionCount = 0;

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

    protected override void Chasing()
    {
        lr.positionCount = 0;

        walkPoint = Enemy.position;

        moving = true;
    }
    protected override void Acting()
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
        
        if (!Enemy.parent.TryGetComponent<EnemyAi>(out EnemyAi ai))
            return;

        Debug.Log("trying support");

        if (RandomBoost == 1)
        {
            ai.speedMod = SpeedBoost;

        }
        else if (RandomBoost == 2)
        {
            ai.damageMod = DamageBoost;
        }
        else
        {
            ai.cooldownMod = -0.5f;
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
            GameplayManager.Instance.addScore += scoreValue;
            particle.Play();
            PlacementPoints.Instance.enemies.Remove(this);
            animator.SetTrigger("Dead");
            Destroy(gameObject, 1f);
        }
    }
}
