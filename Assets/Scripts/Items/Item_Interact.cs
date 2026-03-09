using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class Item_Interact : MonoBehaviour
{
    public float Healthboost;
    public float Manaboost;
    public float PotionAmount;

    public PlayerStats Stats;
    public Transform Potion;

    private void Start()
    {
       Stats = GameObject.Find("Player").GetComponent<PlayerStats>();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && Input.GetKey("e")) 
        {
 

            //Apply Stat Boosts
            Stats.Health += Healthboost;
                Stats.Mana += Manaboost;

                if (Stats.Mana > Stats.MaxMana)
                {
                    Stats.Mana = Stats.MaxMana;
                }
                if (Stats.Health > Stats.MaxHealth)
                {
                    Stats.Health = Stats.MaxHealth;
                }

            // Destroy Item
            PotionAmount -= 1;
            Destroy(gameObject);
                

        }
    }
}
