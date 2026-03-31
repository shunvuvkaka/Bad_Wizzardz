using UnityEngine;

public abstract class BaseMovingObject : MonoBehaviour
{
    public float speed;
    public float lifespan;
    public float damage;
    public Rigidbody rigidBody;
    private const string OBJECT_TAG = "Object";
    private const string ENEMY_TAG = "Enemy";
    protected void Loop()
    {
        rigidBody.linearVelocity += transform.forward * speed;

        if (lifespan < 0)
            Destroy(gameObject);

        lifespan -= Time.fixedDeltaTime;
    }
    protected void Damage(Collision collision)
    {
        IDamageable damageable;

        if (collision.gameObject.TryGetComponent<IDamageable>(out damageable) && collision.transform.tag != "Player")
        {
            damageable.Damage(damage);
        }
    }
    protected void AddPhysics(Collision collision, float force, Vector3 pos)
    {
        if (collision.transform.tag != OBJECT_TAG)
            return;

        Rigidbody rb;

        if (!collision.gameObject.TryGetComponent<Rigidbody>(out rb))
        {
            rb = collision.gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.mass = 10f;

        Vector3 dir = (collision.transform.position - pos).normalized;

        rb.AddForce(dir * force, ForceMode.Impulse);
    }
}
