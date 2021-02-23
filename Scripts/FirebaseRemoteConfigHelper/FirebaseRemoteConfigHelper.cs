using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;
using Firebase.RemoteConfig;
using SS.View;
using System.Threading.Tasks;
using System;
using Firebase.Extensions;

public class FirebaseRemoteConfigHelper : MonoBehaviour
{
    public static System.EventHandler<bool> onFetchComplete;
    public static FirebaseRemoteConfigHelper instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        if (FirebaseManager.FirebaseReady) OnFirebaseReady(this, true);
        else FirebaseManager.handleOnReady += OnFirebaseReady;
    }

    [SerializeField] bool showFirebaseInstanceId = false;
    static bool? initSuccess;
    static bool InitSuccessValue
    {
        get
        {
            bool returnVal = initSuccess.HasValue ? initSuccess.Value : false;
            if (!returnVal)
            {
                Debug.Log("Firebase Remote: init not success");
            }
            return returnVal;
        }
    }

    void OnFirebaseReady(object sender, bool isReady)
    {
        initSuccess = isReady;
        if (isReady) Init();
    }

    async void Init()
    {
        string id = await GetFirebaseInstanceId();
        if (Debug.isDebugBuild && showFirebaseInstanceId)
        {
            TextEditor te = new TextEditor();
            te.text = id;
            te.SelectAll();
            te.Copy();
            //Manager.Add(PopupController.POPUP_SCENE_NAME, new PopupData(PopupType.OK, System.String.Format("Instance ID Token {0}", id)));
        }
        if (FirebaseManager.FirebaseReady)
        {
            Debug.Log($"Firebase ready: {FirebaseManager.FirebaseReady}");
            SetDefaultValues();
            initSuccess = true;
        }
        FetchData();

        if (Debug.isDebugBuild)
        {
            var setting = FirebaseRemoteConfig.Settings;
            setting.IsDeveloperMode = true;
        }
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        //FetchData();
        //if (!InitSuccessValue) Debug.Log("Firebase Remote: init not success");
        return InitSuccessValue ? (int)GetConfig(key).DoubleValue : defaultValue;
    }

    public static float GetFloat(string key, float defaultValue = 0)
    {
        return InitSuccessValue ? (float)GetConfig(key).DoubleValue : defaultValue;
    }

    public static bool GetBool(string key, bool defaultValue)
    {
        return InitSuccessValue ? GetConfig(key).BooleanValue : defaultValue;
    }

    public static string GetString(string key, string defaultValue)
    {
        return InitSuccessValue ? GetConfig(key).StringValue : defaultValue;
    }

    static ConfigValue GetConfig(string key)
    {
        return FirebaseRemoteConfig.GetValue(key);
    }

    async Task<string> GetFirebaseInstanceId()
    {
        string result = null;
        if (FirebaseManager.FirebaseReady)
        {
            //Firebase.InstanceId.FirebaseInstanceId.GetInstanceId(FirebaseManager.app);
            await Firebase.InstanceId.FirebaseInstanceId.DefaultInstance.GetTokenAsync().ContinueWith(
            task =>
            {
                if (!(task.IsCanceled || task.IsFaulted) && task.IsCompleted)
                {
                    UnityEngine.Debug.Log(System.String.Format("Instance ID Token {0}", task.Result));
                    result = task.Result;
                }
            });
        }
        return result;
    }

    static void FetchData()
    {
        if (FirebaseManager.FirebaseReady)
        {
            FetchDataAsync((task) =>
            {
                Debug.Log("Fetch async done");
                FirebaseRemoteConfig.ActivateFetched();
                onFetchComplete?.Invoke(null, true);
            });
        }
    }

    // Start a fetch request.
    // FetchAsync only fetches new data if the current data is older than the provided
    // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
    // By default the timespan is 12 hours, and for production apps, this is a good
    // number. For this example though, it's set to a timespan of zero, so that
    // changes in the console will always show up immediately.
    static Task FetchDataAsync(Action<Task> FetchComplete)
    {
        Debug.Log("Fetching data...");
        System.Threading.Tasks.Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.FetchAsync(
            TimeSpan.Zero);
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }

    void SetDefaultValues()
    {
        System.Collections.Generic.Dictionary<string, object> defaults = new System.Collections.Generic.Dictionary<string, object>();
        
        //defaults.Add(Const.RMCF_TIME_BETWEEN_ADS, AdsManager.TIME_BETWEEN_ADS);
        defaults.Add(RemoteConfigAdsPlacement.RMCF_ADS_PLACEMENT_CONFIG, "");

        Firebase.RemoteConfig.FirebaseRemoteConfig.SetDefaults(defaults);
    }

    /// <summary>
    /// Check if Remote Config has fetched successfully. If yes, return callback immediately, else add in to delegate to wait for callback
    /// </summary>
    public static void CheckAndHandleFetchConfig(System.EventHandler<bool> callback)
    {
        if (initSuccess.HasValue) callback(null, InitSuccessValue);
        else onFetchComplete += callback;
    }
 }
