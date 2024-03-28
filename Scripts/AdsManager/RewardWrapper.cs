using System;
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
        Loading = 3,
        Loaded = 4
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

        public float TimeLastShowRewardAd => timeLastShowRewardAd;

        public void Reward(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            AdsManager.ToggleLoading(true);
            // _adsManager.StartCoroutine(CoReward(onFinish, placementType));
            RequestAndShowReward(placementType, onFinish);
        }

        [Obsolete("Use RequestAndShowReward() instead")]
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

        void RequestAndShowReward(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            RequestRewardAd(placementType, result =>
            {
                if (result.type == RewardResult.Type.Loaded)
                {
                    ShowRewardAd(placementType, onFinish, true);
                }
                else
                {
                    onFinish?.Invoke(result);
                }
            }, true);
        }

        public void RequestRewardAd(AdPlacement.Type placementType, RewardDelegate onFinish, bool showLoading)
        {
            if (showLoading)
            {
                AdsManager.ToggleLoading(true);
            }

            _adsManager.StartCoroutine(CoRequestRewardAd(placementType, onFinish, showLoading));
        }

        IEnumerator CoRequestRewardAd(AdPlacement.Type placementType, RewardDelegate onFinish, bool showLoading)
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
                WaitForSecondsRealtime checkInterval = new WaitForSecondsRealtime(0.3f);

                List<CustomMediation.AD_NETWORK> adPriority = _adsManager.GetAdsNetworkPriority(placementType);

                for (int i = 0; i < adPriority.Count; i++)
                {
                    bool checkAdNetworkDone = false;
                    IAdsNetworkHelper adsHelper = _adsManager.GetAdsNetworkHelper(adPriority[i]);
                    if (adsHelper == null) continue;
                    adsHelper.RequestRewardAd(placementType, (result) =>
                    {
                        checkAdNetworkDone = true;
                        rewardResult = result;
                    });
                    while (!checkAdNetworkDone)
                    {
                        yield return checkInterval;
                    }

                    if (rewardResult.type == RewardResult.Type.Loaded)
                    {
                        currentAdsHelper = adsHelper;
                        break;
                    }

                    if (rewardResult.type == RewardResult.Type.LoadFailed)
                    {
                        break;
                    }
                }
            }

            onFinish(rewardResult);
            AdsManager.OnRewardAdLoadedEvent?.Invoke(placementType, rewardResult);
            if (showLoading)
                AdsManager.ToggleLoading(false);
            if (rewardResult.type == RewardResult.Type.LoadFailed)
            {
                AdsManager.LogError(rewardResult.message, placementType.ToString());
                if (showLoading)
                {
                    AdsManager.ShowError(rewardResult, placementType.ToString());
                }
            }
        }

        public void ShowRewardAd(AdPlacement.Type placementType, RewardDelegate onFinish, bool showLoading)
        {
            if (showLoading)
            {
                AdsManager.ToggleLoading(true);
            }
            _adsManager.StartCoroutine(CoShowReward(placementType, onFinish));
        }

        IEnumerator CoShowReward(AdPlacement.Type placementType, RewardDelegate onFinish)
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
                    AdsManager.OnRewardAdOpeningEvent?.Invoke(placementType);
                    adsHelper.ShowRewardAd(placementType, (result) =>
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