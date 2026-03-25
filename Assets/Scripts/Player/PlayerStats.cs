using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    //Health
    public float Health;
    public float HealthRegen;     //per second
    public float MaxHealth;

    //Damage
    public readonly float Damage = 1f;
    public float DamageMultiplier;

    //Mana
    public float Mana;
    public float ManaRegen;       //per second
    public float MaxMana;

    //Speed
    public float Speed;

    //Counter
    private int count;


    private void Start()
    {
        MaxHealth = 100;
        Health = 100;
        HealthRegen = 0;
        MaxMana = 100;
        Mana = 100;
        ManaRegen = 0;
        Speed = 1;
        DamageMultiplier = 1;
    }

    private void FixedUpdate()
    {
        if (count % 50 == 0 )
        {
            Health += HealthRegen;
            if (Health > MaxHealth) { Health = MaxHealth; }
            Mana += ManaRegen;
            if (Mana > MaxMana) { Mana = MaxMana; }
        }
        ++count;
    }


    public float getDamage()
    {
        return Damage * DamageMultiplier;
    }    
}
