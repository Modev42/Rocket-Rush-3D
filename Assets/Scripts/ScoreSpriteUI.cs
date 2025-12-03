using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreSpriteUI : MonoBehaviour
{
    [Header("Assets")]
    public Sprite[] digitSprites;          // 0..9
    public GameObject digitPrefab;         // Image içeren prefab

    [Header("Layout")]
    public float digitHeight = 60f;        // px
    public float spacing = 6f;             // temel boşluk
    public bool alignRight = true;         // sağ-üst hizala

    [Header("Kerning (genel)")]
    public float kerningForOne = 4f;       // '1' yanını biraz aç
    public float globalTighten = -2f;      // tüm aralıklara ek (negatif = sıkı)
    public float extraTightAll = -2f;      // tüm aralıklara ek

    [Header("Sadece 1. ve 2. basamak arası")]
    public float firstPairExtraWhenStartsWith1 = 6f; // “1x…” başlıyorsa ilk aralığı bu kadar AÇ

    [System.Serializable] public struct PairK { public string pair; public float k; }
    public PairK[] pairKerns = new PairK[] {
        // normal çift ayarları (isteğe bağlı)
        new PairK{pair="11", k=+6f},
        new PairK{pair="12", k=+4f}, new PairK{pair="13", k=+3f}, new PairK{pair="14", k=+4f},
        new PairK{pair="15", k=+4f}, new PairK{pair="16", k=+4f}, new PairK{pair="17", k=+3f},
        new PairK{pair="18", k=+4f}, new PairK{pair="19", k=+3f},
        new PairK{pair="36", k=-2f}, new PairK{pair="63", k=-2f},
    };

    [Header("Fine tune")]
    public float uniformWidth = -1f;               // >0 ise tüm rakamlar aynı genişlik
    public float[] perDigitScale = new float[10];  // 0 => 1
    public float[] perDigitOffsetX = new float[10];// px (+sağa)
    void Reset(){ perDigitOffsetX = new float[10]; perDigitOffsetX[1] = +1.5f; }

    [Header("Offset")]
    public Vector2 nudge = Vector2.zero;   // tüm bloğu kaydır

    [Header("Pixel snap")]
    public bool snapToPixels = true;
    public float referencePPU = 100f;

    int lastScore = -1;
    readonly List<Image> pool = new();
    RectTransform rect;

    void Awake() { rect = GetComponent<RectTransform>(); }

    void Update()
    {
        int score = GameManager.instance ? GameManager.instance.GetScore() : 0;
        if (score != lastScore) { RenderScore(score); lastScore = score; }
    }

    void RenderScore(int value)
    {
        string s = Mathf.Max(0, value).ToString();
        EnsurePoolSize(s.Length);
        for (int i = 0; i < pool.Count; i++) pool[i].gameObject.SetActive(false);

        // 1) genişlikler
        float[] widths = new float[s.Length];
        for (int i = 0; i < s.Length; i++)
        {
            int d = s[i] - '0';
            var img = pool[i];
            img.sprite = (digitSprites != null && d >= 0 && d <= 9) ? digitSprites[d] : null;
            img.preserveAspect = true;
            img.raycastTarget = false;

            float aspect = 1f;
            if (img.sprite && img.sprite.rect.height > 0)
                aspect = img.sprite.rect.width / img.sprite.rect.height;

            float scale = (perDigitScale[d] == 0f) ? 1f : perDigitScale[d];
            float w = digitHeight * aspect * scale;
            if (uniformWidth > 0f) w = uniformWidth;

            widths[i] = w;

            var rt = img.rectTransform;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   digitHeight);
            img.gameObject.SetActive(true);
        }

        // 2) gap’ler (sadece ilk aralığı hedefleme dahil)
        float[] gaps = new float[Mathf.Max(0, s.Length - 1)];
        for (int i = 0; i < gaps.Length; i++)
        {
            char a = s[i];
            char b = s[i + 1];

            float g = spacing + globalTighten + extraTightAll;
            if (a == '1' || b == '1') g += kerningForOne;
            g += GetPairKerning(a, b);

            // SADECE 1. ve 2. basamak arası genişletme
            if (i == 0 && a == '1') g += firstPairExtraWhenStartsWith1;

            gaps[i] = g;
        }

        // 3) toplam genişlik
        float totalW = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            totalW += widths[i];
            if (i < gaps.Length) totalW += gaps[i];
        }

        float startX = alignRight ? -totalW : -totalW * 0.5f;

        // 4) yerleştir
        float x = startX;
        for (int i = 0; i < s.Length; i++)
        {
            int d = s[i] - '0';
            var rt = pool[i].rectTransform;

            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 1f);

            float localOffset = perDigitOffsetX[d];
            Vector2 pos = new Vector2(x + localOffset + nudge.x, nudge.y);
            if (snapToPixels) pos = new Vector2(Snap(pos.x), Snap(pos.y));
            rt.anchoredPosition = pos;

            x += widths[i];
            if (i < gaps.Length) x += gaps[i];
        }
    }

    float GetPairKerning(char a, char b)
    {
        foreach (var pk in pairKerns)
            if (pk.pair.Length == 2 && pk.pair[0] == a && pk.pair[1] == b) return pk.k;
        return 0f;
    }

    float Snap(float v) => Mathf.Round(v * referencePPU) / referencePPU;

    void EnsurePoolSize(int n)
    {
        while (pool.Count < n)
        {
            var go = Instantiate(digitPrefab, transform);
            var img = go.GetComponent<Image>();
            pool.Add(img);
        }
    }
}
