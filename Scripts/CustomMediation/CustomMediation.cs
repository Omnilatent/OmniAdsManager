using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AdPlacement
{
    public struct Type
    {
        private int _Value;

        public static implicit operator Type(int value)
        {
            return new Type { _Value = value };
        }

        public static implicit operator int(Type value)
        {
            return value._Value;
        }
    }
    public static readonly Type Banner = 1;
    public static readonly Type Interstitial = 2;
    public static readonly Type Reward_Skip = 3;
    public static readonly Type Inter_Splash = 4;
    public static readonly Type Reward_GetMoreHint = 5;
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

    public static string GetUnityPlacementId(AdPlacement.Type adPlacementType)
    {
        string placementId = string.Empty;

        if(adPlacementType == AdPlacement.Banner)
        {
            placementId = "UNITY_GameDraw_Banner_50";
        }
        else if (adPlacementType == AdPlacement.Interstitial)
        {
            placementId = "UNITY_GameDraw_Banner_50";
        }
        else if (adPlacementType == AdPlacement.Reward_GetMoreHint)
        {
            placementId = "UNITY_GameDraw_Banner_50";
        }

        if (placementId == string.Empty)
        {
            Debug.LogError($"Custom Mediation: {adPlacementType} has no Unity Ads ID, default ID will be used");
        }
        return placementId;
    }

}