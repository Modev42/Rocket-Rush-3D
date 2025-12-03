using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu instance;

    [Header("Panels")]
    public GameObject privacyPanel;

    [Header("Buttons")]
    public Button btnReset;
    public Button btnOpenPrivacy;
    public Button btnClosePrivacy;
    public Button btnInstagram;
    public Button btnTwitter;
    public Button btnWebsite;

    [Header("Links")]
    public string instagramUrl = "https://instagram.com/yourpage";
    public string twitterUrl = "https://twitter.com/yourpage";
    public string websiteUrl = "https://yourwebsite.com";

    void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (privacyPanel) privacyPanel.SetActive(false);

        // Reset
        if (btnReset) btnReset.onClick.AddListener(ResetGame);

        // Gizlilik paneli
        if (btnOpenPrivacy) btnOpenPrivacy.onClick.AddListener(OpenPrivacy);
        if (btnClosePrivacy) btnClosePrivacy.onClick.AddListener(ClosePrivacy);

        // Linkler
        if (btnInstagram) btnInstagram.onClick.AddListener(() => OpenURL(instagramUrl));
        if (btnTwitter) btnTwitter.onClick.AddListener(() => OpenURL(twitterUrl));
        if (btnWebsite) btnWebsite.onClick.AddListener(() => OpenURL(websiteUrl));
    }

    // --- RESET ---
    public void ResetGame()
    {
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.DeleteKey("CurrentRoad");
        PlayerPrefs.DeleteKey("LastCheckpointZ");
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // --- GİZLİLİK PANELİ ---
    public void OpenPrivacy()
    {
        if (privacyPanel) privacyPanel.SetActive(true);
    }

    public void ClosePrivacy()
    {
        if (privacyPanel) privacyPanel.SetActive(false);
    }

    // --- LINKLER ---
    void OpenURL(string url)
    {
        Application.OpenURL(url);
    }
}
