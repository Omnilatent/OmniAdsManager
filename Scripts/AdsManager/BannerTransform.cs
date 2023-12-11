using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation
{
    // The position of the ad on the screen.
    public enum AdPosition
    {
        Unset = -1,
        Top = 0,
        Bottom = 1,
        TopLeft = 2,
        TopRight = 3,
        BottomLeft = 4,
        BottomRight = 5,
        Center = 6
    }

    public class BannerTransform
    {
        public AdPosition adPosition = AdPosition.Bottom;
        public object adSizeData;
        
        /// <summary>
        /// Currently supported by Admob Banner, with position as top and bottom. If true, banner ad will be collapsible.
        /// </summary>
        public bool Collapsible;

        public static BannerTransform defaultValue = new BannerTransform();

        public BannerTransform()
        {
        }

        public BannerTransform(AdPosition adPosition)
        {
            this.adPosition = adPosition;
        }
    }

    
}