using UnityEngine;

[CreateAssetMenu(fileName = "Dash", menuName = "Spell/Dash")]
public class Dash : BaseSpell
{
    public override void Cast()
    {
        base.Cast();

        /* 
         * UNIQUE SPELL CODE HERE
         * "player" is an already provided value of type GameObject
         * have fun!
        */

        Force(2000f, cam.forward);
    }
}