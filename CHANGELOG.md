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
