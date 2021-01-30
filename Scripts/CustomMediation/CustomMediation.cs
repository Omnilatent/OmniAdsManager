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

public static partial class CustomMediation
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

}