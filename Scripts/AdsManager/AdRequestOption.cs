using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation
{
    public struct AdRequestOption
    {
        public AdsManager.InterstitialDelegate onAdLoaded;
        public AdsManager.InterstitialDelegate onAdClose;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onAdLoaded">Callback when ad is loaded</param>
        /// <param name="onAdClose">Callback when ad is closed</param>
        public AdRequestOption(AdsManager.InterstitialDelegate onAdLoaded = null, AdsManager.InterstitialDelegate onAdClose = null)
        {
            this.onAdLoaded = onAdLoaded;
            this.onAdClose = onAdClose;
        }
    }
}
