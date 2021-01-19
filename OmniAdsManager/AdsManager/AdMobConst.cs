using System.Collections;
using System.Collections.Generic;

public class AdMobConst
{
#if DEBUG_ADS
    public const string ADMOB_APP_ID = "ca-app-pub-7830655096475746~3981052972";
    public const string BANNER_ID = "ca-app-pub-3940256099942544/6300978111";
    public const string REWARD_ID = "ca-app-pub-3940256099942544/5224354917";
    public const string REWARD_SKIP_ID = "ca-app-pub-3940256099942544/5224354917";
    public const string REWARD_GET_MORE_HINT_ID = "ca-app-pub-3940256099942544/5224354917";
    public const string INTERSTITIAL_SPLASH = "ca-app-pub-3940256099942544/8691691433";
    public const string INTERSTITIAL_COMPLETELEVEL = "ca-app-pub-3940256099942544/8691691433";

#else
    public const string ADMOB_APP_ID = "ca-app-pub-7830655096475746~3981052972";
    public const string BANNER_ID = "ca-app-pub-7830655096475746/3876292958";
    public const string REWARD_ID = "ca-app-pub-7830655096475746/4942780854";
    public const string REWARD_SKIP_ID = "ca-app-pub-7830655096475746/4942780854";
    public const string REWARD_GET_MORE_HINT_ID = "ca-app-pub-7830655096475746/8937047948";
    public const string INTERSTITIAL_SPLASH = "ca-app-pub-7830655096475746/3932133277";
    public const string INTERSTITIAL_COMPLETELEVEL = "ca-app-pub-7830655096475746/3437309258";

    //same ad IDs for cached load
    //public const string Interstitial_Continue = "ca-app-pub-7830655096475746/9238897593";
    //public const string Interstitial_Endgame = "ca-app-pub-7830655096475746/9238897593";

    //Unique Ad IDs

#endif
}
