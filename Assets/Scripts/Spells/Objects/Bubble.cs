using UnityEngine;

public class Bubble : BaseMovingObject
{
    public float dampening;
    public GameObject pop;
    void FixedUpdate()
    {
        speed = Mathf.Lerp(speed, 0f, dampening * Time.fixedDeltaTime);

        //effectively hijacking loops destruction system
        if (lifespan < 0)
        {
            Pop();
            return;
        }

        Loop();
    }

    void OnCollisionEnter(Collision collision)
    {
        //in furture, code here to actually do stuff to what it collides with
    }

    public void Pop()
    {
        GameObject go = Instantiate(pop, transform.position, transform.rotation);
        go.GetComponent<ParticleSystem>().Play();

        Destroy(gameObject); 
    }
}
