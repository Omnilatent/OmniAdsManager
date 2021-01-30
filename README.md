Setup:
- Add AdsManager prefab into the first scene loaded.
- Set Common Ads placement ID in CustomMediation.cs > AdPlacementType.
- Set Admob Placement ID in AdMobConst.
- Set AdNetwork used in Resources/AdNetworkSetting

- Audience Network: Facebook's Audience Network's script does not have .asmdef file (Scripting Assembly Definition). You'll have to manually create a .asmdef file named "AudienceNetwork" in AudienceNetwork's folder.

Usage:
- To Request and Show Interstitial ads after requesting successful:

    AdsManager.instance.RequestInterstitialNoShow(AdPlacementType.Interstitial, (bool success) =>
        {
            if (success) AdsManager.instance.ShowInterstitial(AdPlacementType.Interstitial);
            //Load next scene here
        });

- To add an Ads Network to AdsManager:
 + Add OmniAds Ads Helper package (Admob / FAN / Unity).
 + Run setup by click on menu: Tools/Omnilatent/AdsManager/Toggle {Ads network name}.

- To remove Ads Network AdsManager:
 + Run setup by click on menu: Tools/Omnilatent/AdsManager/Toggle {Ads network name} again.