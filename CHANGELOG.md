2.2.0
New features:
- Added support for Admob's App Open Ad format.

Changes:
- Add LogError function to separate ShowError & LogError functions so that plugins can choose to log error and show error depend on use case.
- Change HandleOnAdLoadFailed's 2nd parameter to string type so plugins can log the error placementID without needing to know what AdPlacement.Type it's using.

===
2.1.0
Changes:
- Update to use Firebase API 7.1.0, add Firebase DefaultInstance, switch to use Firebase Installation

===
2.0.1
New features:
- Added Interstitial Rewarded. Update is required for all AdsHelper class.

===
2.0.0
Changes:
- Change enum AdsPlacementType to a struct in partial class AdsPlacement.Type so user can add their own PlacementType without having to change CustomMediation.cs

===
1.1.0 (2021/1/30)
New features:
- You can now generate AdsManager script to implement AdmobManager and FacebookAudienceNetworkHelper instead of relying on Scripting Define.
Access the new menu in Tools/Omnilatent/AdsManager.

Changes:
- AdmobManager has been moved out of AdsManager package to a separate package.
- Restructure folders.

===
1.0.3 (2021/1/15):
New features:
- Ad Placement remote config can now set network priority order for each placement. 
- Example remote config json for new placement setting:
    {
      "placementID": 1,
      "name": "Banner",
      "show": true,
      "priority": [
        1,
        2,
        3
      ]
    },
- Added On Ads Closed callback for ShowInterstitial(). You can now preload next ads after previous ads has been closed by using this callback
Fix:
- Unity Ads' ShowBanner correctly show only if it received a successful callback

===
2020/11/16:
Update:
- Updated AdMobManager Initialize code
- AdMobManager prefab will be instantiated from Resources folder
- Unity Ads codes in AdsManager moved into UNITYADS define symbol
Fix:
- When all ads failed to load, currentAdsHelper will remain null and cause error in AdsManager.ShowInterstitial()
