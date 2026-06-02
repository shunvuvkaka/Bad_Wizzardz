using UnityEngine;

[CreateAssetMenu(fileName = "PowerBlast", menuName = "Spell/PowerBlast")]
public class PowerBlast : BaseSpell
{
    //Executed immediately after cast
    public override void Cast()
    {
        base.Cast();

        /* 
         * UNIQUE SPELL CODE HERE
         * Please visit the BaseSpell script and other base scripts to review helper functions
         * have fun!
        */

        Debug.Log("Just casted an example spell!");
    }
}