using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed;
    public float lifespan;
    void Update()
    {
        transform.position += speed * Time.deltaTime * transform.forward;
        lifespan -= Time.deltaTime;

        if (lifespan < 0)
            Destroy(gameObject);
    }
}
