using System.Collections;
using UnityEngine;

public class Windball : BaseMovingObject
{
    public SphereCollider sphereCollider;
    public float boostRadius;
    public MeshRenderer mr;
    public float boost;
    public float boostTime;
    public ParticleSystem particle;
    void FixedUpdate()
    {
        Loop();
    }

    void OnCollisionEnter(Collision collision)
    {
        rigidBody.constraints = RigidbodyConstraints.FreezePosition;
        mr.enabled = false;
        particle.Play();

        StartCoroutine(Explode());

        if (collision.transform.tag == "Player" || collision.transform.tag == "Spell")
        {
            Rigidbody rb = collision.transform.GetComponent<Rigidbody>();
            Vector3 dir = (collision.transform.position - transform.position).normalized;

            rb.AddForce(dir * boost, ForceMode.Impulse);
        }
    }

    IEnumerator Explode()
    {
        float currentTIme = boostTime;
        float radiusSteps = boostRadius - sphereCollider.radius / boostTime;

        while (currentTIme > 0)
        {
            sphereCollider.radius += radiusSteps * Time.deltaTime;

            currentTIme -= Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }
}
