using SubjectNerd.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AdNetworkSetting", menuName = "Ad Network Setting")]
public class AdNetworkSetting : ScriptableObject
{
    [Tooltip("Priority from top to bottom, (0 first)")]
    //[Reorderable]
    [SerializeField] private List<CustomMediation.AD_NETWORK> adNetworks;

    public static AdNetworkSetting Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<AdNetworkSetting>("AdNetworkSetting");
            }
            return instance;
        }
    }

//#if UNITY_EDITOR
//    public static List<CustomMediation.AD_NETWORK> AdNetworks = new List<CustomMediation.AD_NETWORK>() { CustomMediation.AD_NETWORK.Unity };
//#else
    public static List<CustomMediation.AD_NETWORK> AdNetworks { get => Instance.adNetworks; }
//#endif
    public static AdNetworkSetting instance;
}
