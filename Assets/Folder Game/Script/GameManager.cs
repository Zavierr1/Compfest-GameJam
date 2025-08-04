using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Changed to match the MemoryFragment script
    public int totalMemori = 3;
    private int memoriTerkumpul;
    public TextMeshProUGUI uiText;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

public void KumpulkanMemori(FragmentType fragmentType)
    {
        memoriTerkumpul++;
        Debug.Log($"Mengumpulkan: {fragmentType}");
        UpdateUI();

        if (memoriTerkumpul >= totalMemori)
        {
            Menang();
        }
    }

    void UpdateUI()
    {
        if (uiText != null)
        {
            uiText.text = $"Memori: {memoriTerkumpul}/{totalMemori}";
        }
    }

    void Menang()
    {
        Debug.Log("Selamat! Purpose Ditemukan!");
        // Tambahkan efek menang di sini
    }
    
    void Update()
    {
        // ESC is now handled by PauseMenuManager
        // Remove direct ESC handling to avoid conflicts
    }
    
    public void ReturnToMainMenu()
    {
        // Use scene transition manager if available, otherwise load directly
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}