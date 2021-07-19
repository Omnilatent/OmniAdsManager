Setup:
- Import Extra files: Tools/Omnilatent/Ads Manager/Import Extra Package
- Add AdsManager prefab into the first scene loaded.
- Set Common Ads placement ID in CustomMediation.cs > AdPlacementType.
- Set Admob Placement ID in AdMobConst.

- Audience Network: Facebook's Audience Network's script does not have .asmdef file (Scripting Assembly Definition). To fix this:
Tools/Omnilatent/Ads Manager/Import AudienceNetwork Assembly Fix
    OR
Create a .asmdef file named "AudienceNetwork.asmdef" in AudienceNetwork's folder.

Usage:
- To switch between using real Ads ID and test Ads ID:
    Tools/Omnilatent/AdsManager/Debug Ad
        OR
    Go to Project Setting/Scripting Define Symbol, and add/remove "DEBUG_ADS"

- To Request and Show Interstitial ads after requesting successful:
    AdsManager.instance.RequestInterstitialNoShow(AdPlacementType.Interstitial, (bool success) =>
        {
            if (success) AdsManager.instance.ShowInterstitial(AdPlacementType.Interstitial);
            //Load next scene here
        });

- To show Rewarded Ad:
    AdsManager.Reward((bool rewardSuccess) => {
        //Give reward item
    }, AdPlacement.Reward);

- To show Banner Ad:
    AdsManager.Instance.ShowBanner(AdPlacement.Banner, new Omnilatent.AdsMediation.BannerTransform(AdPosition.Bottom));

- To add an Ads Network to AdsManager:
 + Add OmniAds Ads Helper package (Admob / FAN / Unity).
     https://github.com/Omnilatent/OmniAdmobAdsManager
     https://github.com/Omnilatent/OmniUnityAdsManager
     https://github.com/Omnilatent/OmniAudienceNetworkAdsManager

Files included in Extra package:
Extra
|- AdsPlacementID
|   |- AdditionalPlacementID.cs
|   |- CustomMediationAdmobID.cs
|   |- CustomMediationAudienceNetworkID.cs
|   |- CustomMediationUnityID.cs
|   |- Omnilatent.AdsManager Reference.asmref
|- HandleAdsManagerMessage.cs
|- MessagePopup
|-  |- MessagePopup.cs
|-  |- Resources
|-  |-  |- MessagePopupCanvas.Prefab
|- Resources
|-  |- AdsManager.prefab