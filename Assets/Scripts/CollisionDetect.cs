using UnityEngine;

public class CollisionDetect : MonoBehaviour
{
    public float Healthboost;
    public float Damageboost;
    public float Speedboost;
    public float Manaboost;

    public PlayerStats Stats;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player") 
        {
            //Apply Stat Boosts
            Stats.Health += Healthboost;
            Stats.Damage += Damageboost;
            Stats.Speed += Speedboost;
            Stats.Mana += Manaboost;

            if (Stats.Mana > Stats.MaxMana)
            {
                Stats.Mana = Stats.MaxMana;
            } 
            if (Stats.Health > Stats.MaxHealth)
            {
                Stats.Health = Stats.MaxHealth;
            }
            if (Stats.Speed > Stats.MaxSpeed)
            {
                Stats.Speed = Stats.MaxSpeed;
            }

            // Destroy Item
            Destroy(gameObject);
        }
    }
}
