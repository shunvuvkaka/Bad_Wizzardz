using UnityEngine;

public abstract class BaseSpell : ScriptableObject
{
    protected GameObject player;
    protected Transform cam;
    public virtual void Cast()
    {
        try
        {        
            player = GameObject.FindWithTag("Player");
            cam = GameObject.FindWithTag("MainCamera").transform;
        }
        catch (UnityException)
        {
            player = null;
            cam = null;
            Debug.LogWarning("No player found...");
        }
    }
    public void Force(float force, Vector3 dir)
    {
        if (player != null)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            rb.AddForce(dir * force, ForceMode.Impulse);
        }
    }
}
