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

    public class InterstitialAdObject
    {
        public AdPlacement.Type adPlacementType;
        public AdsManager.InterstitialDelegate onAdLoaded;
        public AdsManager.InterstitialDelegate onAdClosed;
        public AdObjectState state;

        public InterstitialAdObject()
        {
        }

        public InterstitialAdObject(AdPlacement.Type adPlacementType, AdsManager.InterstitialDelegate onAdLoaded)
        {
            this.adPlacementType = adPlacementType;
            this.onAdLoaded = onAdLoaded;
        }

        public bool CanShow
        {
            get
            {
                return state == AdObjectState.Ready;
            }
        }
    }

    public class RewardAdObject
    {
        public AdPlacement.Type adPlacementType;
        public RewardDelegate onAdLoaded;
        public RewardDelegate onAdClosed;
        public AdObjectState state;

        public RewardAdObject()
        {
        }

        public RewardAdObject(AdPlacement.Type adPlacementType, RewardDelegate onAdClosed)
        {
            this.adPlacementType = adPlacementType;
            this.onAdClosed = onAdClosed;
        }

        public bool CanShow
        {
            get
            {
                return state == AdObjectState.Ready;
            }
        }
    }

    public class BannerAdObject
    {
        public AdPlacement.Type adPlacementType;
        public AdObjectState state;

        public BannerAdObject()
        {
        }

        public BannerAdObject(AdPlacement.Type adPlacementType)
        {
            this.adPlacementType = adPlacementType;
        }
    }
}