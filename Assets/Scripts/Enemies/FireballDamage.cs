using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class FireballDamage : MonoBehaviour
{
    public float damage;
    public float DestroyTime = 1;
    public float steering;
    public float speed;
    public Transform player;
    private Rigidbody rb;
    private bool collided = false;
    void Awake()
    {
        rb = transform.GetComponent<Rigidbody>();
    }
    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player" && !collided)
        {
            PlayerMovement player = other.gameObject.GetComponent<PlayerMovement>();
            player.Damage(damage);
            Destroy(gameObject, DestroyTime);

            collided = true;
        }
    }

    void FixedUpdate()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, steering * Time.fixedDeltaTime);

        rb.AddForce(transform.forward * speed);
    }
}