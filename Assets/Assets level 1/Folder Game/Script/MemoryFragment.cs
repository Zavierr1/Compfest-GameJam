using UnityEngine;

public enum FragmentType { Purpose, Vision, Sound, Time }

public class MemoryFragment : MonoBehaviour
{
    public FragmentType fragmentType = FragmentType.Purpose;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Call the GameManager to log the collection
            if (GameManager.Instance != null)
            {
                GameManager.Instance.KumpulkanMemori(fragmentType);
            }
            else
            {
                Debug.LogError("GameManager.Instance not found!");
            }

            // Use the MemoryFragmentEffects singleton to apply the actual effect
            if (MemoryFragmentEffects.Instance != null)
            {
                // Play collection particles at the fragment's position
                MemoryFragmentEffects.Instance.PlayCollectionParticles(transform.position);

                switch (fragmentType)
                {
                    case FragmentType.Vision:
                        MemoryFragmentEffects.Instance.ApplyVisionEffect();
                        break;
                    case FragmentType.Sound:
                        MemoryFragmentEffects.Instance.ApplySoundEffect();
                        break;
                    case FragmentType.Time:
                        MemoryFragmentEffects.Instance.ApplyTimeEffect();
                        break;
                }
            }
            else
            {
                Debug.LogError("MemoryFragmentEffects.Instance not found!");
            }
            
            // Destroy the fragment after collection
            Destroy(gameObject);
        }
    }


    void ApplySoundEffect()
    {
        // Code for sound effects
        Debug.Log("Sound effect applied!");
    }

    void ApplyTimeEffect()
    {
        // Code to manipulate time
        Debug.Log("Time effect applied!");
    }
}
