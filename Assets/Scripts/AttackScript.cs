using System.Collections;
using UnityEngine;

public class AttackScript : MonoBehaviour
{

    public float damage;
    public float Speed;
    public float SwoopTime;
    public float xOffset;
    public float yOffset;

    private PlayerStats playerStats;

    public Transform Player;
    public Rigidbody Rb;
    public GameObject ReturnPos;

    public bool IsAttacking;
    public bool GoToPosition;

    private Vector3 Target;
    private Vector3 ReturnTarget;
    private Vector3 position;


    void Start()
    {
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
        Player = GameObject.Find("Player").transform;
        IsAttacking = false;
        GoToPosition = false;
    }

    // Update is called once per frame
    void Update()
    { 
        position = transform.position;
        Target = new Vector3(Player.position.x, Player.position.y, Player.position.z);
        ReturnTarget = ReturnPos.transform.position;
        StartCoroutine(FollowPlayer());
        // Swoop down on player
        if (Player.position.y >= 2)
        {
            StartCoroutine(AttackPlayer());
            new WaitForSeconds(10);
            StopCoroutine(AttackPlayer());
        }
        if (IsAttacking && GoToPosition) 
        {

                IsAttacking = false;
        }
        if (GoToPosition) 
        {
            transform.position = Vector3.MoveTowards(transform.position, ReturnTarget, Speed * Time.deltaTime);
        }
        if (transform.position == ReturnTarget) 
        {
            GoToPosition = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            // Apply damage
            playerStats.Health -= damage;
            GoToPosition = true;
            IsAttacking = false;
        }
    }
    private IEnumerator AttackPlayer() 
    {
        if (!IsAttacking && !GoToPosition)
        {
            yield return new WaitForSeconds(5);
            transform.position = Vector3.MoveTowards(transform.position, Target, Speed * Time.deltaTime);
            IsAttacking = true;
        }
        if (!GoToPosition && IsAttacking) 
        {
            yield return new WaitForSeconds(20);
            GoToPosition = true;
            IsAttacking = false;
        }
    }
    private IEnumerator FollowPlayer() 
    {
        while (true)
        {
            yield return new WaitForSeconds(20);

        }
    }
}
