using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AdPlacementType
{
    Banner = 1,
    Interstitial = 2,
    Reward_Skip = 3,
    Inter_Splash = 4,
    Reward_GetMoreHint = 5,
}

public static class CustomMediation
{
    public enum AD_NETWORK { None = 0, Unity = 1, FAN = 2, GoogleAdmob = 3 }
    private static AD_NETWORK currentAdNetwork;

    public static AD_NETWORK CurrentAdNetwork
    {
        get => currentAdNetwork;
        set
        {
#if SWITCHABLE_AD_NETWORK
            PlayerPrefs.SetInt(PREF_AD_NETWORK, (int)value);
#endif
            currentAdNetwork = value;
        }
    }

    static CustomMediation()
    {
        CheckAdNetwork();
    }

    static void CheckAdNetwork()
    {
        currentAdNetwork = AdNetworkSetting.AdNetworks[0];
#if SWITCHABLE_AD_NETWORK && !UNITY_EDITOR
        currentAdNetwork = (AD_NETWORK)PlayerPrefs.GetInt(PREF_AD_NETWORK, 0);
#elif UNITY_EDITOR
        currentAdNetwork = AD_NETWORK.Unity;
#endif
    }
    const string PREF_AD_NETWORK = "PREFERED_AD_NETWORK";

    public static string GetUnityPlacementId(AdPlacementType adPlacementType)
    {
        string placementId = string.Empty;
        switch (adPlacementType)
        {
            case AdPlacementType.Banner:
                placementId = "UNITY_GameDraw_Banner_50"; break;
            case AdPlacementType.Interstitial:
                placementId = "UNITY_CompleteLevel_Interstitial"; break;
            case AdPlacementType.Reward_Skip:
                placementId = "UNITY_ClickButtonSkip_Rewarded"; break;
            case AdPlacementType.Reward_GetMoreHint:
                placementId = "UNITY_Getmorehint_Rewarded"; break;
            case AdPlacementType.Inter_Splash:
                placementId = "UNITY_Splash_Interstitial"; break;
        }
        if (placementId == string.Empty)
        {
            Debug.LogError($"Custom Mediation: {adPlacementType} has no Unity Ads ID, default ID will be used");
        }
        return placementId;
    }

    public static string GetFANPlacementId(AdPlacementType adPlacementType)
    {
        string placementId = string.Empty;
        //test VID_HD_16_9_15S_LINK
        switch (adPlacementType)
        {
            case AdPlacementType.Banner:
                placementId = "896546937757236_898375510907712";
                break;
            case AdPlacementType.Interstitial:
                placementId = "896546937757236_898374294241167";
                break;
            case AdPlacementType.Reward_Skip:
                placementId = "896546937757236_898377787574151";
                break;
            case AdPlacementType.Inter_Splash:
                placementId = "";
                break;
            case AdPlacementType.Reward_GetMoreHint:
                placementId = "896546937757236_898377640907499";
                break;
        }
#if DEBUG_ADS
        placementId = GetFANTestAdsID(placementId, adPlacementType);
#endif
        return placementId;
    }

    static string GetFANTestAdsID(string placementID, AdPlacementType placementType)
    {
        if (string.IsNullOrEmpty(placementID)) return placementID;
        string testID = $"VID_HD_16_9_15S_APP_INSTALL#{placementID}";
        switch (placementType)
        {
            case AdPlacementType.Banner:
                testID = $"IMG_16_9_LINK#{placementID}";
                break;
        }
        return testID;
    }

    public static string GetAdmobID(AdPlacementType adPlacementType, string defaultID)
    {
        string id = defaultID;
        switch (adPlacementType)
        {
            case AdPlacementType.Banner:
                id = AdMobConst.BANNER_ID;
                break;
            case AdPlacementType.Interstitial:
                id = AdMobConst.INTERSTITIAL_SPLASH;
                break;
            case AdPlacementType.Reward_Skip:
                id = AdMobConst.REWARD_SKIP_ID;
                break;
            case AdPlacementType.Reward_GetMoreHint:
                id = AdMobConst.REWARD_GET_MORE_HINT_ID;
                break;
            case AdPlacementType.Inter_Splash:
                id = AdMobConst.INTERSTITIAL_SPLASH;
                break;
        }
        return id;
    }
}