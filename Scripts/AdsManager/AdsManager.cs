using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Omnilatent.AdsMediation;
using System;

public struct RewardResult
{
    public enum Type { LoadFailed = 0, Finished = 1, Canceled = 2, Loading = 3 }
    public Type type;
    public string message;

    public RewardResult(Type type, string message = "")
    {
        this.type = type;
        this.message = message;
    }

    public bool IsFinished { get { return type == Type.Finished; } }
}
public delegate void RewardDelegate(RewardResult result);

public class RemoteConfigAdsNetworkData
{
    public CustomMediation.AD_NETWORK adNetwork;
    public bool enable;
    public override string ToString()
    {
        return $"{adNetwork}, {enable}";
    }
}

public partial class AdsManager : MonoBehaviour
{
    [SerializeField] bool initializeAutomatically = true;
    private static bool initialized = false;
    public static bool Initialized { get => initialized; }

    public static Action<bool> OnInitializedEvent;

    public delegate void BoolDelegate(bool reward);

    public delegate void InterstitialDelegate(bool isSuccess = false);

    public static Action<AdPlacement.Type> OnInterAdLoadedEvent;
    public static Action<AdPlacement.Type, string> OnInterAdLoadFailedEvent;
    /// <summary>
    /// Called on inter ad closed. First param is ad placement type, second param is true if ad was displayed successfully.
    /// </summary>
    public static Action<AdPlacement.Type, bool> OnInterAdClosedEvent;

    public static Action<AdPlacement.Type, RewardResult> OnRewardAdClosedEvent;

    public static CustomMediation.AD_NETWORK CurrentAdNetwork { get { return CustomMediation.CurrentAdNetwork; } set { CustomMediation.CurrentAdNetwork = value; } }

    IAdsNetworkHelper _FANHelper;
    IAdsNetworkHelper _adMobHelper;
    IAdsNetworkHelper _unityAdsHelper;
    IAdsNetworkHelper _ironSourceHelper;
    IAdsNetworkHelper _MAXHelper; //AppLovin
    List<IAdsNetworkHelper> defaultAdsNetworkHelpers; //Default waterfall of ads network helper, start from index 0
    List<IAdsNetworkHelper> adsNetworkHelpers;
    IAdsNetworkHelper currentAdsHelper; //current ads helper, to keep consistency of whose interstitial ads was loaded
    IAdsNetworkHelper currentRewardInterAdsHelper; //current ads helper, to keep consistency of whose interstitial ads was loaded

    //List<CustomMediation.AD_NETWORK> showingBanners = new List<CustomMediation.AD_NETWORK>(); //store list of banners that was showed

    public delegate bool NoAdsDelegate();
    public NoAdsDelegate noAds;

    public delegate bool ConfigPlacementHideAds(AdPlacement.Type placementType);
    public ConfigPlacementHideAds configPlacementHideAds; //get remote config value to check if show or hide this placement

    public delegate void HandleOnAdLoadFailed(string message, string placementID); //TODO: param placement has to be string since ad library dont know what AdPlacementType it use
    public static HandleOnAdLoadFailed onAdLoadFailed;

    public delegate void HandleOnShowErrorMessage(RewardResult message, string placementID);
    public static HandleOnShowErrorMessage showErrorMessage; //show error message on ad load failed

    bool isDoneInitRemoteConfig;
    bool isLoadingInterstitial; //to prevent duplicate call of RequestInterstitial & duplicate callback when previous load isn't done yet. Should work when cacheInterstitial is false
    bool isLoadingAppOpenAd;
    const string admobManagerResourcesPath = "AdmobManager";

    float time; //counting time in app
    float timeLastShowInterstitial = -9999f; //the value of time when last interstitial was shown
    float timeLastShowAppOpenAd = -9999f; //the value of time when last app open ad was shown
    float timeLastShowRewardAd = -9999f; //the value of time when last reward ad was shown
    public static float TIME_BETWEEN_ADS = 18f; //minimum time between interstitial

    static float TIME_BETWEEN_APP_OPEN_ADS = 5f; //minimum time between app open ad
    public static float TimeBetweenAppOpenAds { get => TIME_BETWEEN_APP_OPEN_ADS; set => TIME_BETWEEN_APP_OPEN_ADS = value; }

    bool IsShowingBanner { get => currentShowingBanner != null; }
    AdPlacement.Type? currentShowingBanner = null;
    public AdPlacement.Type? CurrentShowingBanner { get => currentShowingBanner; }
    BannerTransform currentShowingBannerTransform;
    public BannerTransform CurrentShowingBannerTransform { get => currentShowingBannerTransform; }

    Dictionary<string, RemoteConfigAdsNetworkData> configData;

    List<CustomMediation.AD_NETWORK> defaultAdsNetworkPriority;
    public delegate List<CustomMediation.AD_NETWORK> ConfigPlacementAdsNetworkPriority(AdPlacement.Type placementType);
    public ConfigPlacementAdsNetworkPriority configPlacementAdsNetworkPriority; //get remote config value for waterfall order

    public const string RMCF_ADS_PRIORITY = "ads_priority";

    static System.Action<bool> onToggleLoading;
    /// <summary>
    /// Return true when an interstitial or reward ad's loading overlay is being displayed
    /// </summary>
    public static bool LoadingActive { get => loadingActive; }
    private static bool loadingActive;

    bool showingInterstitial;
    bool showingRewardAd;
    public bool ShowingInterstitial { get => showingInterstitial; }
    public bool ShowingRewardAd { get => showingRewardAd; }

    public static AdsManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject prefab = Resources.Load<GameObject>("AdsManager");
                instance = Instantiate(prefab).GetComponent<AdsManager>();
            }
            return instance;
        }
    }

    public static AdsManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        if (initializeAutomatically)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        if (initialized)
            return;
#if !DISABLE_SSCENE
        onToggleLoading = SS.View.Manager.LoadingAnimation;
#endif

        noAds += HasNoInternet;

        //init default ads helper
        defaultAdsNetworkHelpers = new List<IAdsNetworkHelper>();
        defaultAdsNetworkPriority = new List<CustomMediation.AD_NETWORK>();
        AddDefaultNetworkHelper(CustomMediation.AD_NETWORK.GoogleAdmob, InitAdmobManager());
#if !UNITY_EDITOR
        AddDefaultNetworkHelper(CustomMediation.AD_NETWORK.FAN, InitFANHelper());
#endif
        AddDefaultNetworkHelper(CustomMediation.AD_NETWORK.Unity, InitUnityAdsManager());
        _ironSourceHelper = AddDefaultNetworkHelper(CustomMediation.AD_NETWORK.IronSource, AdWrapperFinder.InitISHelper());
        _MAXHelper = AddDefaultNetworkHelper(CustomMediation.AD_NETWORK.AppLovinMAX, AdWrapperFinder.InitMAXHelper());
        adsNetworkHelpers = defaultAdsNetworkHelpers;
        //FirebaseRemoteConfigHelper.CheckAndHandleFetchConfig(SetupRemoteConfig); //switched to use RemoteConfigAdsPlacement
        initialized = true;
        OnInitializedEvent?.Invoke(initialized);
    }

    #region Deprecated codes
    /*void InitializeRemoteConfig(object sender, bool isReady)
    {
        int[] adsPriorityInt = GetRemoteAdsPriorityInt(isReady);
        if (adsPriorityInt == null) return;

        adsNetworkHelpers = new List<IAdsNetworkHelper>();
        for (int i = 0; i < adsPriorityInt.Length; i++)
        {
            adsNetworkHelpers.Add(defaultAdsNetworkHelpers[adsPriorityInt[i]]);
        }
        isDoneInitRemoteConfig = true;
        return;
    }*/

    /*int[] GetRemoteAdsPriorityInt(bool isReady)
    {
        //Initialize remote config
        //0 is admob, 1 is unity
        string adsPriorityStr = FirebaseRemoteConfigHelper.GetString(RMCF_ADS_PRIORITY, "");
        Debug.Log($"remote config value: {adsPriorityStr}");
        if (!isReady || isDoneInitRemoteConfig || string.IsNullOrEmpty(adsPriorityStr)) return null;
        var splitStr = adsPriorityStr.Split(',');
        if (splitStr.Length < defaultAdsNetworkHelpers.Count)
        {
            Debug.LogError("remote string not valid, length not match with number of ads helper");
            return null;
        }
        int[] adsPriorityInt = new int[defaultAdsNetworkHelpers.Count];
        for (int i = 0; i < splitStr.Length; i++)
        {
            if (!int.TryParse(splitStr[i], out adsPriorityInt[i]))
            {
                Debug.LogError("parse ads priority to int failed");
                return null;
            }
            Debug.Log($"ads priority int: {adsPriorityInt[i]}");
        }
        return adsPriorityInt;
    }*/

    /*void SetupRemoteConfig(object sender, bool isSuccess)
    {
        string configJsonData = FirebaseRemoteConfigHelper.GetString(RMCF_ADS_PRIORITY, null);
        if (!string.IsNullOrEmpty(configJsonData))
        {
            if ((configJsonData.StartsWith("{") && configJsonData.EndsWith("}")) || //For object
            (configJsonData.StartsWith("[") && configJsonData.EndsWith("]"))) //For array) 
            {
                configData = LitJson.JsonMapper.ToObject<Dictionary<string, RemoteConfigAdsNetworkData>>(configJsonData);

                adsNetworkHelpers = new List<IAdsNetworkHelper>();
                string deb = "ads_placement_config:\n";
                foreach (var item in configData)
                {
                    deb += ($"{item.Key} {item.Value}\n");
                    if (!item.Value.enable) continue;
                    var adsHelper = GetAdsNetworkHelper(item.Value.adNetwork);
                    if (adsHelper == null)
                    {
                        Debug.LogWarning("Reference to ads helper component is null, refused adding invalid ads helper");
                        continue;
                    }
                    AddNetworkHelper(item.Value.adNetwork, adsHelper);
                }
                Debug.Log(deb);
            }
            else
            {
                Debug.LogError($"AdsManager: {RMCF_ADS_PRIORITY} has invalid format. {configJsonData}");
            }
        }
        else
        {
            Debug.LogError($"AdsManager: {RMCF_ADS_PRIORITY} is null");
        }
    }*/
    #endregion

    public IAdsNetworkHelper GetAdsNetworkHelper(CustomMediation.AD_NETWORK adsNetworkID)
    {
        IAdsNetworkHelper adsHelper = null;
        switch (adsNetworkID)
        {
            case CustomMediation.AD_NETWORK.Unity:
                adsHelper = _unityAdsHelper;
                break;
            case CustomMediation.AD_NETWORK.GoogleAdmob:
                adsHelper = _adMobHelper;
                break;
            case CustomMediation.AD_NETWORK.FAN:
                adsHelper = _FANHelper;
                break;
            case CustomMediation.AD_NETWORK.IronSource:
                adsHelper = _ironSourceHelper;
                break;
            case CustomMediation.AD_NETWORK.AppLovinMAX:
                adsHelper = _MAXHelper;
                break;
        }
        if (adsHelper == null)
            Debug.LogError($"Reference to ads Helper of {adsNetworkID} is null. Check if the ad network was included in switch/case or its gameObject was instantiated correctly.");
        return adsHelper;
    }

    void AddNetworkHelper(CustomMediation.AD_NETWORK adsNetworkID, IAdsNetworkHelper adsHelper)
    {
        adsNetworkHelpers.Add(adsHelper);
    }

    IAdsNetworkHelper AddDefaultNetworkHelper(CustomMediation.AD_NETWORK adsNetworkID, IAdsNetworkHelper adsHelper)
    {
        if (adsHelper != null)
        {
            defaultAdsNetworkHelpers.Add(adsHelper);
            defaultAdsNetworkPriority.Add(adsNetworkID);
        }
        return adsHelper;
    }

    /// <summary>
    /// Get ads network priority order. If there is remote config, get from config, otherwise use default
    /// </summary>
    public List<CustomMediation.AD_NETWORK> GetAdsNetworkPriority(AdPlacement.Type placementType)
    {
        List<CustomMediation.AD_NETWORK> adPriority = null;
        if (configPlacementAdsNetworkPriority != null)
        {
            adPriority = configPlacementAdsNetworkPriority(placementType);
        }
        if (adPriority == null)
        {
            adPriority = defaultAdsNetworkPriority;
        }
        return adPriority;
    }

    public static bool HasNoInternet()
    {
        return (Application.internetReachability == NetworkReachability.NotReachable);
    }

    [System.Obsolete("Use ShowBanner(type, position, onAdLoaded) instead.")]
    public void ShowBanner(AdPlacement.Type placementType, BoolDelegate onAdLoaded = null)
    {
        if (IsShowingBanner) { Debug.Log("AdsManager: A banner is already being shown"); return; }
        if (DoNotShowAds(placementType))
        {
            onAdLoaded?.Invoke(false);
            return;
        };
        StartCoroutine(CoShowBanner(placementType, BannerTransform.defaultValue, onAdLoaded));
        /*switch (CurrentAdNetwork)
        {
            case CustomMediation.AD_NETWORK.Unity:
                UnityAdsManager.ShowBanner(CustomMediation.GetUnityPlacementId(placementType));
                break;
            case CustomMediation.AD_NETWORK.FAN:
                FacebookAudienceNetworkHelper.instance.ShowBanner(CustomMediation.GetFANPlacementId(placementType));
                break;
        }
        showingBanners.Add(CurrentAdNetwork);*/
    }

    public void ShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, BoolDelegate onAdLoaded = null)
    {
        if (IsShowingBanner) { Debug.Log("AdsManager: A banner is already being shown"); return; }
        if (DoNotShowAds(placementType))
        {
            onAdLoaded?.Invoke(false);
            return;
        };
        StartCoroutine(CoShowBanner(placementType, bannerTransform, onAdLoaded));
    }

    IEnumerator CoShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform = null, BoolDelegate onAdLoaded = null)
    {
        if (bannerTransform == null) bannerTransform = BannerTransform.defaultValue;

        bool isSuccess = false;
        WaitForSecondsRealtime checkInterval = new WaitForSecondsRealtime(0.3f);

        var adPriority = GetAdsNetworkPriority(placementType);

        for (int i = 0; i < adPriority.Count; i++)
        {
            bool checkAdNetworkDone = false;
            var adsHelper = GetAdsNetworkHelper(adPriority[i]);
            if (adsHelper == null) continue;
            adsHelper.ShowBanner(placementType, bannerTransform,
                (success) =>
                {
                    checkAdNetworkDone = true;
                    isSuccess = success;
                    onAdLoaded?.Invoke(success);
                    if (!success)
                    {
                        currentShowingBanner = null;
                        currentShowingBannerTransform = null;
                    }
                });
            while (!checkAdNetworkDone)
            {
                yield return checkInterval;
            }
            if (isSuccess)
            {
                //showingBanners.Add(CurrentAdNetwork);
                //isShowingBanner = true;
                currentShowingBanner = placementType;
                currentShowingBannerTransform = bannerTransform;
                break;
            }
        }
    }

    public void HideBanner()
    {
        if (!Initialized) return;
        foreach (var item in adsNetworkHelpers)
        {
            //Debug.Log("hiding banner " + item.ToString());
            HideBanner(item);
        }
        //showingBanners.Clear();
        //isShowingBanner = false;
        currentShowingBanner = null;
        currentShowingBannerTransform = null;
    }

    void HideBanner(IAdsNetworkHelper adNetwork)
    {
        adNetwork.HideBanner();
    }

    /// <param name="onAdClosed">Warning: not completely functional yet, only Admob will call onAdClosed when the interstitial is closed</param>
    public void ShowInterstitial(AdPlacement.Type placeType, InterstitialDelegate onAdClosed = null)
    {
        if (DoNotShowAds(placeType))
        {
            onAdClosed?.Invoke(false);
            return;
        }
        if (currentAdsHelper == null)
        {
            Debug.LogError("currentAdsHelper is null due to all ads failed to load");
            onAdClosed?.Invoke(false);
            return;
        }
        showingInterstitial = true;
        currentAdsHelper.ShowInterstitial(placeType, (success) =>
        {
            Instance.showingInterstitial = false;
            onAdClosed?.Invoke(success);
            OnInterAdClosedEvent?.Invoke(placeType, success);
        });
        timeLastShowInterstitial = time;
        /*switch (CurrentAdNetwork)
        {
            case CustomMediation.AD_NETWORK.Unity:
                UnityAdsManager.ShowInterstitial(CustomMediation.GetUnityPlacementId(placeType));
                break;
            case CustomMediation.AD_NETWORK.FAN:
                _FANHelper.ShowInterstitial(CustomMediation.GetFANPlacementId(placeType));
                break;
        }*/
    }

    public void RequestInterstitialNoShow(AdPlacement.Type placementType, InterstitialDelegate onAdLoaded = null, bool showLoading = true)
    {
        //if (DoNotShowAds(placementType) || !HasEnoughTimeBetweenInterstitial()) //skip checking interstitial time so we can use this function for preloading interstitial ads
        if (DoNotShowAds(placementType))
        {
            onAdLoaded?.Invoke(false);
            return;
        }
        if (isLoadingInterstitial)
        {
            string message = "Previous interstitial request is still loading";
            Debug.LogWarning(message);
            onAdLoaded?.Invoke(false); //added this so game can continue even with interstitial not finished loading
            OnInterAdLoadFailedEvent?.Invoke(placementType, message);
            return;
        }
        if (showLoading)
            ToggleLoading(true);
        StartCoroutine(CoRequestInterstitialNoShow(placementType, onAdLoaded, showLoading));
        //timeLastShowInterstitial = time; //moved to ShowInterstitial
        /*switch (CurrentAdNetwork)
        {
            case CustomMediation.AD_NETWORK.Unity:
                UnityAdsManager.instance.RequestInterstitialNoShow(CustomMediation.GetUnityPlacementId(placementType), onAdLoaded, showLoading);
                break;
            case CustomMediation.AD_NETWORK.FAN:
                FacebookAudienceNetworkHelper.instance.RequestInterstitialNoShow(CustomMediation.GetFANPlacementId(placementType), onAdLoaded, showLoading);
                break;
        }*/
    }

    IEnumerator CoRequestInterstitialNoShow(AdPlacement.Type placementType, InterstitialDelegate onAdLoaded = null, bool showLoading = true)
    {
        isLoadingInterstitial = true;
        bool isSuccess = false;
        WaitForSecondsRealtime checkInterval = new WaitForSecondsRealtime(0.05f);

        var adPriority = GetAdsNetworkPriority(placementType);

        for (int i = 0; i < adPriority.Count; i++)
        {
            bool checkAdNetworkDone = false;
            IAdsNetworkHelper adsHelper = GetAdsNetworkHelper(adPriority[i]);
            if (adsHelper == null) continue;
            adsHelper.RequestInterstitialNoShow(placementType,
                        (success) => { checkAdNetworkDone = true; isSuccess = success; },
                        showLoading);
            while (!checkAdNetworkDone)
            {
                yield return checkInterval;
            }
            if (isSuccess)
            {
                //CurrentAdNetwork = AdNetworkSetting.AdNetworks[i];
                currentAdsHelper = adsHelper;
                break;
            }
        }
        //.Log($"AdsManager: CoRequestInterstitialNoShow done {isSuccess}");
        onAdLoaded?.Invoke(isSuccess);
        if (isSuccess)
        {
            OnInterAdLoadedEvent?.Invoke(placementType);
        }
        else OnInterAdLoadFailedEvent?.Invoke(placementType, "All ad networks load failed");
        isLoadingInterstitial = false;
        if (showLoading)
            ToggleLoading(false);
    }

    /// <summary>
    /// Check if enough time has passed between interstitial, request and show ad, and preload the next ad
    /// </summary>
    public void RequestAndShowInterstitial(AdPlacement.Type placementType, AdRequestOption requestOption)
    {
        if (HasEnoughTimeBetweenInterstitial())
        {
            RequestInterstitialNoShow(placementType, (loadSuccess) =>
            {
                requestOption.onAdLoaded?.Invoke(loadSuccess);
                if (loadSuccess)
                {
                    ShowInterstitial(placementType, (showSuccess) =>
                    {
                        requestOption.onAdClosed?.Invoke(showSuccess);
                        if (showSuccess)
                        {
                            RequestInterstitialNoShow(placementType, showLoading: false);
                        }
                    });
                }
                else { requestOption.onAdClosed?.Invoke(false); }
            }, requestOption.showLoading);
        }
        else
        {
            requestOption.onAdLoaded?.Invoke(false);
            requestOption.onAdClosed?.Invoke(false);
        }
    }

    [System.Obsolete("Use Reward(AdPlacement.Type, RewardDelegate) instead.")]
    public static void Reward(BoolDelegate onFinish, AdPlacement.Type placementType)
    {
        ToggleLoading(true);
        instance.StartCoroutine(instance.CoReward((rewardResult) =>
        {
            onFinish.Invoke(rewardResult.type == RewardResult.Type.Finished);
        }, placementType));
    }

    public static void Reward(AdPlacement.Type placementType, RewardDelegate onFinish)
    {
        ToggleLoading(true);
        instance.StartCoroutine(instance.CoReward(onFinish, placementType));
    }

    IEnumerator CoReward(RewardDelegate onFinish, AdPlacement.Type placementType)
    {
        RewardResult rewardResult = new RewardResult();
        string errorMsg = string.Empty;

        if (IsAdsHiddenRemoteConfig(placementType))
        {
            rewardResult.type = RewardResult.Type.LoadFailed;
            rewardResult.message = "Disabled";
        }
        else if (HasNoInternet())
        {
            rewardResult.type = RewardResult.Type.LoadFailed;
            rewardResult.message = "No Internet connection";
        }
        else
        {
            timeLastShowRewardAd = time;
            showingRewardAd = true;
            WaitForSecondsRealtime checkInterval = new WaitForSecondsRealtime(0.3f);

            List<CustomMediation.AD_NETWORK> adPriority = GetAdsNetworkPriority(placementType);

            for (int i = 0; i < adPriority.Count; i++)
            {
                bool checkAdNetworkDone = false;
                IAdsNetworkHelper adsHelper = GetAdsNetworkHelper(adPriority[i]);
                if (adsHelper == null) continue;
                adsHelper.Reward(placementType, (result) =>
                {
                    checkAdNetworkDone = true; rewardResult = result;
                });
                while (!checkAdNetworkDone)
                {
                    yield return checkInterval;
                }
                if (rewardResult.type == RewardResult.Type.Finished)
                {
                    currentAdsHelper = adsHelper;
                    break;
                }
                if (rewardResult.type == RewardResult.Type.Canceled) { break; } //if a reward ads was shown and user skipped it, stop looking for more ads
            }
        }

        /*for (int i = 0; i < adsNetworkHelpers.Count; i++)
        {
            bool checkAdNetworkDone = false;
            adsNetworkHelpers[i].Reward(placementType, (result) =>
            {
                checkAdNetworkDone = true; rewardResult = result;
            });
            while (!checkAdNetworkDone)
            {
                yield return checkInterval;
            }
            if (rewardResult.type == RewardResult.Type.Finished)
            {
                currentAdsHelper = adsNetworkHelpers[i];
                break;
            }
            if (rewardResult.type == RewardResult.Type.Canceled) { break; } //if a reward ads was shown and user skipped it, stop looking for more ads
        }*/
        onFinish(rewardResult);
        OnRewardAdClosedEvent?.Invoke(placementType, rewardResult);
        ToggleLoading(false);
        showingRewardAd = false;
        if (rewardResult.type == RewardResult.Type.LoadFailed || rewardResult.type == RewardResult.Type.Loading)
        {
            if (rewardResult.type == RewardResult.Type.LoadFailed)
            {
                LogError(rewardResult.message, placementType.ToString());
            }
            ShowError(rewardResult, placementType.ToString());
        }
    }

    public void RequestInterstitialRewardedNoShow(AdPlacement.Type placementType, RewardDelegate onAdLoaded = null, bool showLoading = true)
    {
        if (IsAdsHiddenRemoteConfig(placementType))
        {
            onAdLoaded?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, "This Ads has been disabled"));
            return;
        }
        if (showLoading)
            ToggleLoading(true);
        StartCoroutine(CoRequestInterstitialRewardedNoShow(placementType, onAdLoaded, showLoading));
        //timeLastShowInterstitial = time;
    }

    IEnumerator CoRequestInterstitialRewardedNoShow(AdPlacement.Type placementType, RewardDelegate onAdLoaded = null, bool showLoading = true)
    {
        RewardResult rewardResult = new RewardResult();
        string errorMsg = string.Empty;
        WaitForSecondsRealtime checkInterval = new WaitForSecondsRealtime(0.05f);

        var adPriority = GetAdsNetworkPriority(placementType);

        for (int i = 0; i < adPriority.Count; i++)
        {
            bool checkAdNetworkDone = false;
            IAdsNetworkHelper adsHelper = GetAdsNetworkHelper(adPriority[i]);
            if (adsHelper == null) continue;
            adsHelper.RequestInterstitialRewardedNoShow(placementType,
                        (success) => { checkAdNetworkDone = true; rewardResult = success; });

            while (!checkAdNetworkDone) //Wait for ads load to complete
            {
                yield return checkInterval;
            }

            if (rewardResult.type == RewardResult.Type.Finished)
            {
                currentRewardInterAdsHelper = adsHelper;
                break;
            }
            if (rewardResult.type == RewardResult.Type.Canceled) { break; } //if a reward ads was shown and user skipped it, stop looking for more ads
        }
        if (showLoading)
            ToggleLoading(false);
        onAdLoaded?.Invoke(rewardResult);
    }

    public void ShowInterstitialRewarded(AdPlacement.Type placeType, RewardDelegate onAdClosed = null)
    {
        if (currentRewardInterAdsHelper == null)
        {
            Debug.LogError("currentRewardInterAdsHelper is null due to all ads failed to load");
            onAdClosed?.Invoke(new RewardResult(RewardResult.Type.LoadFailed));
            return;
        }
        currentRewardInterAdsHelper.ShowInterstitialRewarded(placeType, onAdClosed);
    }

    public void RequestAppOpenAd(AdPlacement.Type placementType, InterstitialDelegate onAdLoaded = null, bool showLoading = false)
    {
        if (DoNotShowAds(placementType))
        {
            onAdLoaded?.Invoke(false);
            return;
        }
        /*if (isLoadingAppOpenAd)
        {
            Debug.LogWarning("Previous app open ad request is still loading");
            onAdLoaded?.Invoke(false); //added this so game can continue even with interstitial not finished loading
            return;
        }*/
        if (showLoading)
            ToggleLoading(true);
        StartCoroutine(CoRequestAppOpenAd(placementType, onAdLoaded, showLoading));
    }

    IEnumerator CoRequestAppOpenAd(AdPlacement.Type placementType, InterstitialDelegate onAdLoaded = null, bool showLoading = false)
    {
        isLoadingAppOpenAd = true;
        bool isSuccess = false;
        WaitForSecondsRealtime checkInterval = new WaitForSecondsRealtime(0.05f);
        var adsHelper = GetAdsNetworkHelper(CustomMediation.AD_NETWORK.GoogleAdmob);
        bool checkAdNetworkDone = false;
        if (adsHelper != null)
        {
            adsHelper.RequestAppOpenAd(placementType,
                        (result) => { checkAdNetworkDone = true; isSuccess = result.type == RewardResult.Type.Finished; });
            while (!checkAdNetworkDone)
            {
                yield return checkInterval;
            }
        }

        //.Log($"AdsManager: CoRequestInterstitialNoShow done {isSuccess}");
        onAdLoaded?.Invoke(isSuccess);
        isLoadingAppOpenAd = false;
        if (showLoading)
            ToggleLoading(false);
    }

    public void ShowAppOpenAd(AdPlacement.Type placementType, InterstitialDelegate onAdClosed = null)
    {
        if (DoNotShowAds(placementType) || !HasEnoughTimeBetweenAppOpenAd())
        {
            onAdClosed?.Invoke(false);
            return;
        }
        var adsHelper = GetAdsNetworkHelper(CustomMediation.AD_NETWORK.GoogleAdmob);
        if (adsHelper != null)
        {
            adsHelper.ShowAppOpenAd(placementType, onAdClosed);
            timeLastShowAppOpenAd = time;
        }
        else
        {
            Debug.LogError("Show Open Ad failed. No Admob Helper.");
            onAdClosed?.Invoke(false);
        }
    }

    public static void ShowError(RewardResult msg, string placementType)
    {
        showErrorMessage?.Invoke(msg, placementType);
        //string text = string.Format("There was a problem displaying this ads. {0}. Please try again later.", msg);
        //FirebaseManager.LogEvent($"AdsError_{placementName}", "message", msg);
    }

    public static void LogError(string msg, string placementType)
    {
        onAdLoadFailed?.Invoke(msg, placementType);
    }

    public float GetTimeSinceLastShowInterstitial() { return time - timeLastShowInterstitial; }
    public float GetTimeSinceLastShowRewardAd() { return time - timeLastShowRewardAd; }

    public bool HasEnoughTimeBetweenInterstitial()
    {
        bool enoughTimeHasPassed = GetTimeSinceLastShowInterstitial() >= TIME_BETWEEN_ADS;
        //.Log($"time between inter {time - timeLastShowInterstitial}");
        return enoughTimeHasPassed;
    }

    public bool HasEnoughTimeBetweenAppOpenAd() //Different from Interstitial, this is to prevent repeated app open ad show when resuming game
    {
        bool enoughTimeHasPassed = (time - timeLastShowAppOpenAd) >= TIME_BETWEEN_APP_OPEN_ADS;
        return enoughTimeHasPassed;
    }

    /// <summary>
    /// Check if user has purchased remove ads and remote config. Return true if this ads is hidden
    /// </summary>
    public bool DoNotShowAds(AdPlacement.Type placementType)
    {
        if (!Initialized)
        {
            Debug.LogError("AdsManager has not been initialized.");
            return true;
        }
        bool isNoAds = false;
        if (noAds != null)
        {
            var noAdsInvokeList = noAds.GetInvocationList();
            for (int i = 0; i < noAdsInvokeList.Length; i++)
            {
                isNoAds = isNoAds || (bool)noAdsInvokeList[i].DynamicInvoke();
                //.Log($"AdsManager: {noAdsInvokeList[i].Method.Name}");
            }
        }
        //.Log($"AdsManager: do not show ads {placementType}: {isNoAds}");
        if (isNoAds) return true;
        if (IsAdsHiddenRemoteConfig(placementType)) return true;
        return false;
    }

    /// <summary>
    /// Check remote config for this ads placement. Return true if this ads is disabled
    /// </summary>
    bool IsAdsHiddenRemoteConfig(AdPlacement.Type placementType)
    {
        if (configPlacementHideAds != null && configPlacementHideAds(placementType)) return true;
        return false;
    }

    static void ToggleLoading(bool active)
    {
        loadingActive = active;
        if (onToggleLoading == null)
        {
            Debug.LogWarning($"Loading overlay should be toggled {active}. Assign a function to this Action or use SS library");
            return;
        }
        onToggleLoading.Invoke(active);
    }

    private void Update()
    {
        time += Time.deltaTime;
    }
}
