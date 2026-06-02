using UnityEngine;

[CreateAssetMenu(fileName = "BubbleSpell", menuName = "Spell/BubbleSpell")]
public class BubbleSpell : BaseSpell
{
    public int amount;
    public GameObject bubble;
    public override void Cast()
    {
        base.Cast();

        /* 
         * UNIQUE SPELL CODE HERE
         * "player" is an already provided value of type GameObject
         * have fun!
        */

        float angle = 360 / amount;

        for (int i = 0; i < amount; i++)
        {
            GameObject go = Instantiate(bubble, player.transform.position, 
                                        Quaternion.Euler(0, angle * i, 0));
        }
    }
}