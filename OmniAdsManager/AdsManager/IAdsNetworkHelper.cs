using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAdsNetworkHelper
{
    void ShowBanner(AdPlacementType placementType, AdsManager.InterstitialDelegate onAdLoaded = null);
    void HideBanner();
    void ShowInterstitial(AdPlacementType placementType, AdsManager.InterstitialDelegate onAdClosed);
    void RequestInterstitialNoShow(AdPlacementType placementType, AdsManager.InterstitialDelegate onAdLoaded = null, bool showLoading = true);
    void Reward(AdPlacementType placementType, RewardDelegate onFinish);
}
