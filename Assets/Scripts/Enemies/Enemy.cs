using UnityEngine;

public abstract class Enemy : MonoBehaviour
{  
    [Header("References")]
    public Animator animator; 
    protected Transform player;
    public Rigidbody rb;
    [Header("Audio")]
    public ParticleSystem particle;
    public AudioSource hit;
    public Vector2 pitchRange;
    [Header("Movement")]
    public Vector3 walkPoint;
    protected bool walkPointSet;
    public float walkPointRange;
    public float maxDist;
    public float sightRange, attackRange;
    [Header("Stats")]
    public float maxHealth;
    public float health;
    public float moveSpeed;
    public int scoreValue;
    public enum EnemyState
    {
        Patroling,
        Chasing,
        Acting
    }
    public EnemyState currentState;
    void Awake()
    {
        Iniitialise();
    }

    void OnDestroy()
    {
        PlacementPoints.Instance.enemies.Remove(this);
    }

    protected virtual void Iniitialise()
    {
        player = GameObject.Find("Player").transform;
        health = maxHealth;
    }
    /// <summary>
    /// Main loop for enemies, finding and acting upon their current state
    /// </summary>

    protected virtual void EnemyLogic()
    {
        currentState = Scan();

        switch (currentState)
        {
            case EnemyState.Patroling:
                Patroling();
                break;
            case EnemyState.Chasing:
                Chasing();
                break;
            case EnemyState.Acting:
                Acting();
                break;
        }
        if (Vector3.Distance(transform.position, player.position) > maxDist)
        {
            Destroy(gameObject);
        }

    }
    /// <summary>
    /// This code should contain the logic for the enemy deciding
    /// whether its target is in signt or in acting range
    /// </summary>
    protected abstract EnemyState Scan();
    /// <summary>
    /// Executed when the enemy is neither in sight or range of its target
    /// </summary>
    protected abstract void Patroling();
    /// <summary>
    /// Executed when the enemy is in sight, but not in range of its target
    /// </summary>
    protected abstract void Chasing();
    /// <summary>
    /// Executed when the enemy is in range of its target
    /// </summary>
    protected abstract void Acting();

}
