using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdManager Instance;

    private const string _androidGameId = "5264066";
    private const string _iOSGameId = "5264067";
    private string _gameId;
    private bool _testMode = false;

    private Action onRewardedAdEndAction;

    //Ad names
    private string REWARDED_ANDROID = "Rewarded_Android";
    private string INTERSTITIAL_ANDROID = "Interstitial_Android";

    private const int countOfGamesToShowAd = 3;

    private int gameCountUntilAd = 0;

    private void Awake()
    {
        Instance = this;

        if (!Advertisement.isInitialized)
        { 
            InitializeAds();
        }
    }

    private void Start()
    {
        GameManager.Instance.OnGameStart += (a, b) => gameCountUntilAd++;

        GameManager.Instance.OnGameRestart += (a, b) =>
        {
            if (gameCountUntilAd >= countOfGamesToShowAd)
            {
                LoadInerstitialAd();
                gameCountUntilAd = 0;
            }
        };
    }

    public void InitializeAds()
    {
        _gameId = (Application.platform == RuntimePlatform.IPhonePlayer) ? _iOSGameId : _androidGameId;
        Advertisement.Initialize(_gameId, _testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        LoadBannerAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    public void LoadInerstitialAd()
    {
        Advertisement.Load(INTERSTITIAL_ANDROID, this);
    }

    public void LoadRewardedAd(Action onRewardedAdEndAction)
    {
        this.onRewardedAdEndAction = onRewardedAdEndAction;
        Advertisement.Load(REWARDED_ANDROID, this);
    }

    public static void LoadRewardedAd_Static(Action onRewardedAdEndAction)
    {
        Instance.LoadRewardedAd(onRewardedAdEndAction);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("OnUnityAdsAdLoaded");
        Advertisement.Show(placementId, this);
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {placementId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log("OnUnityAdsShowFailure");
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("OnUnityAdsShowStart");
        Time.timeScale = 0;
        Advertisement.Banner.Hide();
        AudioManager.Mute_Static();
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("OnUnityAdsShowClick");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("OnUnityAdsShowComplete " + showCompletionState);
        if (placementId.Equals("Rewarded_Android") && UnityAdsShowCompletionState.COMPLETED.Equals(showCompletionState))
        {
            onRewardedAdEndAction?.Invoke();
            Debug.Log("rewared Player");
        }
        Time.timeScale = 1;
        Advertisement.Banner.Show("Banner_Android");
        AudioManager.UnMute_Static();
    }



    public void LoadBannerAd()
    {
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Load("Banner_Android",
            new BannerLoadOptions
            {
                loadCallback = OnBannerLoaded,
                errorCallback = OnBannerError
            }
            );
    }

    void OnBannerLoaded()
    {
        Advertisement.Banner.Show("Banner_Android");
    }

    void OnBannerError(string message)
    {

    }
}
