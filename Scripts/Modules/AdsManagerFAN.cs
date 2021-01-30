using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public partial class AdsManager : MonoBehaviour
{
#if false //MODULE_MAKER
    FacebookAudienceNetworkHelper InitFANHelper()
    {
        _FANHelper = gameObject.GetComponent<FacebookAudienceNetworkHelper>() as IAdsNetworkHelper;
        if (_FANHelper == null)
        {
            throw new System.NullReferenceException($"Component {typeof(FacebookAudienceNetworkHelper).Name} does not exist");
        }
        return _FANHelper as FacebookAudienceNetworkHelper;
    }
#else
    IAdsNetworkHelper InitFANHelper()
    {
        return null;
    }
#endif
}
