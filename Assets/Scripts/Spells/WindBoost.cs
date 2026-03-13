using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "WindBoost", menuName = "Spell/WindBoost")]
public class WindBoost : BaseSpell
{
    public override void Cast()
    {
        base.Cast();

        /* 
         * UNIQUE SPELL CODE HERE
         * "player" is an inherited value of type GameObject
         * have fun!
        */

        Force(1000f, player.transform.up);
    }
}