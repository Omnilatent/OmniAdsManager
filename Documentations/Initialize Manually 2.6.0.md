Since v2.6.0, Ads Manager can be initialized manually.

This can break some code that expected Ads Manager to be initialized during Awake() cycle. E.g: Handle Ads Manager Message.

Please update all affected code to check for AdsManager.Initialized.

Use AdsManager.OnInitializedEvent for callback when Ads Manager has finish Initialized.