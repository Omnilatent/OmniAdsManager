using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public partial class AdsManager : MonoBehaviour
{
#if true //MODULE_MAKER
    const string unityAdsManagerResourcesPath = "UnityAdsHelper";
    IAdsNetworkHelper InitUnityAdsManager()
    {
        var resGO = Resources.Load<GameObject>(unityAdsManagerResourcesPath);
        if (resGO == null)
        {
            Debug.Log($"{unityAdsManagerResourcesPath} not found in Resources");
            return null;
        }
        var adsHelperGO = Instantiate(resGO);
        return adsHelperGO.GetComponent<IAdsNetworkHelper>();
    }
#else
    IAdsNetworkHelper InitFANHelper()
    {
        return null;
    }
#endif
}
