using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI")]
    public Text highScoreText;
    
    void Start()
    {
        // High Score'u yükle ve göster
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        if (highScoreText != null)
        {
            highScoreText.text = " " + highScore;
        }
    }
    
    public void PlayGame()
    {
        // GameScene'i yükle
        SceneManager.LoadScene("SampleScene");
    }
}