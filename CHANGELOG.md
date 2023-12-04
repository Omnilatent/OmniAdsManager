## 2.9.1
News:
- Add time between ad to Remote Config Ads Placement to allow customize time between show of each Ad Placements. If set to 0, it will default to TIME_BETWEEN_ADS.
- Make AdPlacement.Type serializable.

## 2.9.0
Changes:
- No longer automatically add listener to event onToggleLoading for SScene library. Symbol DISABLE_SSCENE is no longer used.

## 2.8.0
Changes:
- Moved Firebase Remote Config Helper class to Omni Firebase Manager package.

Require Omni Firebase Manager 1.3.0 from here on if you want to use Firebase Remote Config to control Ads.

## 2.7.3
News:
- Add callback on app open ad opened and closed.
- Add callback when remote config fetch complete.
- Update extra files for admob 8.x, separate admob callback handle to a separate class.
- Update extra file: Show ad on app resume has method to allow setting ad placement.

Changes:
- On toggle loading SS load: use addiction instead of overwriting.
- Scripting define symbol manager: fix rename Omnilatent.AdsManager namespace to Omnilatent.AdsMediation.
- Firebase remote config: change order of assigning Firebase remote config's initSuccess.

## 2.7.2
News:
- Add public method to get Firebase installation ID.
- App open ad: do not show ad if loading is active to prevent app open ad showing behind an interstitial ad.
- Add public property to get showingAppOpenAd.
- Automatically add scripting define symbol OMNILATENT_ADS_MANAGER to project. Update extra package: add component Unity main thread dispatcher to Ads manager prefab.

Fixes:
- Update extra files: update ShowAdOnAppResume to fix issue with app open ad overlapping with interstitial/rewarded ad.

## 2.7.1
News:
- Banner transform: add field adSizeData (object) for Admob custom ad size

Changes:
- Firebase Remote Config Helper: Change property InitSuccessValue to method HasInitialized(), make that method public to allow user to check if Firebase Remote Config has initialized. By default, it logs error if Firebase has not initialized.
- Make AdsManager.onToggleLoading public field.

## 2.7.0
News:
- Support showing App Open Ads of more ad networks other than Admob.
- Add loadingActive field and property to return whether an ad loading overlay is being displayed.
- Allow multiple simultaneous App open ad requests since Admob manager supports it.
- Add AppOpenAdObject.

Fixes:
- Check if app open ad is showing before showing another app open ad to prevent app open ad stacking on each other when using more than 1 ad networks.

Changes:
- Update extra files: ShowAdOnAppResume will check if Ads manager is loading before showing ad; Handle Ads Manager Message will check Ads Manager initialized before adding Admob callback.
- Update extra files: Improve get admob ID make it easier to add ad placement.

## 2.6.3
Changes:
- Show interstitial: check DoNotShowAds before showing, call onAdClosed if all ads failed to load.
- Ad Request Option: add show loading variable.

## 2.6.2
News:
- New function: RequestAndShowInterstitial. Check if enough time has passed between interstitial, request and show ad, and preload the next ad. Goal: simplify code where user wants to do all of those things.

Fixes:
- Set current showing banner to null on banner ad load fail so next show banner call can attempt to load banner again.

Changes:
- Move unity main thread dispatcher back to Omni Ads Manager because MAX & Iron Source Ad Wrapper depend on it. Requires Admob Manager 1.5.4

## 2.6.1
- Update handle ads manager message to initialize admob callback on ads manager init event.
- Add static field for inter ad load, load failed, closed events & reward ad closed event.

## 2.6.0
New:

- Now AdsManager can initialize automatically or manually.
- To init manually, in AdsManager component, uncheck 'Initialize Automatically'. Then call AdsManager.Instance.Initialize().

## 2.5.4
- Remember time last show reward ad. Add bool field to check if it's showing interstitial & reward ad.
- Add network security config file as a package. Reason: MAX Mediation for Facebook in Android API 31 require this file.
- Add reference to Omnilatent.AdjustUnity to asmdef.

## 2.5.3
- Add onAdLoaded field to BannerAdObject.

## 2.5.2
New:
- Added support for Omni IronSource Ad Wrapper (v1.0.1).
- Added support for Omni MAX Ad Wrapper.
- Separate functions to load IronSource and MAX's prefab to new class AdWrapperFinder.
- Class AdObject to standardize how ad networks handle ad loading, callbacks and ad placement.

## 2.5.1
New:
- Add time last show App open ad, always check if enough time has passed since last show open ad before showing it. This is work around for unwanted behaviour calling OnApplicationPause immediately after closing app open ad

Fixes:
- Fix set time last show interstitial: only update time last show interstitial when calling ShowInterstitial, remove incorrect set time last show interstitial
- Firebase remote config fetch data async: Check task completed before invoking callbacks to prevent callbacks getting called twice when an exception occurred during callback invocation.

## 2.5.0
Changes:
- Request app open ad with parameter callback of type RewardDelegate instead of InterstitialDelegate.
- Add placement type for generic app open ad for using in same ad type share cache.
Now require Omni Admob Ad Manager v1.5.0 and above.

## 2.4.0
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

## 2.3.0
- Show Open Ad: will check for placement config & remove ad before showing.
Reason: weird behavior that show Open Ad even though request Open Ad was blocked
- Update to Firebase 8.6.2: remove IsDeveloperMode deprecated Firebase Config
- Update extra package to have show add on app resume script
- Make Get Ads Network Helper public. update extra: HandleAdsManagerMessage: add callback from Admob manager to log event from interstitial ads
- Deprecate premade AdPlacement Type

## 2.2.0
New features:
- Added support for Admob's App Open Ad format.

Changes:
- Add LogError function to separate ShowError & LogError functions so that plugins can choose to log error and show error depend on use case.
- Change HandleOnAdLoadFailed's 2nd parameter to string type so plugins can log the error placementID without needing to know what AdPlacement.Type it's using.

## 2.1.0
Changes:
- Update to use Firebase API 7.1.0, add Firebase DefaultInstance, switch to use Firebase Installation

## 2.0.1
New features:
- Added Interstitial Rewarded. Update is required for all AdsHelper class.

## 2.0.0
Changes:
- Change enum AdsPlacementType to a struct in partial class AdsPlacement.Type so user can add their own PlacementType without having to change CustomMediation.cs

## 1.1.0 (2021/1/30)
New features:
- You can now generate AdsManager script to implement AdmobManager and FacebookAudienceNetworkHelper instead of relying on Scripting Define.
Access the new menu in Tools/Omnilatent/AdsManager.

Changes:
- AdmobManager has been moved out of AdsManager package to a separate package.
- Restructure folders.

## 1.0.3 (2021/1/15):
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

## 2020/11/16:
Update:
- Updated AdMobManager Initialize code
- AdMobManager prefab will be instantiated from Resources folder
- Unity Ads codes in AdsManager moved into UNITYADS define symbol
Fix:
- When all ads failed to load, currentAdsHelper will remain null and cause error in AdsManager.ShowInterstitial()
