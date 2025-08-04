using UnityEngine;

public class LightSource : MonoBehaviour
{
    public string purposeFragment = "Cahaya"; // Potongan tujuan

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Kirim info ke GameManager
            GameManager.Instance.CollectLight(purposeFragment);
            
            // Efek visual
            GetComponent<Light>().enabled = false;
            Destroy(gameObject, 0.5f);
        }
    }
}
