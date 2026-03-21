using System.Collections;
using UnityEngine;

public class Windball : BaseMovingObject
{
    public SphereCollider sphereCollider;
    public float boostRadius;
    public float boost;
    void FixedUpdate()
    {
        Loop();
    }

    void OnCollisionEnter(Collision collision)
    {
        rigidBody.constraints = RigidbodyConstraints.FreezePosition;

        StartCoroutine(Boom());

        if (collision.transform.tag == "Player" || collision.transform.tag == "Spell")
        {
            Rigidbody rb = collision.transform.GetComponent<Rigidbody>();
            Vector3 dir = (collision.transform.position - transform.position).normalized;

            rb.AddForce(dir * boost, ForceMode.Impulse);
        }
    }

    IEnumerator Boom()
    {
        sphereCollider.radius = boostRadius;

        yield return new WaitForSeconds(0.5f);

        Destroy(gameObject);
    }
}
