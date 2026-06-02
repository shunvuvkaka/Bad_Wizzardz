using UnityEngine;

public class Bubble : BaseMovingObject
{
    public float dampening;
    public GameObject pop;
    void FixedUpdate()
    {
        velocity = Mathf.Lerp(velocity, 0f, dampening * Time.fixedDeltaTime);

        Loop();
    }

    void OnCollisionEnter(Collision collision)
    {
        //in furture, code here to actually do stuff to what it collides with
    }

    public void Pop() => EndOfLife();
    protected override void EndOfLife()
    {
        GameObject go = Instantiate(pop, transform.position, transform.rotation);
        go.GetComponent<ParticleSystem>().Play();

        Destroy(gameObject); 
    }
}
