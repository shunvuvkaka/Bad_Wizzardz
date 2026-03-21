using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "WindBoost", menuName = "Spell/WindBoost")]
public class WindBoost : BaseSpell
{
    public GameObject windball;
    public float initialSpace;

    public override void Cast()
    {
        base.Cast();

        /* 
         * UNIQUE SPELL CODE HERE
         * "player" is an inherited value of type GameObject
         * have fun!
        */

        GameObject go = Instantiate(windball, player.transform.position, Quaternion.LookRotation(cam.forward, cam.up));
        Windball arrowS = go.GetComponent<Windball>();

        go.transform.position += go.transform.forward * initialSpace;
    }
}