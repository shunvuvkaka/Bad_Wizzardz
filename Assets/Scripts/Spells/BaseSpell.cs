using UnityEngine;

public abstract class BaseSpell : ScriptableObject
{
    protected GameObject player;
    protected Transform cam;
    public float manaCost;
    public float healthCost;
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

        if (PlayerStats.Instance.Mana > manaCost)
        {
            PlayerStats.Instance.Mana -= manaCost;
        }
        else
        {
            PlayerMovement.Instance.Damage(healthCost);
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
