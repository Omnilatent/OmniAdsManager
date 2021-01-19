Setup:
- Add AdsManager prefab into the first scene loaded.
- Set Common Ads placement ID in CustomMediation.cs > AdPlacementType.
- Set Admob Placement ID in AdMobConst.
- Set AdNetwork used in Resources/AdNetworkSetting

Usage:
- To Request and Show Interstitial ads after requesting successful:

    AdsManager.instance.RequestInterstitialNoShow(AdPlacementType.Interstitial, (bool success) =>
        {
            if (success) AdsManager.instance.ShowInterstitial(AdPlacementType.Interstitial);
            //Load next scene here
        });