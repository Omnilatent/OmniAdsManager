using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation
{
    public struct AdRequestOption
    {
        public AdsManager.InterstitialDelegate onAdLoaded;
        public AdsManager.InterstitialDelegate onAdClosed;
        public bool showLoading;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onAdLoaded">Callback when ad is loaded</param>
        /// <param name="onAdClosed">Callback when ad is closed</param>
        public AdRequestOption(AdsManager.InterstitialDelegate onAdLoaded = null, AdsManager.InterstitialDelegate onAdClosed = null, bool showLoading = true)
        {
            this.onAdLoaded = onAdLoaded;
            this.onAdClosed = onAdClosed;
            this.showLoading = showLoading;
        }
    }
}
