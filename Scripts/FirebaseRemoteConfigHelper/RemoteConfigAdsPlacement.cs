using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteConfigAdsPlacementData
{
    public int placementID;
    public bool show;
    public List<int> priority;
    List<CustomMediation.AD_NETWORK> adNetworkPriority;
    public List<CustomMediation.AD_NETWORK> GetAdNetworkPriority()
    {
        if (adNetworkPriority == null)
        {
            adNetworkPriority = new List<CustomMediation.AD_NETWORK>();
            for (int i = 0; i < priority.Count; i++)
            {
                adNetworkPriority.Add((CustomMediation.AD_NETWORK)priority[i]);
            }
        }
        return adNetworkPriority;
    }

    public override string ToString()
    {
        return $"{placementID}: show:{show}, priority:{priority}";
    }
}

/* Example Firebase string config: ads_placement_config 
 * {"1":{"show":false},"2":{"show":false}} 
 */
public class RemoteConfigAdsPlacement : MonoBehaviour
{
    [Tooltip("Remote Config key set on Firebase")]
    public string adsPlacementConfigKey = "ads_placement_config_2";
    Dictionary<string, RemoteConfigAdsPlacementData> configData;
    //public const string RMCF_ADS_PLACEMENT_CONFIG = "ads_placement_config_2";

    public static RemoteConfigAdsPlacement instance;
    public static Action<bool> OnRemoteConfigFetchCompleted;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
#if !DISABLE_FIREBASE
        FirebaseRemoteConfigHelper.CheckAndHandleFetchConfig(SetupRemoteConfig);
#endif
    }

#if !DISABLE_FIREBASE
    void SetupRemoteConfig(object sender, bool isSuccess)
    {
        string configJsonData = FirebaseRemoteConfigHelper.GetString(adsPlacementConfigKey, null);

        //test json data
        //configJsonData = Resources.Load<TextAsset>("TestPlacementConfig").text;

        if (!string.IsNullOrEmpty(configJsonData))
        {
            //configData = LitJson.JsonMapper.ToObject<Dictionary<string, RemoteConfigAdsPlacementData>>(configJsonData);
            var data = LitJson.JsonMapper.ToObject<Dictionary<string, List<RemoteConfigAdsPlacementData>>>(configJsonData);
            List<RemoteConfigAdsPlacementData> listData = null;
            foreach (var item in data)
            {
                listData = item.Value;
            }

            configData = new Dictionary<string, RemoteConfigAdsPlacementData>();
            for (int i = 0; i < listData.Count; i++)
            {
                configData.Add(listData[i].placementID.ToString(), listData[i]);
            }

            /*string deb = "ads_placement_config:\n";
            foreach (var item in configData)
            {
                if (item.Value.priority == null)
                {
                    Debug.LogError("Ad Placement Config item priority is null");
                    Debug.LogError(item.Key);
                }
                deb += ($"{item.Key} {item.Value.show} {item.Value.priority[0]}\n");
            }
            Debug.Log(deb);*/

            AdsManager.instance.configPlacementHideAds += CheckHideAds;
            AdsManager.instance.configPlacementAdsNetworkPriority += GetAdsNetworkPriority;
        }
        else
        {
            Debug.LogError("RemoteConfigAdsPlacement: ads_placement_config is null");
        }
        OnRemoteConfigFetchCompleted?.Invoke(isSuccess);
    }
#endif

    RemoteConfigAdsPlacementData GetPlacementConfigData(AdPlacement.Type placementType)
    {
        string key = ((int)placementType).ToString();
        if (configData != null && configData.ContainsKey(key)) return configData[key];
        Debug.LogError($"Config for placement {placementType} not found");
        return null;
    }

    bool CheckHideAds(AdPlacement.Type placementType)
    {
        /*configData = new Dictionary<string, RemoteConfigAdsData>() {
             {"1", new RemoteConfigAdsData()},
        };*/
        var config = GetPlacementConfigData(placementType);
        if (config != null) return !config.show;
        return false;
    }

    List<CustomMediation.AD_NETWORK> GetAdsNetworkPriority(AdPlacement.Type placementType)
    {
        var config = GetPlacementConfigData(placementType);
        if (config != null)
        {
            return config.GetAdNetworkPriority();
        }
        else { Debug.LogError($"Config for placement {placementType} not found. Check Firebase remote config."); }
        return null;
    }
}
