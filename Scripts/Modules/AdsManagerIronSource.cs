using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public partial class AdsManager : MonoBehaviour
{
#if true //MODULE_MAKER
    const string ISManagerResourcesPath = "IronSourceHelper";
    IAdsNetworkHelper InitISHelper()
    {
        var resGO = Resources.Load<GameObject>(ISManagerResourcesPath);
        if (resGO == null)
        {
            Debug.Log($"{ISManagerResourcesPath} not found in Resources");
            return null;
        }
        _ironSourceHelper = Instantiate(resGO).GetComponent<IAdsNetworkHelper>();
        return _ironSourceHelper;
    }
#else
    IAdsNetworkHelper InitISHelper()
    {
        return null;
    }
#endif
}
