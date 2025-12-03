using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // -------- SCORE --------
    [Header("Score")]
    public int score = 0;
    public int dodgePoints = 3;

    // -------- CHECKPOINT (TIME BASED) --------
    [Header("Checkpoint (Time Based)")]
    public int currentRoad = 1;

    // Bar'ın kaç saniyede dolacağını buradan ayarlarsın
    public float checkpointDuration = 45f;

    // RocketController hâlâ bunu kullanıyor diye bırakıyoruz
    [HideInInspector]
    public float lastCheckpointZ = 0f;

    // dahili zamanlayıcı
    float timer = 0f;              // saniye cinsinden süre
    float checkpointProgress = 0f; // UI için 0–1 arası

    RocketController rocket;

    void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        rocket = FindObjectOfType<RocketController>();

        // oyun başı state
        ResetCheckpoint(true);

        // ilk road yazısı
        UIManager.instance?.ShowRoadNotification(currentRoad);
    }

    void Update()
    {
        // roket yoksa, oyun başlamadıysa veya pause ise bar ilerlemesin
        if (rocket == null) return;
        if (!rocket.IsGameStarted()) return;
        if (Time.timeScale == 0f) return;

        // ---- oynanış sırasında her frame çağrılır ----
        timer += Time.deltaTime;

        // 0–1 arası oran
        checkpointProgress = Mathf.Clamp01(timer / checkpointDuration);

        // süre dolduysa bir sonraki yola geç
        if (timer >= checkpointDuration)
        {
            NextRoad();
        }
    }

    // Bir sonraki "Road"a geçiş
    void NextRoad()
    {
        currentRoad++;

        // Roketin o anki Z'sini yeni road başlangıcı gibi hatırla
        if (rocket != null)
            lastCheckpointZ = rocket.transform.position.z;

        // sadece timer'ı sıfırla, road numarası artmış durumda kalsın
        ResetCheckpoint(false);

        // ekranda "Road X" animasyonu
        UIManager.instance?.ShowRoadNotification(currentRoad);
    }

    /// <summary>
    /// checkpoint zamanlayıcısını sıfırlar.
    /// resetRoadIndex = true ise road'u da 1'e çeker.
    /// </summary>
    public void ResetCheckpoint(bool resetRoadIndex)
    {
        timer = 0f;
        checkpointProgress = 0f;

        if (resetRoadIndex)
        {
            currentRoad = 1;
            lastCheckpointZ = rocket ? rocket.transform.position.z : 0f;
        }
    }

    // -------- SCORE --------
    public int GetCorrectBallPoints()
    {
        if (score >= 1500) return 20;
        if (score >= 1300) return 18;
        if (score >= 1100) return 16;
        if (score >= 900) return 14;
        if (score >= 700) return 12;
        if (score >= 500) return 10;
        if (score >= 300) return 8;
        return 4;
    }

    public void AddCorrectBallScore()
    {
        int points = GetCorrectBallPoints();
        score += points;
        UIManager.instance?.ShowScorePopup("+" + points);
        CheckHighScore();
    }

    public void AddDodgeScore()
    {
        score += dodgePoints;
        UIManager.instance?.ShowScorePopup("+3");
        CheckHighScore();
    }

    void CheckHighScore()
    {
        int hs = PlayerPrefs.GetInt("HighScore", 0);
        if (score > hs)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
        }
    }

    // Sadece “oyun bitti” bilgisini tut, skor/checkpoint’e dokunma
    public void GameOver()
    {
        // Continue'da kullanmak için skor ve road olduğu gibi kalsın.
        // ResetCheckpoint(true) VE score = 0 BURADAN KALKTI.
    }

    // -------- FULL RESET (RESTART İÇİN) --------
    public void ResetAllProgress()
    {
        score = 0;
        ResetCheckpoint(true);
    }

    // -------- UI GETTER’LARI --------
    public float GetCheckpointProgress() => checkpointProgress;
    public int   GetCurrentRoad()       => currentRoad;
    public int   GetScore()             => score;
}
