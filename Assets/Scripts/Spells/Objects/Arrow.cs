using UnityEngine;

public class Arrow : BaseMovingObject
{
    void FixedUpdate()
    {
        Loop();
    }

    void OnCollisionEnter(Collision collision)
    {
        Damage(collision);

        if (collision.gameObject.TryGetComponent<Bubble>(out Bubble bubble))
        {
            bubble.Pop();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
