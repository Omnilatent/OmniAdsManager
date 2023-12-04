using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RewardResult
{
    public enum Type
    {
        LoadFailed = 0,
        Finished = 1,
        Canceled = 2,
        Loading = 3
    }

    public Type type;
    public string message;

    public RewardResult(Type type, string message = "")
    {
        this.type = type;
        this.message = message;
    }

    public bool IsFinished { get { return type == Type.Finished; } }
}

public delegate void RewardDelegate(RewardResult result);

namespace Omnilatent.AdsMediation
{
    public class RewardWrapper
    {
        private AdsManager _adsManager;
        private IAdsNetworkHelper currentAdsHelper;
        float timeLastShowRewardAd = -9999f; //the value of time when last reward ad was shown

        public RewardWrapper(AdsManager manager)
        {
            _adsManager = manager;
        }

        public void Reward(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            AdsManager.ToggleLoading(true);
            _adsManager.StartCoroutine(CoReward(onFinish, placementType));
        }

        IEnumerator CoReward(RewardDelegate onFinish, AdPlacement.Type placementType)
        {
            RewardResult rewardResult = new RewardResult();
            string errorMsg = string.Empty;

            if (_adsManager.IsAdsHiddenRemoteConfig(placementType))
            {
                rewardResult.type = RewardResult.Type.LoadFailed;
                rewardResult.message = "Disabled";
            }
            else if (AdsManager.HasNoInternet())
            {
                rewardResult.type = RewardResult.Type.LoadFailed;
                rewardResult.message = "No Internet connection";
            }
            else
            {
                timeLastShowRewardAd = _adsManager.TimeInGame;
                _adsManager.ShowingRewardAd = true;
                WaitForSecondsRealtime checkInterval = new WaitForSecondsRealtime(0.3f);

                List<CustomMediation.AD_NETWORK> adPriority = _adsManager.GetAdsNetworkPriority(placementType);

                for (int i = 0; i < adPriority.Count; i++)
                {
                    bool checkAdNetworkDone = false;
                    IAdsNetworkHelper adsHelper = _adsManager.GetAdsNetworkHelper(adPriority[i]);
                    if (adsHelper == null) continue;
                    adsHelper.Reward(placementType, (result) =>
                    {
                        checkAdNetworkDone = true;
                        rewardResult = result;
                    });
                    while (!checkAdNetworkDone)
                    {
                        yield return checkInterval;
                    }

                    if (rewardResult.type == RewardResult.Type.Finished)
                    {
                        currentAdsHelper = adsHelper;
                        break;
                    }

                    if (rewardResult.type == RewardResult.Type.Canceled)
                    {
                        break;
                    } //if a reward ads was shown and user skipped it, stop looking for more ads
                }
            }
            
            onFinish(rewardResult);
            AdsManager.OnRewardAdClosedEvent?.Invoke(placementType, rewardResult);
            AdsManager.ToggleLoading(false);
            _adsManager.ShowingRewardAd = false;
            if (rewardResult.type == RewardResult.Type.LoadFailed || rewardResult.type == RewardResult.Type.Loading)
            {
                if (rewardResult.type == RewardResult.Type.LoadFailed)
                {
                    AdsManager.LogError(rewardResult.message, placementType.ToString());
                }

                AdsManager.ShowError(rewardResult, placementType.ToString());
            }
        }
    }
}