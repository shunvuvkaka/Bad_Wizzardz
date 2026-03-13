using UnityEngine;

[CreateAssetMenu(fileName = "ExampleSpell", menuName = "Spell/ExampleSpell")]
public class ExampleSpell : BaseSpell
{
    public override void Cast()
    {
        base.Cast();

        /* 
         * UNIQUE SPELL CODE HERE
         * "player" is an already provided value of type GameObject
         * have fun!
        */

        Debug.Log("Just casted an example spell!");
    }
}