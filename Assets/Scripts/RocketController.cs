using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class RocketController : MonoBehaviour
{
    [Header("Hız")]
    public float forwardSpeed = 15f;
    public float maxSpeed = 30f;
    public float speedIncreaseRate = 0.3f;   
    public float horizontalSpeed = 22f;
    public float maxHorizontalPosition = 2.5f;
    public float gravityForce = 15f;

    [Header("Dokunmatik & Klavye")]
    [Range(0.5f, 4f)] public float dragSensitivity = 2f;
    public bool enableKeyboardInEditor = true; // Editor'da klavye aç/kapat

    [Header("Materyaller")]
    public Material redMaterial, blueMaterial, yellowMaterial;

    [Header("Sesler")]
    public AudioClip collectGoodSfx, collectBadSfx, gameOverSfx, countdownTickSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    [Header("VFX")]
    public ParticleSystem thruster; 

    // internal
    CharacterController controller;
    Renderer rend;
    AudioSource audioSrc;

    float initialSpeed;
    float savedForwardSpeed;   

    float targetX = 0f;
    bool isGameStarted, isCountingDown;
    RocketColor currentColor;
    float verticalVelocity = 0f;
    Vector3 startPosition;
    Quaternion startRotation;

    // touch
    int activeFingerId = -1;
    float dragStartX;
    float rocketStartX;

    // child transform backup
    Transform[] childs;
    Vector3[] initLocalPos;
    Quaternion[] initLocalRot;
    Vector3[] initLocalScale;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        controller = GetComponent<CharacterController>();
        rend = GetComponent<Renderer>();
        audioSrc = GetComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        audioSrc.loop = false;

        initialSpeed = forwardSpeed;
        savedForwardSpeed = forwardSpeed;

        childs = GetComponentsInChildren<Transform>(true);
        initLocalPos   = new Vector3[childs.Length];
        initLocalRot   = new Quaternion[childs.Length];
        initLocalScale = new Vector3[childs.Length];
        for (int i = 0; i < childs.Length; i++)
        {
            initLocalPos[i]   = childs[i].localPosition;
            initLocalRot[i]   = childs[i].localRotation;
            initLocalScale[i] = childs[i].localScale;
        }

        ApplySavedSensitivity();
        ChangeColor(Random.Range(0, 3)); 
        UpdateThrusterBySpeed();
        SetThrusterActive(false);        

        StartCoroutine(DelayedStart());
    }

    void OnEnable() => ApplySavedSensitivity();

    void ApplySavedSensitivity()
    {
        float raw = PlayerPrefs.GetFloat("SensitivityValue", 50f);
        float t = Mathf.InverseLerp(0f, 100f, raw);
        float curved = Mathf.Pow(t, 1.4f); 
        float minSpeed = 6f;   
        float maxSpeed = 38f;  
        horizontalSpeed = Mathf.Lerp(minSpeed, maxSpeed, curved);
    }

    System.Collections.IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1f);
        if (UIManager.instance != null) StartCountdown();
        else isGameStarted = true;
    }

    void Update()
    {
        if (isCountingDown || !isGameStarted) return;

        HandleInput(); // Hem dokunmatik hem klavye burada
        MoveRocket();
        UpdateThrusterBySpeed();
    }

    // === Input (Dokunmatik + Klavye) ===
    void HandleInput()
    {
        // 1. KLAVYE DESTEĞİ (Unity Editor Kaydı ve Test için)
        #if UNITY_EDITOR || UNITY_STANDALONE
        float hInput = Input.GetAxis("Horizontal");
        if (hInput != 0)
        {
            targetX += hInput * horizontalSpeed * Time.deltaTime;
            targetX = Mathf.Clamp(targetX, -maxHorizontalPosition, maxHorizontalPosition);
            // Klavye kullanılıyorsa dokunmatik kısmına bakma (çakışmayı önler)
            return; 
        }
        #endif

        // 2. DOKUNMATİK DESTEĞİ (Mobil için)
        if (Input.touchCount == 0) { activeFingerId = -1; return; }

        int idx = -1;
        for (int i = 0; i < Input.touchCount; i++)
        {
            var tt = Input.GetTouch(i);
            if (activeFingerId == -1 || tt.fingerId == activeFingerId) { idx = i; break; }
        }
        if (idx == -1) return;

        Touch t = Input.GetTouch(idx);

        if (t.phase == TouchPhase.Began)
        {
            activeFingerId = t.fingerId;
            dragStartX = t.position.x;
            rocketStartX = transform.position.x;
        }
        else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
        {
            float deltaNorm = (t.position.x - dragStartX) / (Screen.width * 0.5f);
            float desired = rocketStartX + deltaNorm * maxHorizontalPosition * dragSensitivity;
            targetX = Mathf.Clamp(desired, -maxHorizontalPosition, maxHorizontalPosition);
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            activeFingerId = -1;
        }
    }

    void MoveRocket()
    {
        Vector3 fwd = Vector3.forward * forwardSpeed * Time.deltaTime;

        float currentX = transform.position.x;
        float newX = Mathf.Lerp(currentX, targetX, horizontalSpeed * 0.8f * Time.deltaTime); // Klavye ile daha smooth geçiş için 0.8f ekledim
        Vector3 hor = new Vector3(newX - currentX, 0, 0);

        verticalVelocity -= gravityForce * Time.deltaTime;
        Vector3 vertical = new Vector3(0, verticalVelocity * Time.deltaTime, 0);

        controller.Move(fwd + hor + vertical);

        if (transform.position.y <= 1f)
        {
            transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
            verticalVelocity = 0f;
        }
    }

    public void IncreaseSpeedByCheckpoint()
    {
        if (forwardSpeed >= maxSpeed) return;
        forwardSpeed = Mathf.Min(maxSpeed, forwardSpeed + speedIncreaseRate);
    }

    public void ChangeColor(int i)
    {
        currentColor = (RocketColor)i;
        switch (currentColor)
        {
            case RocketColor.Red:    rend.material = redMaterial;    break;
            case RocketColor.Blue:   rend.material = blueMaterial;   break;
            case RocketColor.Yellow: rend.material = yellowMaterial; break;
        }
        ApplyThrusterColor(currentColor);
    }

    public int GetCurrentColor() => (int)currentColor;
    public bool IsGameStarted() => isGameStarted;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint")) { IncreaseSpeedByCheckpoint(); return; }

        if (other.CompareTag("ColorRamp"))
        {
            string[] parts = other.name.Split('_');
            if (parts.Length > 1 && int.TryParse(parts[1], out int rampColor))
            {
                ChangeColor(rampColor);
                verticalVelocity = 6f;
            }
            return;
        }

        if (other.CompareTag("GapTrigger"))
        {
            var col = other.GetComponent<Collider>();
            if (col) col.enabled = false;
            GameManager.instance?.AddDodgeScore();
            Destroy(other.gameObject, 0.05f);
            return;
        }

        GameObject root = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.transform.root.gameObject;

        int hitColor = -1;
        if (root.CompareTag("RedBall"))      hitColor = (int)RocketColor.Red;
        else if (root.CompareTag("BlueBall"))   hitColor = (int)RocketColor.Blue;
        else if (root.CompareTag("YellowBall")) hitColor = (int)RocketColor.Yellow;
        else return;

        HitBall(hitColor, root);
    }

    void HitBall(int ballColor, GameObject root)
    {
        foreach (var c in root.GetComponentsInChildren<Collider>(true))
            c.enabled = false;

        if (ballColor == (int)currentColor)
        {
            GameManager.instance?.AddCorrectBallScore();
            PlayOneShotSafe(collectGoodSfx, sfxVolume);
            Destroy(root);
        }
        else
        {
            ClearNearbyBalls(root.transform.position, 8f);
            PlayOneShotSafe(collectBadSfx, sfxVolume);
            GameOver();
        }
    }

    public void GameOver()
    {
        savedForwardSpeed = forwardSpeed;
        isGameStarted = false;
        isCountingDown = false;
        targetX = 0f;
        forwardSpeed = 0f;
        SetThrusterActive(false);
        PlayOneShotSafe(gameOverSfx, sfxVolume);
        GameManager.instance?.GameOver();
        UIManager.instance?.ShowGameOver(0);
    }

    public void ContinueGame()
    {
        forwardSpeed = savedForwardSpeed;
        SetThrusterActive(false);
        StartCountdown();
    }

    public void RestartAtCheckpoint()
    {
        if (GameManager.instance)
        {
            float cz = GameManager.instance.lastCheckpointZ;
            transform.position = new Vector3(0, 1, cz);
        }
        RestoreChildLocals();
        SetThrusterActive(false);
        forwardSpeed = initialSpeed;
        StartCountdown();
    }

    public void ResetToStart()
    {
        controller.enabled = false;
        transform.position = startPosition;
        transform.rotation = startRotation;
        RestoreChildLocals();
        controller.enabled = true;
        verticalVelocity = 0f;
        targetX = 0f;
        forwardSpeed = initialSpeed;
        DestroyAllBalls();
        SetThrusterActive(false);
        StartCountdown();
    }

    void StartCountdown()
    {
        isGameStarted = false;
        isCountingDown = true;
        SetThrusterActive(true);
        UIManager.instance?.StartCountdown();
        StopAllCoroutines();
        StartCoroutine(CountdownCo());
    }

    System.Collections.IEnumerator CountdownCo()
    {
        for (int i = 0; i < 3; i++)
        {
            PlayOneShotSafe(countdownTickSfx, sfxVolume * 0.9f);
            yield return new WaitForSeconds(1f);
        }
        StartGameAfterCountdown();
    }

    void StartGameAfterCountdown()
    {
        isCountingDown = false;
        isGameStarted = true;
        SetThrusterActive(true);
    }

    void RestoreChildLocals()
    {
        for (int i = 1; i < childs.Length; i++)
        {
            childs[i].localPosition = initLocalPos[i];
            childs[i].localRotation = initLocalRot[i];
            childs[i].localScale    = initLocalScale[i];
        }
    }

    void DestroyAllBalls()
    {
        foreach (var go in GameObject.FindGameObjectsWithTag("RedBall"))    Destroy(go);
        foreach (var go in GameObject.FindGameObjectsWithTag("BlueBall"))   Destroy(go);
        foreach (var go in GameObject.FindGameObjectsWithTag("YellowBall")) Destroy(go);
    }

    void ClearNearbyBalls(Vector3 center, float radius)
    {
        float r2 = radius * radius;
        void ClearTag(string tag)
        {
            var objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (var go in objs)
            {
                Vector3 diff = go.transform.position - center;
                if (diff.sqrMagnitude <= r2) Destroy(go);
            }
        }
        ClearTag("RedBall");
        ClearTag("BlueBall");
        ClearTag("YellowBall");
    }

    void SetThrusterActive(bool on)
    {
        if (!thruster) return;
        var em = thruster.emission;
        em.enabled = on;
        if (on) { thruster.Clear(true); thruster.Play(true); }
        else { thruster.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
    }

    void ApplyThrusterColor(RocketColor color)
    {
        if (!thruster) return;
        Color core = color switch {
            RocketColor.Red    => new Color(1f, 0.48f, 0.35f),
            RocketColor.Blue   => new Color(0.55f, 0.95f, 1f),
            RocketColor.Yellow => new Color(1f, 0.92f, 0.45f),
            _ => Color.white
        };
        Color tip = Color.Lerp(core, Color.white, 0.2f);
        var col = thruster.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(core, 0f), new GradientColorKey(tip, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(0.6f, 0.7f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(g);
    }

    void UpdateThrusterBySpeed()
    {
        if (!thruster) return;
        float t = Mathf.InverseLerp(0f, maxSpeed, forwardSpeed);
        var main = thruster.main;
        main.startSize = Mathf.Lerp(0.35f, 0.6f, t);
        var em = thruster.emission;
        em.rateOverTime = Mathf.Lerp(40f, 90f, t);
    }

    void PlayOneShotSafe(AudioClip clip, float vol = 1f)
    {
        if (clip == null || audioSrc == null) return;
        audioSrc.PlayOneShot(clip, Mathf.Clamp01(vol));
    }

    public float GetCurrentSpeed() => forwardSpeed;
}