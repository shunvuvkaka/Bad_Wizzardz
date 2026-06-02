using UnityEngine;
/// <summary>
/// Base class any moving object projectile should inherit from to ensure consistent behaviour
/// </summary>
public abstract class BaseMovingObject : MonoBehaviour
{
    [Tooltip("Max velocity (in m/s) of object")]
    public float velocity;
    [Tooltip("Acceleration (in m/s^2) of object")]
    public float acceleration;
    [Tooltip("Lifespan (in s) of object")]
    public float lifespan;
    [Tooltip("Damage to apply on contact")]
    public float damage;
    [Tooltip("Distance away from caster object should spawn")]
    public float initialSpace;
    [Tooltip("Reference to RigidBody that the force should be applied to")]
    public Rigidbody rigidBody;
    //Tag for objects
    private const string OBJECT_TAG = "Object";
    //Tag for enemies
    private const string ENEMY_TAG = "Enemy";
    void Awake()
    {
        transform.position += transform.forward * initialSpace;
    }

    /// <summary>
    /// Basic loop executed every physics update for moving projectiles
    /// Adds velocity and checks lifetime
    /// </summary>
    protected void Loop()
    {
        if (rigidBody.linearVelocity.magnitude < velocity)
            rigidBody.linearVelocity += transform.forward * acceleration;

        if (lifespan < 0)
            EndOfLife();

        lifespan -= Time.fixedDeltaTime;
    }
    /// <summary>
    /// Damage any object that implements the IDamageable interface
    /// </summary>
    /// <param name="collision">Collision information</param>
    protected void Damage(Collision collision)
    {
        IDamageable damageable;

        if (collision.gameObject.TryGetComponent<IDamageable>(out damageable) && collision.transform.tag != "Player")
        {
            damageable.Damage(damage);
        }
    }
    /// <summary>
    /// Adds physics and an explosive force to anyything with the object tag
    /// </summary>
    /// <param name="collision">Collision information</param>
    /// <param name="force">How much force to apply</param>
    /// <param name="pos">Where the force is coming from</param>
    protected void AddPhysics(Collision collision, float force, Vector3 pos)
    {
        if (collision.transform.tag != OBJECT_TAG)
            return;

        Rigidbody rb;

        //Only adds RB if it does not already exist
        if (!collision.gameObject.TryGetComponent<Rigidbody>(out rb))
        {
            rb = collision.gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.mass = 10f;

        Vector3 dir = (collision.transform.position - pos).normalized;

        rb.AddForce(dir * force, ForceMode.Impulse);
    }

    /// <summary>
    /// Executed upon the expiration of the life span of an object
    /// Overriden to have special behaviour on death
    /// </summary>
    protected virtual void EndOfLife()
    {
        Destroy(gameObject);
    }
}
