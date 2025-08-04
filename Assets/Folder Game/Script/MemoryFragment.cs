using UnityEngine;

public class MemoriFragment : MonoBehaviour
{
    public string namaMemori = "Purpose Fragment";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Use the singleton instance instead of FindFirstObjectByType (more efficient)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.KumpulkanMemori(namaMemori);
            }
            else
            {
                Debug.LogError("GameManager.Instance not found!");
            }
            
            // Hancurkan fragment
            Destroy(gameObject);
        }
    }
}