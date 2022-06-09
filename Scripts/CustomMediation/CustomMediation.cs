using System;
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

        public override string ToString()
        {
            return _Value.ToString();
        }
    }

    [Obsolete("Declare custom AdPlacement in AdditionalPlacementID.cs")]
    public static Type Banner = 1001;

    [Obsolete("Declare custom AdPlacement in AdditionalPlacementID.cs")]
    public static Type Interstitial = 1002;

    //[Obsolete("Declare custom AdPlacement in AdditionalPlacementID.cs")]
    public static Type Reward = 1003;

    public static readonly Type App_Open_Ad = 1005;
}

public static partial class CustomMediation
{
    public enum AD_NETWORK
    {
        None = 0, Unity = 1, FAN = 2, GoogleAdmob = 3,
        IronSource = 4
    }
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
}