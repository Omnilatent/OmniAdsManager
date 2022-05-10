===
2.5.0
Changes:
- Request app open ad with parameter callback of type RewardDelegate instead of InterstitialDelegate.
- Add placement type for generic app open ad for using in same ad type share cache.
Now require Omni Admob Ad Manager v1.4.1 and above.

===
2.4.0
Changes:
- Move Unity Main Thread Dispatcher from Omni Ads Manager to Omni Admob*
- Update reward method: use param reward result instead of bool, change order of params to match other ad method.
- Allow disable SS by using scripting define symbol. custom loading function can be assigned through "onToggleLoading" in AdsManager.
- Default reward ad placement made not obsolete because cache admob use this for sharing ad cache of same type
- Show message when detect reward ad is loading. only log error if reward ad load failed
- Show error will pass reward result object instead of string

*Due to this change, Ads Manager will require Omni Admob Ad Manager v1.3.1 and above.

Minor changes:
- Update to deprecate premade AdPlacement Type: make obsolete warning instead of error
- Firebase remote fetch try catch: add log exception message
- Firebase remote config helper: if fetch fail with error code = 1, log event, otherwise log exception to Crashlytic
- Reward result type: add type loading

===
2.3.0
- Show Open Ad: will check for placement config & remove ad before showing.
Reason: weird behavior that show Open Ad even though request Open Ad was blocked
- Update to Firebase 8.6.2: remove IsDeveloperMode deprecated Firebase Config
- Update extra package to have show add on app resume script
- Make Get Ads Network Helper public. update extra: HandleAdsManagerMessage: add callback from Admob manager to log event from interstitial ads
- Deprecate premade AdPlacement Type

===
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
