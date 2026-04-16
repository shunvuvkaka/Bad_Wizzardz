using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public Transform centerPoint;
    [Header("Paramaeters")]
    public float speed;
    public float acceleration;
    public float rotationSpeed;
    public float characterWidth;
    public int rotationPrecision = 10;
    [SerializeField] private LayerMask obstacles;
    [Header("Distances")]
    public float detectionDistance;
    public float avoidanceDistance;
    public float checkDistance = 3f;
    [Header("Destinations")]
    [SerializeField] private Vector3 destination;
    [SerializeField] private Vector3 currentTarget;
    [SerializeField] private Vector3 currentDir;
    private Vector3 position;
    public bool moving = true;
    private bool frame = true;

    void Awake()
    {
        rb = transform.GetComponent<Rigidbody>();
    }

    public void SetDestination(Vector3 pos)
    {
        destination = pos;
    }

    void FixedUpdate()
    {
        position = transform.position;

        if (moving)
            Movement();
        else
            rb.linearVelocity = Vector3.zero;
    }
    void Update()
    {
        if (frame)
            Pathfinding();
        
        frame = !frame;
    }

    void Movement()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(currentDir.x, 0, currentDir.z)), rotationSpeed);

        if (rb.linearVelocity.magnitude < speed)
        {
            rb.AddForce(transform.forward * acceleration, ForceMode.VelocityChange);
        }
    }

    void Pathfinding()
    {
        Vector3 realDir = (destination - position).normalized;
        Vector3 targetDir = realDir;
        float targetDist = Vector3.Distance(position, destination);

        Physics.SphereCast(centerPoint.position, characterWidth, targetDir, out RaycastHit hit, targetDist, obstacles);

        if (hit.collider == null)
        {
            currentDir = targetDir;
            currentTarget = destination;
            Debug.DrawRay(centerPoint.position, targetDir * targetDist, Color.green);
            Debug.DrawLine(position, currentTarget, Color.blue);
            return;
        }

        targetDist = hit.distance - detectionDistance;

        if (targetDist > avoidanceDistance)
        {
            currentDir = targetDir;
            currentTarget = position + targetDir * targetDist;
            Debug.DrawRay(centerPoint.position, targetDir * targetDist, Color.yellow);
            Debug.DrawLine(position, currentTarget, Color.blue);
            return;
        }

        //actual avoidance logic here
        //operating on a two dimensional plane where base dir is the original angle

        Vector3 baseDirection = currentDir;
        Vector3 bestDirection = Vector3.zero;

        if (currentDir == null)
            currentDir = (destination - position).normalized;

        //angle represents a angle in degrees radians that increases each iteration
        for (float angle = 0; angle <= Mathf.PI; angle += Mathf.PI / rotationPrecision)
        {
            Vector3 leftCheck = CheckDirection(baseDirection, angle);
            Vector3 rightCheck = CheckDirection(baseDirection, -angle);

            //note bias towards left side, consequence of linear execution
            if (leftCheck != Vector3.zero)
            {
                bestDirection = leftCheck;
                break;
            }

            if (rightCheck != Vector3.zero)
            {
                bestDirection = rightCheck;
                break;
            }
            
        }

        currentTarget = position + bestDirection * checkDistance;
        currentDir = bestDirection;

        Debug.DrawLine(position, currentTarget, Color.red);
    }

    Vector3 CheckDirection(Vector3 baseDirection, float angle)
    {
        //standard 2D rotation matrix from baseDirection by angle counterclockwise!!
        float x = baseDirection.x * Mathf.Cos(angle) - baseDirection.z * Mathf.Sin(angle);
        float y = baseDirection.x * Mathf.Sin(angle) + baseDirection.z * Mathf.Cos(angle);

        Vector3 testDir = new Vector3(x, 0, y);

        //defining start position for vectors

        Physics.SphereCast(centerPoint.position, characterWidth, testDir, out RaycastHit hit, checkDistance, obstacles);
        Debug.DrawRay(centerPoint.position, testDir * checkDistance, new Color(1, 1, angle / Mathf.PI));

        //returning succesful ray
        if (hit.collider == null)
        {
            return testDir;
        }

        //fallback
        return Vector3.zero;
    }
}
