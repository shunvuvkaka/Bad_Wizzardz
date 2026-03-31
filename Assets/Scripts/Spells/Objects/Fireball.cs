using System.Collections;
using UnityEngine;

public class Fireball : BaseMovingObject
{
    public float explosionRadius;
    public float explosionTime;
    public SphereCollider sphereCollider;
    public ParticleSystem particle;
    public MeshRenderer mr;

    void FixedUpdate()
    {
        Loop();
    }

    void OnCollisionEnter(Collision collision)
    {
        mr.enabled = false;
        
        var main = particle.main;

        main.startSize = explosionRadius * 2;
        main.startLifetime = explosionTime;  

        particle.Play();

        rigidBody.constraints = RigidbodyConstraints.FreezePosition;

        Damage(collision);
        
        AddPhysics(collision, 500f, transform.position);

        StartCoroutine(Explode());
    }

    IEnumerator Explode()
    {
        float currentTIme = explosionTime;
        float radiusSteps = explosionRadius - sphereCollider.radius / explosionTime;

        while (currentTIme > 0)
        {
            sphereCollider.radius += radiusSteps * Time.deltaTime;

            currentTIme -= Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }
}
