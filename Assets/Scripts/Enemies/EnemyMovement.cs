using System.Data;
using Unity.Mathematics;
using Unity.VectorGraphics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    public float speed;
    public float acceleration;
    public Rigidbody rb;
    public Transform leftPoint;
    public Transform rightPoint;
    public Transform centerPoint;
    public float detectionDistance;
    public float avoidanceDistance;
    public float checkDistance = 3f;
    public float characterWidth;
    public Vector3 destination;
    public LayerMask obstacles;
    private Vector3 currentTarget;
    private Vector3 currentDir;
    private Vector3 position;
    public int rotationPrecision = 10;

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

        Pathfinding();
        Movement();
    }

    void Movement()
    {
        transform.rotation = Quaternion.LookRotation(currentDir);

        rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
    }

    void Pathfinding()
    {
        Vector3 realDir = (destination - position).normalized;
        Vector3 targetDir = realDir;
        float targetDist = Vector3.Distance(position, destination);

        Physics.Raycast(leftPoint.position, targetDir, out RaycastHit leftHit, targetDist, obstacles);
        Physics.Raycast(rightPoint.position, targetDir, out RaycastHit rightHit, targetDist, obstacles);
        Physics.Raycast(centerPoint.position, targetDir, out RaycastHit centerHit, targetDist, obstacles);

        if (leftHit.collider == null && rightHit.collider == null && centerHit.collider == null)
        {
            currentDir = targetDir;
            currentTarget = destination;
            Debug.DrawRay(leftPoint.position, targetDir * targetDist, Color.green);
            Debug.DrawRay(rightPoint.position, targetDir * targetDist, Color.green);
            Debug.DrawLine(position, currentTarget, Color.blue);
            return;
        }

        leftHit.distance = leftHit.collider? leftHit.distance : float.MaxValue;
        rightHit.distance = rightHit.collider? rightHit.distance : float.MaxValue;
        centerHit.distance = centerHit.collider? centerHit.distance : float.MaxValue;

        targetDist = Mathf.Min(leftHit.distance, rightHit.distance, centerHit.distance) - detectionDistance;

        if (targetDist > avoidanceDistance)
        {
            currentDir = targetDir;
            currentTarget = position + targetDir * targetDist;
            Debug.DrawRay(leftPoint.position, targetDir * targetDist, Color.yellow);
            Debug.DrawRay(rightPoint.position, targetDir * targetDist, Color.yellow);
            Debug.DrawLine(position, currentTarget, Color.blue);
            return;
        }

        //actual avoidance logic here
        //operating on a two dimensional plane where base dir is the original angle

        Vector3 baseDirection = currentDir;
        Vector3 bestDirection = Vector3.zero;

        if (currentDir == null)
            currentDir = targetDir;

        //angle represents a angle in degrees radians that increases each iteration
        for (float angle = 0; angle <= Mathf.PI; angle += Mathf.PI / rotationPrecision)
        {
            Vector3 leftCheck = CheckDirection(baseDirection, angle);
            Vector3 rightCheck = CheckDirection(baseDirection, -angle);

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

        Vector3 testDir = new Vector3(x, 0, y).normalized;

        //defining start position for vectors

        Physics.Raycast(leftPoint.position, testDir, out RaycastHit leftHit, checkDistance, obstacles);
        Physics.Raycast(rightPoint.position, testDir, out  RaycastHit rightHit, checkDistance, obstacles);
        Physics.Raycast(centerPoint.position, testDir, out  RaycastHit centerHit, checkDistance, obstacles);
            
        Debug.DrawRay(leftPoint.position, testDir * checkDistance, new Color(angle / Mathf.PI, 1, 1));
        Debug.DrawRay(rightPoint.position, testDir * checkDistance, new Color(1, angle / Mathf.PI, 1));
        Debug.DrawRay(centerPoint.position, testDir * checkDistance, new Color(1, 1, angle / Mathf.PI));

        if (leftHit.collider == null && rightHit.collider == null && centerHit.collider == null)
        {
            return testDir;
        }

        return Vector3.zero;
    }
}
