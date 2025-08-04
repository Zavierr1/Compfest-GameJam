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
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Chase the player
            Vector3 direction = (player.transform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * 1.5f * Time.deltaTime; // 1.5x speed when chasing
            
            // Look at the player
            transform.LookAt(player.transform);
        }
        else
        {
            Debug.LogWarning("VoidAnomalu: Player with 'Player' tag not found!");
            isChasing = false; // Stop chasing if no player found
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LightBeam"))
        {
            isChasing = true;
        }
    }
}