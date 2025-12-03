using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TutorialUI : MonoBehaviour
{
    [Header("Ana Ayarlar")]
    public GameObject panel;      // full-screen TutorialPanel
    public Image pageImage;       // sayfayı gösteren Image
    public Sprite[] pages;        // 4 PNG

    [Header("Butonlar")]
    public Button nextButton;     // ileri
    public Button playButton;     // son sayfadaki PLAY
    public Button howToPlayButton; // ⭐ YENİ EKLEME: Ana menüdeki "Nasıl Oynanır" butonu
    public Button closeTutorialButton; // Tutorial panelini kapatma butonu (Eğer varsa)

    [Header("Animasyon")]
    public float scaleDuration = 0.15f;
    public float scaleFactor   = 0.9f;

    [Header("Ses")]
    public AudioSource audioSource;
    public AudioClip pageFlipSfx;

    [Header("Oyun Sahnesi")]
    public string gameSceneName = "SampleScene"; // ⭐ DİKKAT: Burayı kendi oyun sahnenizin adı ile değiştirin!

    int currentIndex = 0;
    Coroutine animCo;

    // ⭐ GÜVENLİK BAYRAĞI: Çift tıklamayı engeller.
    private bool _isProcessingClick = false; 

    void Start()
    {
        if (panel) panel.SetActive(false);

        // Next
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextPage);
        }

        // Play
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartGameFromTutorial);
        }

        // ⭐ YENİ EKLEME: HowToPlay butonuna dinleyici atama
        if (howToPlayButton != null)
        {
            howToPlayButton.onClick.RemoveAllListeners();
            howToPlayButton.onClick.AddListener(OpenTutorial);
        }
        
        // ⭐ YENİ EKLEME: Kapatma butonuna dinleyici atama
        if (closeTutorialButton != null)
        {
            closeTutorialButton.onClick.RemoveAllListeners();
            closeTutorialButton.onClick.AddListener(CloseTutorial);
        }
    }

    /// <summary>
    /// Tutorial panelini açar. Çift tetiklenmeyi ve panelin zaten açık olma durumunu engeller.
    /// </summary>
    public void OpenTutorial()
    {
        // ⭐ GÜVENLİK KONTROLÜ: Zaten bir işlem varsa veya panel açıksa, geri dön.
        if (_isProcessingClick || (panel != null && panel.activeInHierarchy)) 
        {
            return; 
        }

        if (pages == null || pages.Length == 0 || panel == null || pageImage == null)
            return;
        
        // İşlemi başlattık ve 0.2 saniye boyunca tekrar çalışmasını engelliyoruz.
        _isProcessingClick = true; 
        Invoke(nameof(ResetClick), 0.2f);

        // Ana menüdeki butonu etkisiz hale getir (çift tıklamayı fiziksel olarak önler).
        if (howToPlayButton != null)
        {
            howToPlayButton.interactable = false;
        }
        
        currentIndex = 0;
        panel.SetActive(true);
        UpdateButtons();

        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(SwapPageCo(pages[currentIndex]));
    }
    
    /// <summary>
    /// Tutorial panelini kapatır ve ana menü butonunu aktifleştirir.
    /// </summary>
    public void CloseTutorial()
    {
        if (panel) panel.SetActive(false);
        
        // Kapanınca butonu tekrar aktif et.
        if (howToPlayButton != null)
        {
            howToPlayButton.interactable = true;
        }
        
        // Bayrağı da sıfırla, böylece yeni tıklama kabul edebilir.
        _isProcessingClick = false; 
    }

    void NextPage()
    {
        if (currentIndex >= pages.Length - 1) return;

        currentIndex++;
        UpdateButtons();

        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(SwapPageCo(pages[currentIndex]));
    }

    void StartGameFromTutorial()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    void UpdateButtons()
    {
        bool last = (currentIndex >= pages.Length - 1);

        if (nextButton) nextButton.gameObject.SetActive(!last);
        if (playButton) playButton.gameObject.SetActive(last);
    }
    
    // Tıklama bayrağını sıfırlayan metot
    private void ResetClick()
    {
        _isProcessingClick = false;
    }

    IEnumerator SwapPageCo(Sprite target)
    {
        if (pageImage == null) yield break;

        RectTransform rt = pageImage.rectTransform;
        pageImage.sprite = target;

        if (audioSource && pageFlipSfx)
            audioSource.PlayOneShot(pageFlipSfx, 0.8f);

        // Animasyon: küçülerek değişme
        Vector3 small = Vector3.one * 0.85f;
        Vector3 normal = Vector3.one;

        rt.localScale = small;

        float t = 0f;
        float duration = 0.18f; 
        
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / duration;
            k = 1f - (1f - k) * (1f - k); 
            rt.localScale = Vector3.Lerp(small, normal, k);
            yield return null;
        }

        rt.localScale = normal;
    }
}