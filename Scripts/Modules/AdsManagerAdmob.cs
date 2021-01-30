using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public partial class AdsManager : MonoBehaviour
{
#if true //MODULE_MAKER
    IAdsNetworkHelper InitAdmobManager()
    {
        var resGO = Resources.Load<GameObject>(admobManagerResourcesPath);
        if (resGO == null)
        {
            throw new System.NullReferenceException($"{admobManagerResourcesPath} not found in Resources");
        }
        var admobGO = Instantiate(resGO);
        _adMobHelper = admobGO.GetComponent<IAdsNetworkHelper>();
        return _adMobHelper as IAdsNetworkHelper;
    }
#else
    IAdsNetworkHelper InitAdmobManager()
    {
        return null;
    }
#endif
}
