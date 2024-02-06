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

        /// <summary>
        /// Show a black screen with the text "Loading ad" to cover the whole screen and the banner ad.
        /// </summary>
        /// <param name="cancelableDelay">The amount of miliseconds to wait before setting the black screen cancelable to true to allow use Back button to close it in case it's not closed by callback. Set to -1 if you don't need it.</param>
        public static void Show(int cancelableDelay = 4000)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                Initialize();
                _pluginInstance.CallStatic("showDialog", unityActivity, cancelableDelay);
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
                _pluginInstance.CallStatic("closeDialog");
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