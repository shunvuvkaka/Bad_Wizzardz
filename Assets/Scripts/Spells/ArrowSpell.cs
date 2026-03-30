using UnityEngine;

[CreateAssetMenu(fileName = "ArrowSpell", menuName = "Spell/ArrowSpell")]
public class ArrowSpell : BaseSpell
{
    public GameObject arrow;
    public float speed = 10;
    public float lifespan = 2;
    public float initialSpace = 1;
    public override void Cast()
    {
        base.Cast();

        /* 
         * UNIQUE SPELL CODE HERE
         * "player" is an already provided value of type GameObject
         * have fun!
        */
        Force(600f, -player.transform.forward);

        GameObject go = Instantiate(arrow, player.transform.position, Quaternion.LookRotation(cam.forward, cam.up));
        Arrow arrowS = go.GetComponent<Arrow>();

        go.transform.position += go.transform.forward * initialSpace;

        arrowS.speed = speed;
        arrowS.lifespan = lifespan;
    }
}