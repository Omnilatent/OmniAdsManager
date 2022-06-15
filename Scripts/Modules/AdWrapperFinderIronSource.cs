using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation
{
    public static partial class AdWrapperFinder
    {
#if true //MODULE_MAKER
        const string ISManagerResourcesPath = "IronSourceHelper";
        public static IAdsNetworkHelper InitISHelper()
        {
            var resGO = Resources.Load<GameObject>(ISManagerResourcesPath);
            if (resGO == null)
            {
                Debug.Log($"{ISManagerResourcesPath} not found in Resources");
                return null;
            }
            var _ironSourceHelper = MonoBehaviour.Instantiate(resGO).GetComponent<IAdsNetworkHelper>();
            return _ironSourceHelper;
        }
#else
    IAdsNetworkHelper InitISHelper()
    {
        return null;
    }
#endif
    }
}