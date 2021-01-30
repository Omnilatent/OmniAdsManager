using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAdsNetworkHelper
{
    void ShowBanner(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null);
    void HideBanner();
    void ShowInterstitial(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdClosed);
    void RequestInterstitialNoShow(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null, bool showLoading = true);
    void Reward(AdPlacement.Type placementType, RewardDelegate onFinish);
}
