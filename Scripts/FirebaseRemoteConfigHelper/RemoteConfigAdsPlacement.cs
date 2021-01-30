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
    Dictionary<string, RemoteConfigAdsPlacementData> configData;
    public const string RMCF_ADS_PLACEMENT_CONFIG = "ads_placement_config_2";

    void Start()
    {
        FirebaseRemoteConfigHelper.CheckAndHandleFetchConfig(SetupRemoteConfig);
    }

    void SetupRemoteConfig(object sender, bool isSuccess)
    {
        string configJsonData = FirebaseRemoteConfigHelper.GetString(RMCF_ADS_PLACEMENT_CONFIG, null);

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
            foreach (var item in configSheetData)
            {
                deb += ($"{item.Key} {item.Value.show}\n");
            }
            Debug.Log(deb);*/

            AdsManager.instance.configPlacementHideAds += CheckHideAds;
            AdsManager.instance.configPlacementAdsNetworkPriority += GetAdsNetworkPriority;
        }
        else
        {
            Debug.LogError("RemoteConfigAdsPlacement: ads_placement_config is null");
        }
    }

    RemoteConfigAdsPlacementData GetPlacementConfigData(AdPlacementType placementType)
    {
        string key = ((int)placementType).ToString();
        if (configData != null && configData.ContainsKey(key)) return configData[key];
        Debug.LogError($"Config for placement {placementType} not found");
        return null;
    }

    bool CheckHideAds(AdPlacementType placementType)
    {
        /*configData = new Dictionary<string, RemoteConfigAdsData>() {
             {"1", new RemoteConfigAdsData()},
        };*/
        var config = GetPlacementConfigData(placementType);
        if (config != null) return !config.show;
        return false;
    }

    List<CustomMediation.AD_NETWORK> GetAdsNetworkPriority(AdPlacementType placementType)
    {
        var config = GetPlacementConfigData(placementType);
        if (config != null)
        {
            return config.GetAdNetworkPriority();
        }
        return null;
    }
}
