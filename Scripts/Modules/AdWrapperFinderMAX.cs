using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation
{
    public static partial class AdWrapperFinder
    {
#if true //MODULE_MAKER
        const string MAXAdHelperResourcesPath = "OmniRes/MAXAdsWrapper";
        public static IAdsNetworkHelper InitMAXHelper()
        {
            var resGO = Resources.Load<GameObject>(MAXAdHelperResourcesPath);
            if (resGO == null)
            {
                //.Log($"{MAXAdHelperResourcesPath} not found in Resources");
                return null;
            }
            var adHelper = MonoBehaviour.Instantiate(resGO).GetComponent<IAdsNetworkHelper>();
            return adHelper;
        }
#else
    IAdsNetworkHelper InitISHelper()
    {
        return null;
    }
#endif
    }
}