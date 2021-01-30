﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Omnilatent.AdsManager
{
    public class SetupAdsNetwork
    {
        const string packageName = "com.omnilatent.adsmanager";

        [MenuItem("Tools/Omnilatent/Ads Manager/Add Admob")]
        public static void AddAdmobHelper()
        {
            AddNetworkHelper("AdsManagerAdmob");
        }

        [MenuItem("Tools/Omnilatent/Ads Manager/Add Facebook Audience Network")]
        public static void AddFAN()
        {
            AddNetworkHelper("AdsManagerFAN");
        }

        static void AddNetworkHelper(string scriptToAdd)
        {
            string packagePath = Path.GetFullPath($"Packages/{packageName}/OmniAds/OmniAdsManager/Modules");
            string assetPath = Path.GetFullPath($"Assets/OmniAds/OmniAdsManager/Modules");
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
    }
}