using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation
{
    public class ScreenBlock
    {
        private static AndroidJavaClass unityClass;
        private static AndroidJavaObject unityActivity;
        private static AndroidJavaObject _pluginInstance;
        private static bool initialized;

        public static void Initialize()
        {
            if (initialized) return;

            #if UNITY_ANDROID && !UNITY_EDITOR
            unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            _pluginInstance = new AndroidJavaObject("com.omnilatent.screenblock.DialogUtil");
            #endif
            initialized = true;
        }

        public static void Show()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                Initialize();
                _pluginInstance.Call("showDialog", unityActivity);
            }
            catch (Exception e)
            {
                LogException(e);
            }
            #endif
        }

        public static void Hide()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                Initialize();
                _pluginInstance.Call("closeDialog");
            }
            catch (Exception e)
            {
                LogException(e);
            }
            #endif
        }

        static void LogException(Exception e)
        {
            #if OMNILATENT_FIREBASE_MANAGER
            FirebaseManager.LogException(e);
            #else
            Debug.LogException(e);
            #endif
        }
    }
}