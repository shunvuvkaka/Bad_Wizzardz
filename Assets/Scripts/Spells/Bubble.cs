using UnityEngine;

public class Bubble : MonoBehaviour
{
    public float speed;
    public float lifespan;
    public float dampening;
    void Update()
    {
        speed = Mathf.Lerp(speed, 0f, dampening * Time.deltaTime);

        transform.position += speed * Time.deltaTime * transform.forward;
        lifespan -= Time.deltaTime;

        if (lifespan < 0)
            Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        //in furture, code here to actually do stuff to what it collides with
    }


}
