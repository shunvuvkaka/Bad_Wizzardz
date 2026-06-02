using UnityEngine;

/// <summary>
/// Base class for all spells to inherit from
/// </summary>
public abstract class BaseSpell : ScriptableObject
{
    protected GameObject player;
    protected Transform cam;
    [Tooltip("Amount of mana to deduct upon cast")]
    public float manaCost;
    [Tooltip("Amount of helth to deduct upon cast if mana is not sufficient")]
    public float healthCost;

    /// <summary>
    /// Called upon cast from all spells retrieving the player and calling events associated with spellcasting
    /// </summary>
    public virtual void Cast()
    {
        //Very unlikely catch would ever be called, good backup nonetheless
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
            return;
        }

        //Deduct relevent stats
        if (PlayerStats.Instance.Mana > manaCost)
        {
            PlayerStats.Instance.Mana -= manaCost;
        }
        else
        {
            PlayerMovement.Instance.Damage(healthCost);
        }
    }
    /// <summary>
    /// Helper function to apply a force to the player
    /// </summary>
    /// <param name="force">Amount of force to apply</param>
    /// <param name="dir">Direction the force is coming from</param>
    public void Force(float force, Vector3 dir)
    {
        if (player != null)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            rb.AddForce(dir * force, ForceMode.Impulse);
        }
    }
}
