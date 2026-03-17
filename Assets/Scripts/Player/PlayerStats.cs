
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float Health;
    public float MaxHealth;
    public float Damage;
    public float Speed;
    public float MaxSpeed;
    public float Mana;
    public float MaxMana;

    private void Start()
    {
        Health = MaxHealth;
        Speed = MaxSpeed;
        Mana = MaxMana;

    }
    private void Update()
    {
        if (Health <= 0) 
        {
            Destroy(gameObject);
        }
    }
}

