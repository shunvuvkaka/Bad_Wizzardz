using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public float walkSpeed;
    public bool isWalking = false;
    public Vector2 pitchRange;
    public static PlayerAudio Instance;
    [UnitHeaderInspectable("Sources")]
    public AudioSource walking;
    public AudioSource spell;
    public AudioSource hit;
    public AudioClip[] grunts;

    void Awake()
    {
        Instance = this;
        StartCoroutine(Walk());
    }
    public void Spell()
    {
        spell.Play();
    }
    public void Hit()
    {
        hit.pitch = Random.Range(pitchRange.x * 10, pitchRange.y * 10) / 10 - 0.2f;
        hit.PlayOneShot(grunts[Random.Range(0, grunts.Length)]);
    }

    IEnumerator Walk()
    {
        while (true)
        {
            if (isWalking)
            {
                walking.pitch = Random.Range(pitchRange.x * 10, pitchRange.y * 10 + 1) / 10;

                walking.PlayOneShot(walking.clip);

                yield return new WaitForSeconds(walkSpeed);
            }

            yield return null;
        }
    }
}
