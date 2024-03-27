using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation
{
    public delegate void BannerLoadDelegate(bool isSuccess, BannerAdObject loadedAdObject);

    public class BannerManager
    {
        public CustomMediation.AD_NETWORK? AdNetwork;

        // [Obsolete]
        // AdPlacement.Type? currentShowingBanner = null;

        [Obsolete]
        public AdPlacement.Type? CurrentShowingBanner
        {
            get
            {
                foreach (var cachedBanner in _cachedBanners)
                {
                    if (cachedBanner.Value.State == AdObjectState.Showing)
                    {
                        return cachedBanner.Value.AdPlacementType;
                    }
                }

                return null;
            }
        }

        // bool IsShowingBanner { get => currentShowingBanner != null; }
        [Obsolete]
        BannerTransform currentShowingBannerTransform;

        [Obsolete]
        public BannerTransform CurrentShowingBannerTransform
        {
            get
            {
                foreach (var cachedBanner in _cachedBanners)
                {
                    if (cachedBanner.Value.State == AdObjectState.Showing)
                    {
                        return cachedBanner.Value.TransformData;
                    }
                }

                return null;
            }
        }

        private Dictionary<AdPlacement.Type, BannerAdObject> _cachedBanners = new Dictionary<AdPlacement.Type, BannerAdObject>();
        public Dictionary<AdPlacement.Type, BannerAdObject> CachedBanners => _cachedBanners;

        private AdsManager _adsManager;

        public BannerManager(AdsManager adsManager)
        {
            _adsManager = adsManager;
        }

        public BannerAdObject GetCachedBannerObject(AdPlacement.Type placementType, CustomMediation.AD_NETWORK adNetwork = CustomMediation.AD_NETWORK.None)
        {
            if (_cachedBanners.TryGetValue(placementType, out var bannerAdObject))
            {
                // if ad network is not specified, get any matching banner
                if (adNetwork == CustomMediation.AD_NETWORK.None || bannerAdObject.AdNetwork == adNetwork)
                {
                    return bannerAdObject;
                }
            }

            return null;
        }

        public void SetCachedBannerObject(AdPlacement.Type placementType, BannerAdObject bannerAdObject)
        {
            if (!_cachedBanners.ContainsKey(placementType))
            {
                _cachedBanners.Add(placementType, bannerAdObject);
            }
            else
            {
                _cachedBanners[placementType] = bannerAdObject;
            }
        }

        public void RequestBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, BannerLoadDelegate onAdLoaded = null)
        {
            BannerAdObject cachedBanner;
            if (_cachedBanners.TryGetValue(placementType, out cachedBanner))
            {
                if (cachedBanner.CanShow || cachedBanner.State == AdObjectState.Showing)
                {
                    onAdLoaded?.Invoke(true, cachedBanner);
                    return;
                }
                else if (cachedBanner.State == AdObjectState.Loading)
                {
                    Debug.Log($"Banner '{placementType}' is still loading.");
                    onAdLoaded?.Invoke(false, cachedBanner);
                    return;
                }
            }

            if (_adsManager.DoNotShowAds(placementType))
            {
                onAdLoaded?.Invoke(false, null);
                return;
            }

            _adsManager.StartCoroutine(CoRequestBanner(placementType, bannerTransform, onAdLoaded));
        }

        IEnumerator CoRequestBanner(AdPlacement.Type placementType, BannerTransform bannerTransform = null,
            BannerLoadDelegate onAdLoaded = null)
        {
            if (_cachedBanners.TryGetValue(placementType, out var cachedBanner) && bannerTransform == null)
            {
                bannerTransform = cachedBanner.TransformData;
            }

            if (bannerTransform == null)
            {
                bannerTransform = BannerTransform.defaultValue;
            }

            bool isSuccess = false;
            WaitForSecondsRealtime checkInterval = new WaitForSecondsRealtime(0.3f);

            var adPriority = _adsManager.GetAdsNetworkPriority(placementType);

            for (int i = 0; i < adPriority.Count; i++)
            {
                bool checkAdNetworkDone = false;
                var adsHelper = _adsManager.GetAdsNetworkHelper(adPriority[i]);
                if (adsHelper == null) continue;
                adsHelper.RequestBanner(placementType, bannerTransform, ref cachedBanner,
                    (success, newBannerAdObject) =>
                    {
                        checkAdNetworkDone = true;
                        isSuccess = success;
                        if (!success)
                        {
                            cachedBanner.State = AdObjectState.LoadFailed;
                            // _cachedBanners.Remove(placementType);
                        }
                        else
                        {
                            cachedBanner.State = AdObjectState.Ready;
                        }

                        onAdLoaded?.Invoke(success, cachedBanner);
                    });

                SetCachedBannerObject(placementType, cachedBanner);
                cachedBanner.TransformData = bannerTransform;
                cachedBanner.State = AdObjectState.Loading;
                cachedBanner.AdNetwork = adPriority[i];

                while (!checkAdNetworkDone)
                {
                    yield return checkInterval;
                }

                if (isSuccess)
                {
                    //showingBanners.Add(CurrentAdNetwork);
                    //isShowingBanner = true;
                    AdNetwork = adPriority[i];
                    // currentShowingBanner = placementType;
                    // currentShowingBannerTransform = bannerTransform;
                    break;
                }
            }
        }

        public void ShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, AdsManager.BoolDelegate onAdLoaded = null)
        {
            BannerAdObject cachedBanner;
            if (_cachedBanners.TryGetValue(placementType, out cachedBanner))
            {
                if (cachedBanner.State == AdObjectState.Showing)
                {
                    onAdLoaded?.Invoke(true);
                    return;
                }
            }

            if (_adsManager.DoNotShowAds(placementType))
            {
                onAdLoaded?.Invoke(false);
                return;
            }

            RequestBanner(placementType, bannerTransform, (success, adObject) =>
            {
                if (success)
                {
                    var adsHelper = _adsManager.GetAdsNetworkHelper(adObject.AdNetwork);
                    adsHelper.ShowBanner(placementType, bannerTransform, ref adObject,
                        (success, newBannerAdObject) =>
                        {
                            if (!success)
                            {
                                adObject.State = AdObjectState.ShowFailed;
                                // _cachedBanners.Remove(placementType);
                                // currentShowingBanner = null;
                                // currentShowingBannerTransform = null;
                            }
                            else
                            {
                                adObject.State = AdObjectState.Showing;
                            }
                            onAdLoaded?.Invoke(success);
                        });
                }
                else
                {
                    onAdLoaded?.Invoke(false);
                }
            });

            /*BannerAdObject cachedBanner;
            if (_cachedBanners.TryGetValue(placementType, out cachedBanner))
            {
                if (cachedBanner.State == AdObjectState.Showing)
                {
                    onAdLoaded?.Invoke(true);
                    Debug.Log("AdsManager: A banner is already being shown");
                    return;
                }
            }

            if (_adsManager.DoNotShowAds(placementType))
            {
                onAdLoaded?.Invoke(false);
                return;
            }

            _adsManager.StartCoroutine(CoShowBanner(placementType, bannerTransform, onAdLoaded));*/
        }

        IEnumerator CoShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform = null,
            AdsManager.BoolDelegate onAdLoaded = null)
        {
            if (_cachedBanners.TryGetValue(placementType, out var cachedBanner) && bannerTransform == null)
            {
                bannerTransform = cachedBanner.TransformData;
            }

            if (bannerTransform == null)
            {
                bannerTransform = BannerTransform.defaultValue;
            }

            bool isSuccess = false;
            WaitForSecondsRealtime checkInterval = new WaitForSecondsRealtime(0.3f);

            var adPriority = _adsManager.GetAdsNetworkPriority(placementType);

            for (int i = 0; i < adPriority.Count; i++)
            {
                bool checkAdNetworkDone = false;
                var adsHelper = _adsManager.GetAdsNetworkHelper(adPriority[i]);
                if (adsHelper == null) continue;

                adsHelper.ShowBanner(placementType, bannerTransform, ref cachedBanner,
                    (success, newBannerAdObject) =>
                    {
                        checkAdNetworkDone = true;
                        isSuccess = success;
                        onAdLoaded?.Invoke(success);
                        if (!success)
                        {
                            _cachedBanners.Remove(placementType);
                            // currentShowingBanner = null;
                            // currentShowingBannerTransform = null;
                        }
                        else if (GetCachedBannerObject(placementType, adPriority[i]) == null ||
                                 GetCachedBannerObject(placementType, adPriority[i]) != newBannerAdObject)
                        {
                            Debug.Log(
                                $"{adPriority[i]}'s ads helper did not assign new banner object, AdsManager will assign new banner object.");
                            SetCachedBannerObject(placementType, newBannerAdObject);
                        }
                    });
                while (!checkAdNetworkDone)
                {
                    yield return checkInterval;
                }

                if (isSuccess)
                {
                    //showingBanners.Add(CurrentAdNetwork);
                    //isShowingBanner = true;
                    AdNetwork = adPriority[i];
                    // currentShowingBanner = placementType;
                    // currentShowingBannerTransform = bannerTransform;
                    break;
                }
            }
        }

        /// <summary>
        /// Hide all banner
        /// </summary>
        public void HideBanner()
        {
            if (!AdsManager.Initialized) return;

            foreach (var cachedBanner in _cachedBanners)
            {
                var adHelper = _adsManager.GetAdsNetworkHelper(cachedBanner.Value.AdNetwork);
                HideBanner(adHelper, cachedBanner.Value.AdPlacementType);
            }

            /*foreach (var item in _adsManager.adsNetworkHelpers)
            {
                HideBanner(item);
            }*/

            //showingBanners.Clear();
            //isShowingBanner = false;
            // currentShowingBanner = null;
            // currentShowingBannerTransform = null;
        }

        public void HideBanner(AdPlacement.Type placement)
        {
            if (!AdsManager.Initialized) return;
            foreach (var item in _adsManager.adsNetworkHelpers)
            {
                //Debug.Log("hiding banner " + item.ToString());
                HideBanner(item, placement);
            }

            //showingBanners.Clear();
            //isShowingBanner = false;
            // currentShowingBanner = null;
            // currentShowingBannerTransform = null;
        }

        /*void HideBanner(IAdsNetworkHelper adNetwork)
        {
            adNetwork.HideBanner();
        }*/

        void HideBanner(IAdsNetworkHelper adNetwork, AdPlacement.Type placementType)
        {
            adNetwork.HideBanner(placementType);
            
            var cachedBanner = GetCachedBannerObject(placementType);
            if (cachedBanner != null && !AdObject.NeedReload(cachedBanner.State))
            {
                cachedBanner.State = AdObjectState.Closed;
            }
        }

        public void DestroyBanner()
        {
            if (!AdsManager.Initialized) return;

            foreach (var cachedBanner in _cachedBanners)
            {
                var adHelper = _adsManager.GetAdsNetworkHelper(cachedBanner.Value.AdNetwork);
                DestroyBanner(adHelper, cachedBanner.Value.AdPlacementType);
            }

            /*foreach (var item in _adsManager.adsNetworkHelpers)
            {
                DestroyBanner(item);
            }*/

            // currentShowingBanner = null;
            // currentShowingBannerTransform = null;
        }

        public void DestroyBanner(AdPlacement.Type placementType)
        {
            if (!AdsManager.Initialized) return;
            foreach (var item in _adsManager.adsNetworkHelpers)
            {
                DestroyBanner(item, placementType);
            }

            // currentShowingBanner = null;
            // currentShowingBannerTransform = null;
        }

        /*void DestroyBanner(IAdsNetworkHelper adNetwork)
        {
            adNetwork.DestroyBanner();
        }*/

        void DestroyBanner(IAdsNetworkHelper adNetwork, AdPlacement.Type placementType)
        {
            adNetwork.DestroyBanner(placementType);
            var cachedBanner = GetCachedBannerObject(placementType);
            if (cachedBanner != null)
            {
                cachedBanner.State = AdObjectState.None;
            }
        }
    }
}