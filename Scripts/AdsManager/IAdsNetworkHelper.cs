using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Omnilatent.AdsMediation;

public interface IAdsNetworkHelper
{
    [System.Obsolete("Use ShowBanner(type, position, onAdLoaded) instead.")]
    void ShowBanner(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null);
    void ReloadCollapsibleBanner(AdPlacement.Type currentShowingBanner, BannerTransform bannerTransform);
    void ShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, AdsManager.InterstitialDelegate onAdLoaded = null);
    void HideBanner();
    void DestroyBanner();
    void ShowInterstitial(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdClosed);
    void RequestInterstitialNoShow(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null, bool showLoading = true);
    void RequestInterstitialRewardedNoShow(AdPlacement.Type placementType, RewardDelegate onAdLoaded = null);
    void ShowInterstitialRewarded(AdPlacement.Type placementType, RewardDelegate onAdClosed);
    void Reward(AdPlacement.Type placementType, RewardDelegate onFinish);
    void RequestRewardAd(AdPlacement.Type placementType, RewardDelegate onFinish);
    void RequestAppOpenAd(AdPlacement.Type placementType, RewardDelegate onAdLoaded = null);
    void ShowAppOpenAd(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdClosed = null);
}
