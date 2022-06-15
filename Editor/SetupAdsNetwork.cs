using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Omnilatent.AdsMediation
{
    [InitializeOnLoad]
    public static class SetupAdsNetwork
    {
        const string packageName = "com.omnilatent.adsmanager";

        public static bool isDebugAd;
        const string debugAdMenuName = "Tools/Omnilatent/Ads Manager/Debug Ad";
        const string debugAdDefineSymbol = "DEBUG_ADS";

        static SetupAdsNetwork()
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();
            isDebugAd = allDefines.Contains(debugAdDefineSymbol);
            Menu.SetChecked(debugAdMenuName, isDebugAd);
        }

        /// <summary>
        /// Add a define symbol
        /// </summary>
        /// <returns>True if added, false if removed</returns>
        static bool AddDefineSymbols(string newSymbol)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();

            bool isAdd = false;
            if (allDefines.Contains(newSymbol))
            {
                allDefines.Remove(newSymbol);
            }
            else
            {
                allDefines.Add(newSymbol);
                isAdd = true;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
            return isAdd;
        }

        [MenuItem(debugAdMenuName)]
        private static void ToggleDebugAd()
        {
            isDebugAd = AddDefineSymbols(debugAdDefineSymbol);
            Menu.SetChecked(debugAdMenuName, isDebugAd);
        }

        static string GetScriptPath()
        {
            //default path is Asset/Omnilatent/OmniAdsManager/Scripts
            string[] res = System.IO.Directory.GetFiles(Application.dataPath, "SetupAdsNetwork.cs", SearchOption.AllDirectories);
            if (res.Length == 0)
            {
                Debug.LogError("Script SetupAdsNetwork.cs not found");
                return null;
            }

            string path = res[0].Replace("\\", "/");
            bool foundInCorrectPath = false;
            foreach (var pathItem in res)
            {
                string pathItemFormatted = pathItem.Replace("\\", "/");
                if (pathItemFormatted.Contains("OmniAdsManager/Scripts/Editor"))
                {
                    path = pathItemFormatted;
                    foundInCorrectPath = true;
                }
            }
            if (!foundInCorrectPath)
                Debug.LogWarning("Script SetupAdsNetwork.cs not found in correct folder. This can prevent the script from working correctly.");
            path = path.Replace("Editor/SetupAdsNetwork.cs", "");
            Debug.Log(path);
            return path;
        }

        #region Obsolete
        /*[MenuItem("Tools/Omnilatent/Ads Manager/Toggle Admob")]
        public static void AddAdmobHelper()
        {
            AddNetworkHelper("AdsManagerAdmob");
        }

        [MenuItem("Tools/Omnilatent/Ads Manager/Toggle Facebook Audience Network")]
        public static void AddFAN()
        {
            AddNetworkHelper("AdsManagerFAN");
        }*/

        /// <summary>
        /// For modifying AdsManagerAdmob.cs and ad network manager script to include new functions. But it's not needed anymore
        /// </summary>
        static void AddNetworkHelper(string scriptToAdd)
        {
            string packagePath = Path.GetFullPath($"Packages/{packageName}/Scripts/Modules");
            //string assetPath = Path.GetFullPath($"Asset/Omnilatent/OmniAdsManager/Scripts/Modules");
            string assetPath = GetScriptPath() + "/Modules";
            int line_to_edit = 0;
            string sourceFile = Path.Combine(assetPath, $"{scriptToAdd}.cs");
            string destinationFile = Path.Combine(assetPath, $"{scriptToAdd}.cs");
            string tempFile = Path.Combine(assetPath, $"{scriptToAdd}.cs.temp"); ;

            // Read the appropriate line from the file.
            string lineToWrite = "#if true //MODULE_MAKER";
            string line = null;
            bool hasFoundLine = false;
            bool isEnabling = false;

            using (StreamReader reader = new StreamReader(sourceFile))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    line_to_edit++;
                    if (line.Equals("#if false //MODULE_MAKER"))
                    {
                        hasFoundLine = true;
                        isEnabling = true;
                        break;
                    }
                    else if (line.Equals("#if true //MODULE_MAKER"))
                    {
                        lineToWrite = "#if false //MODULE_MAKER";
                        hasFoundLine = true;
                        isEnabling = false;
                        break;
                    }
                }
            }

            if (!hasFoundLine)
                throw new InvalidDataException("Line does not exist in " + sourceFile);

            // Read from the target file and write to a new file.
            int line_number = 1;
            using (StreamReader reader = new StreamReader(destinationFile))
            using (StreamWriter writer = new StreamWriter(tempFile))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line_number == line_to_edit)
                    {
                        writer.WriteLine(lineToWrite);
                    }
                    else
                    {
                        writer.WriteLine(line);
                    }
                    line_number++;
                }
            }

            //backup current file and delete previous backup
            string backupFile = Path.Combine(assetPath, $"{scriptToAdd}.cs.bak"); ;
            File.Delete(backupFile);
            File.Move(sourceFile, backupFile);
            File.Move(tempFile, destinationFile);

            AssetDatabase.Refresh();
            if (isEnabling)
                Debug.Log($"{scriptToAdd} has been added.");
            else
                Debug.Log($"{scriptToAdd} has been removed.");
        }

        [System.Obsolete("Use Import Extra Package instead")]
        /// <summary>
        /// For creating AdsManager prefab in Resources. 
        /// </summary>
        //[MenuItem("Tools/Omnilatent/Ads Manager/Create AdsManager Prefab")]
        public static void CreateAdsManagerPrefab()
        {
            string[] guids2 = AssetDatabase.FindAssets("AdsManagerSamplePrefab t:prefab");
            string sourcePath = AssetDatabase.GUIDToAssetPath(guids2[0]);
            GameObject samplePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);

            Directory.CreateDirectory($"{Application.dataPath}/Resources");
            string path = $"Assets/Resources/AdsManager.prefab";

            //sourcePath = sourcePath.Substring(7, sourcePath.Length - 7);
            bool success = AssetDatabase.CopyAsset(sourcePath, path);

            //PrefabUtility.SaveAsPrefabAsset(samplePrefab, path, out bool success);
            if (success)
                Debug.Log($"Created AdsManager prefab in {path}");
            else
                Debug.LogError($"Creating AdsManager prefab in {path} failed");
        }
        #endregion

        [MenuItem("Tools/Omnilatent/Ads Manager/Import Extra Package")]
        public static void ImportExtraPackage()
        {
            string path = GetPackagePath("Assets/Omnilatent/OmniAdsManager/OmniAdsManagerExtra.unitypackage", "OmniAdsManagerExtra");
            AssetDatabase.ImportPackage(path, true);
        }

        [MenuItem("Tools/Omnilatent/Ads Manager/Import AudienceNetwork Assembly Fix")]
        public static void ImportAudienceNetworkAssemblyFix()
        {
            //For fixing "error CS0117: 'AudienceNetworkAds' does not contain a definition for 'IsInitialized'":
            string path = GetPackagePath("Assets/Omnilatent/OmniAds FAN/AudienceNetworkAssemblyFix.unitypackage", "AudienceNetworkAssemblyFix");
            AssetDatabase.ImportPackage(path, true);
        }

        [MenuItem("Tools/Omnilatent/Ads Manager/Import IronSource Extra Package")]
        public static void ImportIronSourceExtraPackage()
        {
            string path = GetPackagePath("Assets/Omnilatent/IronSourceAdsManager/IronSourceHelperExtra.unitypackage", "IronSourceHelperExtra");
            AssetDatabase.ImportPackage(path, true);
        }

        [MenuItem("Tools/Omnilatent/Ads Manager/Import IronSource Assembly Definition")]
        public static void ImportIronSourceAssemblyDefinition()
        {
            //To make IronSource work with package imported using Unity Package Manager
            string path = GetPackagePath("Assets/Omnilatent/IronSourceAdsManager/IronSourceAssemblyDefinition.unitypackage", "IronSourceAssemblyDefinition");
            AssetDatabase.ImportPackage(path, true);
        }

        [MenuItem("Tools/Omnilatent/Ads Manager/Import MAX Extra Package")]
        public static void ImportMAXExtraPackage()
        {
            string path = GetPackagePath("Assets/Omnilatent/MAXAdsManager/MAXAdsWrapperExtra.unitypackage", "MAXAdsWrapperExtra");
            AssetDatabase.ImportPackage(path, true);
        }

        static string GetPackagePath(string path, string filename)
        {
            if (!File.Exists($"{Application.dataPath}/../{path}"))
            {
                Debug.Log($"{filename} not found at {path}, attempting to search whole project for {filename}");
                string[] guids = AssetDatabase.FindAssets($"{filename} l:package");
                if (guids.Length > 0)
                {
                    path = AssetDatabase.GUIDToAssetPath(guids[0]);
                }
                else
                {
                    Debug.LogError($"{filename} not found at {Application.dataPath}/../{path}");
                    return null;
                }
            }
            return path;
        }
    }
}