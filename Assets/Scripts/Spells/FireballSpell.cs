using UnityEngine;

[CreateAssetMenu(fileName = "FireballSpell", menuName = "Spell/FireballSpell")]
public class FireballSpell : BaseSpell
{
    public GameObject fireball;
    public override void Cast()
    {
        base.Cast();

        /* 
         * UNIQUE SPELL CODE HERE
         * "player" is an already provided value of type GameObject
         * so is cam
         * have fun!
        */

        GameObject go = Instantiate(fireball, player.transform.position, Quaternion.LookRotation(cam.forward, cam.up));
    }
}