using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public Image soundIcon;
    public Sprite soundOnIcon;
    public Sprite soundOffIcon;
    
    private bool isSoundOn = true;

    void Start()
    {
        // Kayıtlı ses durumunu yükle
        isSoundOn = PlayerPrefs.GetInt("SoundState", 1) == 1;
        UpdateSound();
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        UpdateSound();
        
        // Ses durumunu kaydet
        PlayerPrefs.SetInt("SoundState", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void UpdateSound()
    {
        // Ses seviyesini ayarla
        AudioListener.volume = isSoundOn ? 1 : 0;
        
        // Icon'u değiştir
        soundIcon.sprite = isSoundOn ? soundOnIcon : soundOffIcon;
    }
}