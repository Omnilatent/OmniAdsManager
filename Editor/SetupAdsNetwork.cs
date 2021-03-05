using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Omnilatent.AdsManager
{
    public class SetupAdsNetwork
    {
        const string packageName = "com.omnilatent.adsmanager";

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

        [MenuItem("Tools/Omnilatent/Ads Manager/Toggle Admob")]
        public static void AddAdmobHelper()
        {
            AddNetworkHelper("AdsManagerAdmob");
        }

        [MenuItem("Tools/Omnilatent/Ads Manager/Toggle Facebook Audience Network")]
        public static void AddFAN()
        {
            AddNetworkHelper("AdsManagerFAN");
        }

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

        [MenuItem("Tools/Omnilatent/Ads Manager/Create AdsManager Prefab")]
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
    }
}