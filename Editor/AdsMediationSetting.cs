using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Omnilatent.AdsMediation.Editor
{
    public class AdsMediationSetting : ScriptableObject
    {
        private const string MobileAdsSettingsResDir = "Assets/Omnilatent/Resources";

        private const string MobileAdsSettingsFile = "AdsMediationSetting";

        private const string MobileAdsSettingsFileExtension = ".asset";
        internal static AdsMediationSetting LoadInstance()
        {
            //Read from resources.
            var instance = Resources.Load<AdsMediationSetting>(MobileAdsSettingsFile);

            //Create instance if null.
            if (instance == null)
            {
                Directory.CreateDirectory(MobileAdsSettingsResDir);
                instance = ScriptableObject.CreateInstance<AdsMediationSetting>();
                string assetPath = Path.Combine(
                    MobileAdsSettingsResDir,
                    MobileAdsSettingsFile + MobileAdsSettingsFileExtension);
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
            }

            return instance;
        }
    }
}