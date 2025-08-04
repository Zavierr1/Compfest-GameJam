// Buat script "GameManager.cs":
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public TextMeshProUGUI purposeText; // UI untuk menampilkan tujuan
    public List<string> collectedFragments = new List<string>();
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CollectLight(string fragment)
    {
        collectedFragments.Add(fragment);
        UpdatePurposeText();
        CheckWinCondition();
    }

    void UpdatePurposeText()
    {
        purposeText.text = "TUJUAN: " + string.Join(" ", collectedFragments);
    }

    void CheckWinCondition()
    {
        if (collectedFragments.Count >= 5) // Ganti dengan jumlah target
        {
            purposeText.text += "\nTUJUAN DITEMUKAN!";
            // Tambahkan efek kemenangan
        }
    }
}