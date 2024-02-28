using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public partial class AdsManager : MonoBehaviour
{
#if true //MODULE_MAKER
    const string FANManagerResourcesPath = "FacebookAudienceNetworkHelper";
    IAdsNetworkHelper InitFANHelper()
    {
        var resGO = Resources.Load<GameObject>(FANManagerResourcesPath);
        if (resGO == null)
        {
            //.Log($"{FANManagerResourcesPath} not found in Resources");
            return null;
        }
        _FANHelper = Instantiate(resGO).GetComponent<IAdsNetworkHelper>();
        return _FANHelper;
    }
#else
    IAdsNetworkHelper InitFANHelper()
    {
        return null;
    }
#endif
}
