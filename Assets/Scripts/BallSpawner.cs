using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [Header("Refs")]
    public GameObject ballPrefab;
    public Transform player;

    [Header("Timing")]
    public float spawnDistance = 50f;
    public float spawnInterval = 1.2f;    // başlangıç, runtime'da dinamik değişecek

    [Header("Lanes & Height")]
    public float leftPosition = -1.5f, centerPosition = 0f, rightPosition = 1.5f;
    public float ballHeight = 1f;

    [Header("Gaps")]
    public float scenarioGap = 45f;
    public float rowSpacing = 8f;
    public float doubleRowSpacing = 18f;
    public float minRowGap = 6f;

    [Header("Ramp Safety Zone")]
    public float rampInterval = 300f;
    public float firstRampZ = 200f;
    public float rampSafeZone = 30f;      // minimum güvenli mesafe (hız düşükken)

    [Header("Materials (0=Red,1=Blue,2=Yellow)")]
    public Material redMaterial, blueMaterial, yellowMaterial;

    [Header("Dynamic Difficulty")]
    public float minSpawnInterval = 0.6f; // çok hızlıyken
    public float maxSpawnInterval = 1.4f; // yavaşken
    public float maxRampSafeZone = 60f;   // hız yüksekken ramp etrafı daha geniş boşluk

    [Header("Debug")]
    public bool debugLogs = false;

    RocketController rocket;
    RampSpawner rampSpawner;
    GameManager gm;

    float nextSpawnTime;
    float lastRowZ;
    int lastScenario = -1;  // aynı senaryo üst üste gelmesin

    void Start()
    {
        if (!player)
        {
            LogErr("Player boş.");
            enabled = false;
            return;
        }

        rocket = player.GetComponent<RocketController>();
        if (!rocket)
        {
            LogErr("RocketController yok.");
            enabled = false;
            return;
        }

        if (!ballPrefab)
        {
            LogErr("ballPrefab atanmadı.");
            enabled = false;
            return;
        }

        rampSpawner = FindObjectOfType<RampSpawner>();
        gm = GameManager.instance;

        lastRowZ = player.position.z;
        nextSpawnTime = Time.time + 1.0f;

        Log($"Spawner START. scenarioGap={scenarioGap}, rowSpacing={rowSpacing}, doubleRowSpacing={doubleRowSpacing}");
    }

    void Update()
    {
        if (rocket == null) return;

        if (!rocket.IsGameStarted())
        {
            if (Time.frameCount % 120 == 0)
                Log("Game not started. Spawn beklemede.");
            return;
        }

        // --- 1) Hıza göre spawnInterval ayarla ---
        float speed = rocket.GetCurrentSpeed();
        float speedRatio = Mathf.Clamp01(speed / rocket.maxSpeed); // 0–1

        // yavaşken maxSpawnInterval, hızlıyken minSpawnInterval
        spawnInterval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, speedRatio);

        if (Time.time < nextSpawnTime) return;

        float want = player.position.z + spawnDistance;
        float baseZ = Mathf.Max(want, lastRowZ + scenarioGap);

        // --- 2) Skora göre senaryo seç ---
        int s = ChooseScenarioByScore();
        int color = rocket.GetCurrentColor();

        Log($"SPAWN -> color={color} scenario={s} baseZ={baseZ}, speed={speed:F1}");

        switch (color)
        {
            case 2: Spawn_Yellow(s, baseZ); break;
            case 0: Spawn_Red(s, baseZ);    break;
            case 1: Spawn_Blue(s, baseZ);   break;
            default: Spawn_Yellow(s, baseZ); break;
        }

        lastRowZ = baseZ;
        nextSpawnTime = Time.time + spawnInterval;
    }

    // Skora göre senaryo seçimi
    int ChooseScenarioByScore()
    {
        int score = gm ? gm.GetScore() : 0;
        int s;

        if (score < 200)
        {
            // Kolay: çoğunlukla tek sıra, ara ara gap
            float r = Random.value;
            if (r < 0.7f) s = 1;
            else s = 3;
        }
        else if (score < 600)
        {
            // Orta: karışık
            float r = Random.value;
            if (r < 0.4f) s = 1;
            else if (r < 0.8f) s = 2;
            else s = 3;
        }
        else
        {
            // Zor: çoğunlukla iki sıra + gap
            float r = Random.value;
            if (r < 0.2f) s = 1;
            else if (r < 0.6f) s = 2;
            else s = 3;
        }

        // Aynı senaryo üst üste gelmesin
        if (s == lastScenario)
            s = (s % 3) + 1;

        lastScenario = s;
        return s;
    }

    // ========================
    //   RENK SENARYOLARI
    // ========================

    void Spawn_Yellow(int s, float z)
    {
        if (s == 1)
            ThreeDiff_OneRow(z, 2, 0, 1);
        else if (s == 2)
            TwoRows_SameOrder(z, 2, 0, 1);
        else
        {
            // GAP senaryoları: roket sarı iken yanlış renkler = kırmızı+mavi
            int gapType = Random.Range(0, 3);
            if (gapType == 0)      TwoWrong_MiddleEmpty(z, 0, 1, 1);
            else if (gapType == 1) Gap_LeftEmpty(z, 0, 1, 1);
            else                   Gap_RightEmpty(z, 0, 1, 1);
        }
    }

    void Spawn_Red(int s, float z)
    {
        if (s == 1)
            ThreeDiff_OneRow(z, 0, 2, 1);
        else if (s == 2)
            TwoRows_SameOrder(z, 0, 2, 1);
        else
        {
            // Roket kırmızı iken yanlış renkler = sarı+mavi
            int gapType = Random.Range(0, 3);
            if (gapType == 0)      TwoWrong_MiddleEmpty(z, 2, 1, 1);
            else if (gapType == 1) Gap_LeftEmpty(z, 2, 1, 1);
            else                   Gap_RightEmpty(z, 2, 1, 1);
        }
    }

    void Spawn_Blue(int s, float z)
    {
        if (s == 1)
            ThreeDiff_OneRow(z, 1, 0, 2);
        else if (s == 2)
            TwoRows_SameOrder(z, 1, 0, 2);
        else
        {
            // Roket mavi iken yanlış renkler = kırmızı+sarı
            int gapType = Random.Range(0, 3);
            if (gapType == 0)      TwoWrong_MiddleEmpty(z, 0, 2, 1);
            else if (gapType == 1) Gap_LeftEmpty(z, 0, 2, 1);
            else                   Gap_RightEmpty(z, 0, 2, 1);
        }
    }

    // ========================
    //   RAMP GÜVENLİK
    // ========================

    bool IsSafeToSpawn(float z)
    {
        if (rampSpawner == null) return true;

        var ramps = rampSpawner.GetActiveRamps();
        if (ramps == null) return true;

        float speed = rocket.GetCurrentSpeed();
        float t = Mathf.Clamp01(speed / rocket.maxSpeed);
        float safe = Mathf.Lerp(rampSafeZone, maxRampSafeZone, t); // 30 → 60

        foreach (var ramp in ramps)
        {
            if (ramp != null)
            {
                float rampZ = ramp.transform.position.z;
                if (Mathf.Abs(z - rampZ) < safe)
                    return false;
            }
        }
        return true;
    }

    // Roket ile satır arasına rampa düşüyor mu?
    bool HasRampBetweenPlayerAndRow(float rowZ)
    {
        if (rampSpawner == null || player == null) return false;

        var ramps = rampSpawner.GetActiveRamps();
        if (ramps == null) return false;

        float playerZ = player.position.z;

        foreach (var ramp in ramps)
        {
            if (ramp == null) continue;
            float rz = ramp.transform.position.z;

            // ramp roketten ileride ve satırdan gerideyse arada demektir
            if (rz > playerZ && rz < rowZ)
                return true;
        }
        return false;
    }

    // ========================
    //   SATIR SENARYOLARI
    // ========================

    void ThreeDiff_OneRow(float z, int c0, int c1, int c2)
    {
        int[] cols = { c0, c1, c2 };
        Shuffle(cols);

        float z0 = RowZ(z, 0);
        if (!IsSafeToSpawn(z0)) return;

        Log($"Row @ {z0} -> [{cols[0]},{cols[1]},{cols[2]}]");
        SpawnRow(z0, cols[0], cols[1], cols[2]);
    }

    void TwoRows_SameOrder(float z, int c0, int c1, int c2)
    {
        int[] cols = { c0, c1, c2 };
        Shuffle(cols);

        float z0 = RowZ(z, 0);
        float z1 = RowZWide(z, 1);

        if (!IsSafeToSpawn(z0) || !IsSafeToSpawn(z1)) return;

        Log($"Row @ {z0} -> [{cols[0]},{cols[1]},{cols[2]}]");
        Log($"Row @ {z1} -> SAME ORDER");

        SpawnRow(z0, cols[0], cols[1], cols[2]);
        SpawnRow(z1, cols[0], cols[1], cols[2]);
    }

    // === ORTA boş (eski) ===
    void TwoWrong_MiddleEmpty(float z, int wA, int wB, int rows)
    {
        bool swap = Random.value > 0.5f;

        for (int i = 0; i < rows; i++)
        {
            float zi = (i == 0) ? RowZ(z, 0) : RowZWide(z, i);

            // ramp safe + ramp arada mı?
            if (!IsSafeToSpawn(zi)) continue;
            if (HasRampBetweenPlayerAndRow(zi)) continue;

            if (!swap)
            {
                SpawnBall(leftPosition,  zi, wA);
                SpawnBall(rightPosition, zi, wB);
            }
            else
            {
                SpawnBall(leftPosition,  zi, wB);
                SpawnBall(rightPosition, zi, wA);
            }

            SpawnGapTrigger(zi); // center lane
            Log($"Gap row CENTER @ {zi}");
        }
    }

    // === SOL boş ===
    void Gap_LeftEmpty(float z, int cMid, int cRight, int rows)
    {
        for (int i = 0; i < rows; i++)
        {
            float zi = (i == 0) ? RowZ(z, 0) : RowZWide(z, i);

            if (!IsSafeToSpawn(zi)) continue;
            if (HasRampBetweenPlayerAndRow(zi)) continue;

            SpawnBall(centerPosition, zi, cMid);
            SpawnBall(rightPosition,  zi, cRight);

            SpawnGapTrigger(zi, leftPosition);
            Log($"Gap row LEFT @ {zi}");
        }
    }

    // === SAĞ boş ===
    void Gap_RightEmpty(float z, int cLeft, int cMid, int rows)
    {
        for (int i = 0; i < rows; i++)
        {
            float zi = (i == 0) ? RowZ(z, 0) : RowZWide(z, i);

            if (!IsSafeToSpawn(zi)) continue;
            if (HasRampBetweenPlayerAndRow(zi)) continue;

            SpawnBall(leftPosition,   zi, cLeft);
            SpawnBall(centerPosition, zi, cMid);

            SpawnGapTrigger(zi, rightPosition);
            Log($"Gap row RIGHT @ {zi}");
        }
    }

    // ========================
    //   GAP TRIGGER
    // ========================

    void SpawnGapTrigger(float zPos)
    {
        SpawnGapTrigger(zPos, centerPosition);
    }

    void SpawnGapTrigger(float zPos, float xPos)
    {
        GameObject trigger = new GameObject("GapTrigger");
        trigger.transform.position = new Vector3(xPos, ballHeight, zPos);
        trigger.transform.parent = transform;
        trigger.tag = "GapTrigger";

        BoxCollider col = trigger.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1.5f, 2f, 2f);

        Destroy(trigger, 10f);
    }

    // ========================
    //   Z HESAPLARI
    // ========================

    float RowZ(float baseZ, int i)
    {
        float cand = baseZ + i * rowSpacing;
        float zz = Mathf.Max(cand, lastRowZ + minRowGap);
        lastRowZ = zz;
        return zz;
    }

    float RowZWide(float baseZ, int i)
    {
        float cand = baseZ + i * doubleRowSpacing;
        float zz = Mathf.Max(cand, lastRowZ + minRowGap);
        lastRowZ = zz;
        return zz;
    }

    // ========================
    //   TOP SPAWN
    // ========================

    void SpawnRow(float z, int leftCol, int midCol, int rightCol)
    {
        SpawnBall(leftPosition,   z, leftCol);
        SpawnBall(centerPosition, z, midCol);
        SpawnBall(rightPosition,  z, rightCol);
    }

    void SpawnBall(float xPos, float zPos, int colorIndex)
    {
        if (!ballPrefab)
        {
            LogErr("ballPrefab yok, SpawnBall iptal.");
            return;
        }

        var go = Instantiate(ballPrefab,
                             new Vector3(xPos, ballHeight, zPos),
                             Quaternion.identity,
                             transform);

        var r = go.GetComponentInChildren<Renderer>(true);
        if (!r)
        {
            LogErr("Prefabta Renderer yok. Top destroy.");
            Destroy(go);
            return;
        }

        switch (colorIndex)
        {
            case 0: r.material = redMaterial;    go.tag = "RedBall";    break;
            case 1: r.material = blueMaterial;   go.tag = "BlueBall";   break;
            case 2: r.material = yellowMaterial; go.tag = "YellowBall"; break;
            default: r.material = redMaterial;   go.tag = "RedBall";    break;
        }

        var auto = go.AddComponent<BallAutoDestroy>();
        auto.player = player;
        auto.maxBehindDistance = 40f;
    }

    // ========================
    //   YARDIMCI
    // ========================

    void Shuffle(int[] a)
    {
        for (int i = 0; i < a.Length; i++)
        {
            int j = Random.Range(i, a.Length);
            (a[i], a[j]) = (a[j], a[i]);
        }
    }

    void Log(string msg)
    {
        if (debugLogs)
            Debug.Log($"[BallSpawner] {msg}");
    }

    void LogErr(string msg)
    {
        Debug.LogError($"[BallSpawner] {msg}");
    }

    public void ResetSpawner()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        lastRowZ = player ? player.position.z : 0f;
        nextSpawnTime = Time.time + 1.0f;
    }
}
