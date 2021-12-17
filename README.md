## Dependencies:
- Firebase Remote Config SDK 7.2.0
- Firebase Installation SDK 7.2.0
- Omni Firebase Manager
- Unity LitJson https://github.com/Omnilatent/UnityLitJson

## Setup:
- Import Extra files: Tools/Omnilatent/Ads Manager/Import Extra Package
- Add AdsManager prefab into the first scene loaded.
- Set Common Ads placement ID in CustomMediation.cs > AdPlacementType.
- Set Admob Placement ID in AdMobConst.

- Audience Network: Facebook's Audience Network's script does not have .asmdef file (Scripting Assembly Definition). To fix this:
Tools/Omnilatent/Ads Manager/Import AudienceNetwork Assembly Fix
OR
Create a .asmdef file named "AudienceNetwork.asmdef" in AudienceNetwork's folder.

### Setup Remote Config:

To setup remote config of ad placement, first check the remote config key (adsPlacementConfigKey field) in AdsManager's RemoteConfigAdsPlacement component, by default it is "ads_placement_config_2".

Next, go to Firebase's Remote Config, create a parameter with key the same as adsPlacementConfigKey (default: ads_placement_config_2), set the value to a JSON string with this format:

Example:
```
{
  "0": [
    {
      "placementID": 6,
      "name": "Home_Gameplay_Banner",
      "show": true,
      "priority": [
        3,
		2,
		1
      ]
    },
    {
      "placementID": 7,
      "name": "Gameplay_Interstitial",
      "show": true,
      "priority": [
        3
      ]
    }
}
```

Format of a RemoteConfigAdsPlacementData in JSON:
```
{
      "placementID": [placement ID],
      "name": [name for readability],
      "show": [if true the ad will be shown, if false the ad won't be loaded and won't be shown],
      "priority": [
        [an array of ads network ID used for this placement. 
		Google Admob: 3,
		Facebook: 2,
		Unity Ads: 1]
      ]
}
```

## Usage:
- To switch between using real Ads ID and test Ads ID:

    Tools/Omnilatent/AdsManager/Debug Ad
    
        OR
        
    Go to Project Setting/Scripting Define Symbol, and add/remove "DEBUG_ADS"

- To Request and Show Interstitial ads after requesting successful:
```
    AdsManager.instance.RequestInterstitialNoShow(AdPlacementType.Interstitial, (bool success) =>
        {
            if (success) AdsManager.instance.ShowInterstitial(AdPlacementType.Interstitial);
            //Load next scene here
        });
```

- To show Rewarded Ad:
```
    AdsManager.Reward((bool rewardSuccess) => {
        //Give reward item
    }, AdPlacement.Reward);
```

- To show Banner Ad:
    `AdsManager.Instance.ShowBanner(AdPlacement.Banner, new Omnilatent.AdsMediation.BannerTransform(AdPosition.Bottom));`

- To add an Ads Network to AdsManager:
 + Add OmniAds Ads Helper package (Admob / FAN / Unity).
 
     https://github.com/Omnilatent/OmniAdmobAdsManager
     
     https://github.com/Omnilatent/OmniUnityAdsManager
     
     https://github.com/Omnilatent/OmniAudienceNetworkAdsManager

Files included in Extra package:
```
Extra
|- AdsManagerExtra
|-  |- AdsPlacementID
|-  |   |- AdditionalPlacementID.cs
|-  |   |- CustomMediationAdmobID.cs
|-  |   |- CustomMediationAudienceNetworkID.cs
|-  |   |- CustomMediationUnityID.cs
|-  |   |- Omnilatent.AdsManager Reference.asmref
|-  |- HandleAdsManagerMessage.cs
|-  |- Resources
|-  |-  |- AdsManager.prefab
|- MessagePopup
|-  |- MessagePopup.cs
|-  |- Resources
|-  |-  |- MessagePopupCanvas.prefab
```
