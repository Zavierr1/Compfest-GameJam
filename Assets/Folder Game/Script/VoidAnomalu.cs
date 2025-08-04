using UnityEngine;

public class VoidAnomalu : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Transform[] patrolPoints;
    private int currentPoint;
    private bool isChasing;

    void Update()
    {
        if (!isChasing)
        {
            Patrol();
        }
        else
        {
            ChasePlayer();
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPoint];
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
        }
    }

    void ChasePlayer()
    {
        // Implement chasing logic here
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LightBeam"))
        {
            isChasing = true;
        }
    }
}