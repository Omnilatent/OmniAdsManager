using System;

public partial class AdsManager{
    public static Action<AdPlacement.Type> OnInterAdRequestEvent;
    public static Action<AdPlacement.Type> OnRewardAdRequestEvent;
    public static Action<AdPlacement.Type> OnBannerAdRequestEvent;
    public static Action<AdPlacement.Type> OnAppOnAdRequestEvent;
}