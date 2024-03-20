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

        AdPlacement.Type? currentShowingBanner = null;
        bool IsShowingBanner { get => currentShowingBanner != null; }
        BannerTransform currentShowingBannerTransform;
        public BannerTransform CurrentShowingBannerTransform { get => currentShowingBannerTransform; }

        private Dictionary<AdPlacement.Type, BannerAdObject> _cachedBanners = new Dictionary<AdPlacement.Type, BannerAdObject>();

        private AdsManager _adsManager;

        public BannerManager(AdsManager adsManager)
        {
            _adsManager = adsManager;
        }

        public BannerAdObject GetCachedBannerObject(AdPlacement.Type placementType)
        {
            if (_cachedBanners.TryGetValue(placementType, out var bannerAdObject))
            {
                return bannerAdObject;
            }

            return null;
        }

        public void ShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, AdsManager.BoolDelegate onAdLoaded = null)
        {
            BannerAdObject cachedBanner;
            if (_cachedBanners.TryGetValue(placementType, out cachedBanner))
            {
                if (cachedBanner.State == AdObjectState.Showing)
                {
                    Debug.Log("AdsManager: A banner is already being shown");
                    return;
                }
            }

            if (_adsManager.DoNotShowAds(placementType))
            {
                onAdLoaded?.Invoke(false);
                return;
            }

            _adsManager.StartCoroutine(CoShowBanner(placementType, bannerTransform, onAdLoaded));
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

                adsHelper.ShowBanner(placementType, bannerTransform,
                    (success, newBannerAdObject) =>
                    {
                        checkAdNetworkDone = true;
                        isSuccess = success;
                        onAdLoaded?.Invoke(success);
                        if (!success)
                        {
                            _cachedBanners.Remove(placementType);
                            currentShowingBanner = null;
                            currentShowingBannerTransform = null;
                        }
                        else
                        {
                            // register a banner in cached banner dict
                            if (cachedBanner == null)
                            {
                                _cachedBanners.Add(placementType, newBannerAdObject);
                            }
                            else
                            {
                                _cachedBanners[placementType] = newBannerAdObject;
                            }
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
                    currentShowingBanner = placementType;
                    currentShowingBannerTransform = bannerTransform;
                    break;
                }
            }
        }

        /// <summary>
        /// Hide all banner
        /// </summary>
        [Obsolete("Use HideBanner(AdPlacement.Type placement) instead")]
        public void HideBanner()
        {
            if (!AdsManager.Initialized) return;
            foreach (var item in _adsManager.adsNetworkHelpers)
            {
                //Debug.Log("hiding banner " + item.ToString());
                HideBanner(item);
            }

            //showingBanners.Clear();
            //isShowingBanner = false;
            currentShowingBanner = null;
            currentShowingBannerTransform = null;
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
            currentShowingBanner = null;
            currentShowingBannerTransform = null;
        }
        
        void HideBanner(IAdsNetworkHelper adNetwork)
        {
            adNetwork.HideBanner();
        }
        
        void HideBanner(IAdsNetworkHelper adNetwork, AdPlacement.Type placement)
        {
            adNetwork.HideBanner(placement);
        }

        public void DestroyBanner()
        {
            if (!AdsManager.Initialized) return;
            foreach (var item in _adsManager.adsNetworkHelpers)
            {
                DestroyBanner(item);
            }

            currentShowingBanner = null;
            currentShowingBannerTransform = null;
        }
        
        public void DestroyBanner(AdPlacement.Type placementType)
        {
            if (!AdsManager.Initialized) return;
            foreach (var item in _adsManager.adsNetworkHelpers)
            {
                DestroyBanner(item, placementType);
            }

            currentShowingBanner = null;
            currentShowingBannerTransform = null;
        }

        void DestroyBanner(IAdsNetworkHelper adNetwork)
        {
            adNetwork.DestroyBanner();
        }
        
        void DestroyBanner(IAdsNetworkHelper adNetwork, AdPlacement.Type placementType)
        {
            adNetwork.DestroyBanner(placementType);
        }
    }
}