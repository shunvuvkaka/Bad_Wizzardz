
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
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
    public float MaxSpeed;

    //Counter
    private int count;


    private void Start()
    {
        Health = MaxHealth;
        Speed = MaxSpeed;
        Mana = MaxMana;
    }
    void Awake()
    {
        Instance = this;
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
    private void Update()
    {
        if (Health <= 0) 
        {
            Destroy(gameObject);
        }
    }


    public float getDamage()
    {
        return Damage * DamageMultiplier;
    }    
}

