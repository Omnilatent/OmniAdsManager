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
            //.Log($"{unityAdsManagerResourcesPath} not found in Resources");
            return null;
        }
        _unityAdsHelper = Instantiate(resGO).GetComponent<IAdsNetworkHelper>();
        return _unityAdsHelper;
    }
#else
    IAdsNetworkHelper InitFANHelper()
    {
        return null;
    }
#endif
}
