using UnityEngine;

public class MoveCamera : MonoBehaviour
{

    public Transform Player;

    void Update()
    {
        transform.position = Player.transform.position;
    }
}
