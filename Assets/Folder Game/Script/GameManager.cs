using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Changed to match the MemoryFragment script
    public int totalMemori = 4; // Changed to 4 for all memory fragments
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
        Debug.Log("Selamat! Semua 4 Memori Ditemukan! Menuju Level 2...");
        
        // Add a small delay before transitioning to give player time to see the completion
        StartCoroutine(TransitionToLevel2());
    }
    
    private System.Collections.IEnumerator TransitionToLevel2()
    {
        // Wait for 2 seconds to let player see they won
        yield return new WaitForSeconds(2f);
        
        // Ensure time scale is normal before scene transition
        Time.timeScale = 1f;
        
        // Reset cursor state for scene transition
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Load Level2 scene using SceneTransitionManager if available
        if (SceneTransitionManager.Instance != null)
        {
            Debug.Log("Using SceneTransitionManager to load Level2");
            SceneTransitionManager.Instance.LoadScene("Level2");
        }
        else
        {
            Debug.Log("SceneTransitionManager not found, loading Level2 directly");
            SceneManager.LoadScene("Level2");
        }
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