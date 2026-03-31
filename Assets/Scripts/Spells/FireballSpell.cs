using UnityEngine;

[CreateAssetMenu(fileName = "FireballSpell", menuName = "Spell/FireballSpell")]
public class FireballSpell : BaseSpell
{
    public GameObject fireball;
    public float speed = 3;
    public float lifespan = 10;
    public float initialSpace = 5;
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
        Fireball arrowS = go.GetComponent<Fireball>();

        go.transform.position += go.transform.forward * initialSpace;

        arrowS.speed = speed;
        arrowS.lifespan = lifespan;
    }
}