using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SensitivityUI : MonoBehaviour
{
    [Header("Refs")]
    public GameObject panel;          // SensitivityPanel
    public Slider slider;             // SensitivitySlider
    public TextMeshProUGUI valueText; // "50" veya "50%" gösterecek yazı

    const string PREF_KEY = "SensitivityValue"; // 0–100 arası kaydediyoruz

    void Start()
    {
        // Panel başlangıçta kapalı olsun
        if (panel) 
            panel.SetActive(false);

        // Kayıtlı değeri oku, yoksa 50 olsun
        float saved = PlayerPrefs.GetFloat(PREF_KEY, 50f);

        if (slider)
        {
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.wholeNumbers = true; // istersen tam sayı
            slider.value = saved;
            slider.onValueChanged.AddListener(OnSliderChanged);
        }

        UpdateValueText(saved);
    }

    // Ana menüde hassasiyet ikonuna bastığında
    public void OpenPanel()
    {
        if (panel) 
            panel.SetActive(true);
    }

    // Panel içindeki X / Close butonu
    public void ClosePanel()
    {
        if (panel) 
            panel.SetActive(false);
    }

    // Slider her değiştiğinde
    void OnSliderChanged(float v)
    {
        PlayerPrefs.SetFloat(PREF_KEY, v);
        PlayerPrefs.Save();
        UpdateValueText(v);
    }

    // Yazıyı güncelle
    void UpdateValueText(float v)
    {
        if (!valueText) return;

        int iv = Mathf.RoundToInt(v);

        // Sadece sayı:
        // valueText.text = iv.ToString();

        // Yüzde gibi dursun istiyorsan:
        valueText.text = iv.ToString() + "%";
    }
}
