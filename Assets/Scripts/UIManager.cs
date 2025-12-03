using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    // --- singleton & coroutines ---
    public static UIManager instance;
    Coroutine roadNotificationCoroutine;
    Coroutine scorePopupCoroutine;
    Coroutine countdownCoroutine;
    Coroutine pauseBlinkCoroutine;

    // --- UI refs ---
    [Header("UI Refs")]
    public Text scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI countdownText;

    public Slider checkpointBar;
    public TextMeshProUGUI leftRoadIcon;
    public TextMeshProUGUI rightRoadIcon;
    public TextMeshProUGUI roadNotificationText;

    [Header("Sounds (optional)")]
    public AudioSource audioSource;
    public AudioClip tickSfx;
    public AudioClip goSfx;
    public AudioClip roadNotificationSfx;

    [Header("Score Popup")]
    public TextMeshProUGUI scorePopupText;

    [Header("Pause Button")]
    public Button pauseButton;        // sol üstteki pause butonu
    public Image pauseIcon;           // butonun içindeki image
    public Sprite iconPause;          // "||"
    public Sprite iconPlay;           // "▶"
    public TextMeshProUGUI pauseText; // ortadaki "PAUSED" yazısı
    bool isPaused = false;

    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;     // Continue / Restart / Home ikonlarının paneli
    public Button pauseContinueButton;    // play ikonu
    public Button pauseRestartButton;     // dönen ok ikonu
    public Button pauseMainMenuButton;    // ev ikonu

    void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (countdownText) countdownText.gameObject.SetActive(false);
        if (roadNotificationText) roadNotificationText.gameObject.SetActive(false);

        // CHECKPOINT BAR ayarları (0–1 arası)
        if (checkpointBar)
        {
            checkpointBar.minValue = 0f;
            checkpointBar.maxValue = 1f;
            checkpointBar.value    = 0f;
        }

        // PAUSE button (sol üst)
        if (pauseButton) pauseButton.onClick.AddListener(OnPauseToggle);
        if (pauseIcon && iconPause) pauseIcon.sprite = iconPause;
        if (pauseText) pauseText.gameObject.SetActive(false);

        // PAUSE MENU panel & butonlar
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);

        // Eğer inspector'da atanmamışsa, panel altından otomatik bul
        if (pauseMenuPanel)
        {
            var buttons = pauseMenuPanel.GetComponentsInChildren<Button>(true);

            if (!pauseContinueButton)
            {
                foreach (var b in buttons)
                    if (b.name.ToLower().Contains("continue") || b.name.ToLower().Contains("play"))
                    { pauseContinueButton = b; break; }
            }

            if (!pauseRestartButton)
            {
                foreach (var b in buttons)
                    if (b.name.ToLower().Contains("restart") || b.name.ToLower().Contains("retry"))
                    { pauseRestartButton = b; break; }
            }

            if (!pauseMainMenuButton)
            {
                foreach (var b in buttons)
                    if (b.name.ToLower().Contains("home") || b.name.ToLower().Contains("menu"))
                    { pauseMainMenuButton = b; break; }
            }
        }

        // Listener'lar
        if (pauseContinueButton)
        {
            pauseContinueButton.onClick.RemoveAllListeners();
            pauseContinueButton.onClick.AddListener(OnPauseToggle);   // devam et
        }

        if (pauseRestartButton)
        {
            pauseRestartButton.onClick.RemoveAllListeners();
            pauseRestartButton.onClick.AddListener(OnRestartButton);  // yeniden başlat
        }

        if (pauseMainMenuButton)
        {
            pauseMainMenuButton.onClick.RemoveAllListeners();
            pauseMainMenuButton.onClick.AddListener(OnMainMenuButton); // ana menü
        }
    }

    void Update()
    {
        var gm = GameManager.instance;
        if (gm != null)
        {
            if (scoreText) scoreText.text = gm.GetScore().ToString();
            if (checkpointBar) checkpointBar.value = gm.GetCheckpointProgress();

            int currentRoad = gm.GetCurrentRoad();
            if (leftRoadIcon) leftRoadIcon.text = currentRoad.ToString();
            if (rightRoadIcon) rightRoadIcon.text = (currentRoad + 1).ToString();
        }

        // Klavyeden ESC ile pause (editor testi için)
        if (Input.GetKeyDown(KeyCode.Escape)) OnPauseToggle();
    }

    // --- Road notification ---
    public void ShowRoadNotification(int roadNumber)
    {
        if (!roadNotificationText) return;

        roadNotificationText.text = "Road " + roadNumber;
        if (roadNotificationCoroutine != null) StopCoroutine(roadNotificationCoroutine);
        roadNotificationCoroutine = StartCoroutine(RoadNotificationAnimation());

        if (audioSource && roadNotificationSfx) audioSource.PlayOneShot(roadNotificationSfx, 0.8f);
    }

    IEnumerator RoadNotificationAnimation()
    {
        roadNotificationText.gameObject.SetActive(true);
        RectTransform rect = roadNotificationText.GetComponent<RectTransform>();

        Vector2 startPos = new Vector2(-800, 300);
        Vector2 centerPos = Vector2.zero;

        rect.anchoredPosition = startPos;
        roadNotificationText.transform.localScale = Vector3.zero;
        roadNotificationText.alpha = 0f;

        float duration = 0.6f, elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            rect.anchoredPosition = Vector2.Lerp(startPos, centerPos, easeT);
            roadNotificationText.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.2f, easeT);
            roadNotificationText.alpha = t;
            yield return null;
        }

        rect.anchoredPosition = centerPos;

        // küçük bounce
        Vector3 orig = roadNotificationText.transform.localScale;
        Vector3 big = orig * 1.15f;
        float bounce = 0.1f;
        elapsed = 0f;
        while (elapsed < bounce)
        {
            elapsed += Time.unscaledDeltaTime;
            roadNotificationText.transform.localScale = Vector3.Lerp(orig, big, elapsed / bounce);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < bounce)
        {
            elapsed += Time.unscaledDeltaTime;
            roadNotificationText.transform.localScale = Vector3.Lerp(big, orig, elapsed / bounce);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1.5f);

        // çıkış
        Vector2 endPos = new Vector2(0, 600);
        Vector3 normal = roadNotificationText.transform.localScale;
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float easeT = t * t * t;
            rect.anchoredPosition = Vector2.Lerp(centerPos, endPos, easeT);
            roadNotificationText.transform.localScale = Vector3.Lerp(normal, Vector3.zero, easeT);
            roadNotificationText.alpha = 1f - t;
            yield return null;
        }
        roadNotificationText.gameObject.SetActive(false);
    }

    // --- Game Over / Buttons ---
    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        // game over'da unpause
        isPaused = false;
        ApplyPause();
    }

    public void OnContinueButton()
    {
        HideGameOver();
        var rocket = FindObjectOfType<RocketController>();
        if (rocket) rocket.ContinueGame();
    }

    public void OnRestartButton()
    {
        Debug.Log("Pause Restart CLICKED");

        HideGameOver();

        // garanti unpause
        isPaused = false;
        ApplyPause();

        // checkpoint zamanlayıcısını ve road'u başa al
        var gm = GameManager.instance;
        if (gm != null)
            gm.ResetAllProgress();

        var rocket = FindObjectOfType<RocketController>();
        if (rocket)
        {
            rocket.ResetToStart(); // countdown içeriyor
            var rb = rocket.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        var cam   = FindObjectOfType<CameraFollow>();  if (cam)   cam.ResetCamera();
        var road  = FindObjectOfType<RoadSpawner>();   if (road)  road.ResetSpawner();
        var balls = FindObjectOfType<BallSpawner>();   if (balls) balls.ResetSpawner();
        var ramps = FindObjectOfType<RampSpawner>();   if (ramps) ramps.ResetSpawner();
    }

    public void OnMainMenuButton()
    {
        isPaused = false;
        ApplyPause();
        SceneManager.LoadScene("MainMenu");
    }

    void HideGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    // --- Countdown ---
    public void StartCountdown()
    {
        if (!countdownText) return;
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    IEnumerator CountdownCoroutine()
    {
        countdownText.gameObject.SetActive(true);
        yield return PlayCountAndWait("3");
        yield return PlayCountAndWait("2");
        yield return PlayCountAndWait("1");
        countdownText.gameObject.SetActive(false);
    }

    IEnumerator PlayCountAndWait(string text)
    {
        countdownText.text = text;
        if (audioSource && tickSfx) audioSource.PlayOneShot(tickSfx);
        yield return new WaitForSecondsRealtime(1f); // pause sırasında da sayabilsin
    }

    // --- Score popup ---
    public void ShowScorePopup(string text)
    {
        if (!scorePopupText) return;

        var rocket = FindObjectOfType<RocketController>();
        if (rocket)
        {
            Color popupColor = Color.yellow;
            switch (rocket.GetCurrentColor())
            {
                case 0: popupColor = Color.red;    break;
                case 1: popupColor = Color.cyan;   break;
                case 2: popupColor = Color.yellow; break;
            }
            scorePopupText.color = popupColor;
        }

        if (scorePopupCoroutine != null) StopCoroutine(scorePopupCoroutine);
        scorePopupCoroutine = StartCoroutine(ScorePopupAnimation(text));
    }

    IEnumerator ScorePopupAnimation(string text)
    {
        scorePopupText.text = text;
        scorePopupText.gameObject.SetActive(true);

        scorePopupText.alpha = 1f;
        scorePopupText.transform.localScale = Vector3.one * 0.5f;

        float duration = 1f, elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            scorePopupText.transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * 1.5f, t);
            scorePopupText.alpha = 1f - t;
            yield return null;
        }

        scorePopupText.gameObject.SetActive(false);
    }

    // --- Pause ---
    public void OnPauseToggle()
    {
        isPaused = !isPaused;
        ApplyPause();
    }

    void ApplyPause()
    {
        Time.timeScale = isPaused ? 0f : 1f;
        AudioListener.pause = isPaused;

        if (pauseIcon) 
            pauseIcon.sprite = isPaused ? iconPlay : iconPause;

        if (pauseText)
        {
            pauseText.gameObject.SetActive(isPaused);
            if (isPaused)
            {
                if (pauseBlinkCoroutine != null) StopCoroutine(pauseBlinkCoroutine);
                pauseBlinkCoroutine = StartCoroutine(BlinkPauseText());
            }
            else
            {
                if (pauseBlinkCoroutine != null) StopCoroutine(pauseBlinkCoroutine);
                pauseText.alpha = 1f;
            }
        }

        // Pause menü paneli aç/kapa
        if (pauseMenuPanel)
            pauseMenuPanel.SetActive(isPaused);
    }

    IEnumerator BlinkPauseText()
    {
        while (isPaused)
        {
            pauseText.alpha = 1f;
            yield return new WaitForSecondsRealtime(0.75f);
            pauseText.alpha = 0f;
            yield return new WaitForSecondsRealtime(0.75f);
        }
    }
}
