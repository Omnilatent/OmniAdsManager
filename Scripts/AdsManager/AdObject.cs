using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation
{
    /* Class to standardize ad state of different ad networks
     * Currently implemented by IronSource Ad Wrapper */

    public enum AdObjectState
    {
        None = 0, Loading, Ready, Showing, Shown, Closed, LoadFailed, ShowFailed
    }

    public class AdObject
    {
        protected AdObjectState state;
        public AdObjectState State { get => state; set => state = value; }
        protected AdPlacement.Type adPlacementType;
        public AdPlacement.Type AdPlacementType { get => adPlacementType; set => adPlacementType = value; }
    }

    public class InterstitialAdObject : AdObject
    {
        public AdsManager.InterstitialDelegate onAdLoaded;
        public AdsManager.InterstitialDelegate onAdClosed;

        public InterstitialAdObject()
        {
        }

        public InterstitialAdObject(AdPlacement.Type adPlacementType, AdsManager.InterstitialDelegate onAdLoaded)
        {
            this.AdPlacementType = adPlacementType;
            this.onAdLoaded = onAdLoaded;
        }

        public bool CanShow
        {
            get
            {
                return State == AdObjectState.Ready;
            }
        }
    }

    public class RewardAdObject : AdObject
    {
        public RewardDelegate onAdLoaded;
        public RewardDelegate onAdClosed;

        public RewardAdObject()
        {
        }

        public RewardAdObject(AdPlacement.Type adPlacementType, RewardDelegate onAdClosed)
        {
            this.AdPlacementType = adPlacementType;
            this.onAdClosed = onAdClosed;
        }

        public bool CanShow
        {
            get
            {
                return State == AdObjectState.Ready;
            }
        }
    }

    public class BannerAdObject : AdObject
    {
        public BannerLoadDelegate onAdLoaded;
        public BannerTransform TransformData;

        public BannerAdObject()
        {
        }

        public BannerAdObject(AdPlacement.Type adPlacementType, BannerLoadDelegate onAdLoaded)
        {
            this.AdPlacementType = adPlacementType;
            this.onAdLoaded = onAdLoaded;
        }
    }

    public class AppOpenAdObject : AdObject
    {
        public RewardDelegate onAdLoaded;
        public AdsManager.InterstitialDelegate onAdClosed;

        public AppOpenAdObject()
        {
        }

        public AppOpenAdObject(AdPlacement.Type adPlacementType, RewardDelegate onAdLoaded)
        {
            this.AdPlacementType = adPlacementType;
            this.onAdLoaded = onAdLoaded;
        }

        public bool CanShow
        {
            get
            {
                return State == AdObjectState.Ready;
            }
        }
    }
}