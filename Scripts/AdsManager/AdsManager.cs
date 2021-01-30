using SS.View;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RewardResult
{
    public enum Type { LoadFailed = 0, Finished = 1, Canceled = 2 }
    public Type type;
    public string message;

    public RewardResult(Type type, string message = "")
    {
        this.type = type;
        this.message = message;
    }
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
    public delegate void BoolDelegate(bool reward);

    public delegate void InterstitialDelegate(bool isSuccess = false);
    public InterstitialDelegate interstitialFinishDelegate;
    public InterstitialDelegate interstitialLoadedDelegate;

    public static CustomMediation.AD_NETWORK CurrentAdNetwork { get { return CustomMediation.CurrentAdNetwork; } set { CustomMediation.CurrentAdNetwork = value; } }

    IAdsNetworkHelper _FANHelper;
    IAdsNetworkHelper _adMobHelper;
    IAdsNetworkHelper _unityAdsHelper;
    List<IAdsNetworkHelper> defaultAdsNetworkHelpers; //Default waterfall of ads network helper, start from index 0
    List<IAdsNetworkHelper> adsNetworkHelpers;
    IAdsNetworkHelper currentAdsHelper; //current ads helper, to keep consistency of whose interstitial ads was loaded

    //List<CustomMediation.AD_NETWORK> showingBanners = new List<CustomMediation.AD_NETWORK>(); //store list of banners that was showed

    public delegate bool NoAdsDelegate();
    public NoAdsDelegate noAds;

    public delegate bool ConfigPlacementHideAds(AdPlacement.Type placementType);
    public ConfigPlacementHideAds configPlacementHideAds; //get remote config value to check if show or hide this placement

    public delegate void HandleOnAdLoadFailed(string message, AdPlacement.Type placementType);
    public static HandleOnAdLoadFailed onAdLoadFailed;

    bool isDoneInitRemoteConfig;
    bool isLoadingInterstitial; //to prevent duplicate call of RequestInterstitial & duplicate callback when previous load isn't done yet. Should work when cacheInterstitial is false
    const string admobManagerResourcesPath = "AdmobManager";

    float time; //counting time in app
    float timeLastShowInterstitial = -9999f; //the value of time when last interstitial was shown
    public static float TIME_BETWEEN_ADS = 18f; //minimum time between interstitial

    bool isShowingBanner = false;

    Dictionary<string, RemoteConfigAdsNetworkData> configData;

    List<CustomMediation.AD_NETWORK> defaultAdsNetworkPriority;
    public delegate List<CustomMediation.AD_NETWORK> ConfigPlacementAdsNetworkPriority(AdPlacement.Type placementType);
    public ConfigPlacementAdsNetworkPriority configPlacementAdsNetworkPriority; //get remote config value for waterfall order

    public const string RMCF_ADS_PRIORITY = "ads_priority";

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
        }
        DontDestroyOnLoad(gameObject);
        noAds += HasNoInternet;

        //init default ads helper
        defaultAdsNetworkHelpers = new List<IAdsNetworkHelper>();
        defaultAdsNetworkPriority = new List<CustomMediation.AD_NETWORK>();
#if UNITY_EDITOR && UNITYADS
        if ((InitAdmobManager() as IAdsNetworkHelper) == null) Debug.LogError("admob manager not found");
        AddDefaultNetworkHelper(CustomMediation.AD_NETWORK.Unity, InitUnityAdsManager());
#else
        AddDefaultNetworkHelper(CustomMediation.AD_NETWORK.GoogleAdmob, InitAdmobManager());
#if UNITYADS
        AddDefaultNetworkHelper(CustomMediation.AD_NETWORK.Unity, InitUnityAdsManager());
#endif
#if !UNITY_EDITOR
        AddDefaultNetworkHelper(CustomMediation.AD_NETWORK.FAN, InitFANHelper());
#endif
#endif
        adsNetworkHelpers = defaultAdsNetworkHelpers;
        //FirebaseRemoteConfigHelper.CheckAndHandleFetchConfig(SetupRemoteConfig); //switched to use RemoteConfigAdsPlacement
    }

#if UNITYADS
    UnityAdsManager InitUnityAdsManager()
    {
        _unityAdsHelper = gameObject.GetComponent<UnityAdsManager>() as IAdsNetworkHelper;
        return _unityAdsHelper as UnityAdsManager;
    }
#endif

    void InitializeRemoteConfig(object sender, bool isReady)
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
    }

    int[] GetRemoteAdsPriorityInt(bool isReady)
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
    }

    void SetupRemoteConfig(object sender, bool isSuccess)
    {
        string configJsonData = FirebaseRemoteConfigHelper.GetString(RMCF_ADS_PRIORITY, null);
        if (!string.IsNullOrEmpty(configJsonData))
        {
            if ((configJsonData.StartsWith("{") && configJsonData.EndsWith("}")) || //For object
            (configJsonData.StartsWith("[") && configJsonData.EndsWith("]"))) //For array) 
            {
                configData = JsonUtility.FromJson<Dictionary<string, RemoteConfigAdsNetworkData>>(configJsonData);

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
    }

    IAdsNetworkHelper GetAdsNetworkHelper(CustomMediation.AD_NETWORK adsNetworkID)
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
        }
        if (adsHelper == null)
            Debug.LogError($"Reference to ads Helper of {adsNetworkID} is null");
        return adsHelper;
    }

    void AddNetworkHelper(CustomMediation.AD_NETWORK adsNetworkID, IAdsNetworkHelper adsHelper)
    {
        adsNetworkHelpers.Add(adsHelper);
    }

    void AddDefaultNetworkHelper(CustomMediation.AD_NETWORK adsNetworkID, IAdsNetworkHelper adsHelper)
    {
        if (adsHelper != null)
        {
            defaultAdsNetworkHelpers.Add(adsHelper);
            defaultAdsNetworkPriority.Add(adsNetworkID);
        }
    }

    /// <summary>
    /// Get ads network priority order. If there is remote config, get from config, otherwise use default
    /// </summary>
    List<CustomMediation.AD_NETWORK> GetAdsNetworkPriority(AdPlacement.Type placementType)
    {
        List<CustomMediation.AD_NETWORK> adPriority;
        if (configPlacementAdsNetworkPriority != null)
        {
            adPriority = configPlacementAdsNetworkPriority(placementType);
        }
        else
        {
            adPriority = defaultAdsNetworkPriority;
        }
        return adPriority;
    }

    public static bool HasNoInternet()
    {
        return (Application.internetReachability == NetworkReachability.NotReachable);
    }

    public void ShowBanner(AdPlacement.Type placementType, BoolDelegate onAdLoaded = null)
    {
        if (isShowingBanner) { Debug.Log("AdsManager: A banner is already being shown"); return; }
        if (DoNotShowAds(placementType))
        {
            onAdLoaded?.Invoke(false);
            return;
        };
        StartCoroutine(CoShowBanner(placementType, onAdLoaded));
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

    IEnumerator CoShowBanner(AdPlacement.Type placementType, BoolDelegate onAdLoaded = null)
    {
        bool isSuccess = false;
        WaitForSecondsRealtime checkInterval = new WaitForSecondsRealtime(0.3f);

        var adPriority = GetAdsNetworkPriority(placementType);

        for (int i = 0; i < adPriority.Count; i++)
        {
            bool checkAdNetworkDone = false;
            var adsHelper = GetAdsNetworkHelper(adPriority[i]);
            if (adsHelper == null) continue;
            adsHelper.ShowBanner(placementType,
                (success) => { checkAdNetworkDone = true; isSuccess = success; onAdLoaded?.Invoke(success); });
            while (!checkAdNetworkDone)
            {
                yield return checkInterval;
            }
            if (isSuccess)
            {
                //showingBanners.Add(CurrentAdNetwork);
                isShowingBanner = true;
                break;
            }
        }
    }

    public void HideBanner()
    {
        foreach (var item in adsNetworkHelpers)
        {
            //Debug.Log("hiding banner " + item.ToString());
            HideBanner(item);
        }
        //showingBanners.Clear();
        isShowingBanner = false;
    }

    void HideBanner(IAdsNetworkHelper adNetwork)
    {
        adNetwork.HideBanner();
    }

    /// <param name="onAdClosed">Warning: not completely functional yet, only Admob will call onAdClosed when the interstitial is closed</param>
    public void ShowInterstitial(AdPlacement.Type placeType, InterstitialDelegate onAdClosed = null)
    {
        if (currentAdsHelper == null)
        {
            Debug.LogError("currentAdsHelper is null due to all ads failed to load");
            return;
        }
        currentAdsHelper.ShowInterstitial(placeType, onAdClosed);
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
            Debug.LogWarning("Previous interstitial request is still loading");
            onAdLoaded?.Invoke(false); //added this so game can continue even with interstitial not finished loading
            return;
        }
        if (showLoading)
            Manager.LoadingAnimation(true);
        StartCoroutine(CoRequestInterstitialNoShow(placementType, onAdLoaded, showLoading));
        timeLastShowInterstitial = time;
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
        isLoadingInterstitial = false;
        if (showLoading)
            Manager.LoadingAnimation(false);
    }


    public static void Reward(BoolDelegate onFinish, AdPlacement.Type placementType)
    {
        instance.StartCoroutine(instance.CoReward(onFinish, placementType));
    }

    IEnumerator CoReward(BoolDelegate onFinish, AdPlacement.Type placementType)
    {
        RewardResult rewardResult = new RewardResult(); string errorMsg = string.Empty;
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
        onFinish(rewardResult.type == RewardResult.Type.Finished);
        Manager.LoadingAnimation(false);
        if (rewardResult.type == RewardResult.Type.LoadFailed) { ShowError(rewardResult.message, placementType); }
    }

    public static void ShowError(string msg, AdPlacement.Type placementType)
    {
        onAdLoadFailed?.Invoke(msg, placementType);
    }

    public static void ShowError(string msg, string placementName)
    {
        //string text = string.Format("There was a problem displaying this ads. {0}. Please try again later.", msg);
        //Manager.Add(PopupController.POPUP_SCENE_NAME, new PopupData(PopupType.OK, text));
        //FirebaseManager.LogEvent($"AdsError_{placementName}", "message", msg);
    }

    public bool HasEnoughTimeBetweenInterstitial()
    {
        bool enoughTimeHasPassed = (time - timeLastShowInterstitial) >= TIME_BETWEEN_ADS;
        //.Log($"time between inter {time - timeLastShowInterstitial}");
        return enoughTimeHasPassed;
    }

    bool DoNotShowAds(AdPlacement.Type placementType)
    {
        bool isNoAds = false;
        if (noAds != null)
        {
            var noAdsInvokeList = noAds.GetInvocationList();
            for (int i = 0; i < noAdsInvokeList.Length; i++)
            {
                isNoAds = isNoAds || (bool)noAdsInvokeList[i].DynamicInvoke();
            }
        }
        //.Log($"AdsManager: do not show ads {placementType}: {isNoAds}");
        if (isNoAds) return true;
        if (configPlacementHideAds != null && configPlacementHideAds(placementType)) return true;
        return false;
    }

    private void Update()
    {
        time += Time.deltaTime;
    }
}
