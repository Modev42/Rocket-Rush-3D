using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdsManager : MonoBehaviour
{
    [Header("AdMob Birim ID'leri (Slash '/' olanlar)")]
    // Buraya AdMob panelinden aldığın Reklam Birimi kodlarını yapıştır
    public string bannerUnitId = "ca-app-pub-5367730823626710/9819694702"; 
    public string interstitialUnitId = "ca-app-pub-5367730823626710/4466026758";

    private BannerView _bannerView;
    private InterstitialAd _interstitialAd;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        // Google Mobile Ads SDK'sını başlatır
        MobileAds.Initialize((InitializationStatus status) =>
        {
            // SDK başladıktan sonra reklamları yükle
            LoadBannerAd();
            LoadInterstitialAd();
        });
    }

    #region BANNER (Şerit Reklam)
    public void LoadBannerAd()
    {
        // Eğer varsa eski banner'ı temizle
        if (_bannerView != null)
        {
            _bannerView.Destroy();
        }

        // Yeni banner oluştur (Boyut: Standart Banner, Pozisyon: Alt Orta)
        _bannerView = new BannerView(bannerUnitId, AdSize.Banner, AdPosition.Bottom);
        
        // Reklam isteği oluştur ve yükle
        AdRequest adRequest = new AdRequest();
        _bannerView.LoadAd(adRequest);
    }
    #endregion

    #region INTERSTITIAL (Geçiş Reklamı)
    public void LoadInterstitialAd()
    {
        // Eski reklamı temizle
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        var adRequest = new AdRequest();

        // Reklamı arka planda yükle
        InterstitialAd.Load(interstitialUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Geçiş reklamı yüklenemedi: " + error);
                return;
            }

            _interstitialAd = ad;
        });
    }

    // BU FONKSİYONU CONTINUE BUTONUNA BAĞLAYACAKSIN
    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _interstitialAd.Show();
            LoadInterstitialAd(); // Bir sonraki kullanım için yenisini yükle
        }
        else
        {
            Debug.Log("Reklam henüz hazır değil, oyun devam ediyor.");
            // Reklam yoksa bile oyuncuyu bekletmemek için buraya 'Devam Et' kodunu ekleyebilirsin
        }
    }
    #endregion
}