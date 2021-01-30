using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Omnilatent.AdsManager
{
    public class SetupAdsNetwork : MonoBehaviour
    {
        const string packageName = "com.omnilatent.adsmanager";

        [MenuItem("Tools/Omnilatent/Ads Manager/Add Admob")]
        public static void AddAdmobHelper()
        {
            //string packagePath = Path.GetFullPath($"Packages/{packageName}");
            string packagePath = Path.GetFullPath($"Assets/OmniAds");
            int line_to_edit = 0;
            string sourceFile = Path.Combine(packagePath, "OmniAdsManager/AdsManagerTemplates/AdsManagerAdmob.cs");
            string destinationFile = Path.Combine(packagePath, "OmniAdsManager/AdsManagerTemplates/AdsManagerAdmob.cs");
            string tempFile = Path.Combine(packagePath, "OmniAdsManager/AdsManagerTemplates/AdsManagerAdmob.cs.temp"); ;

            // Read the appropriate line from the file.
            string lineToWrite = "#if true //MODULE_MAKER";
            string line = null;
            bool hasFoundLine = false;
            using (StreamReader reader = new StreamReader(sourceFile))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    line_to_edit++;
                    if (line.Equals("#if false //MODULE_MAKER"))
                    {
                        hasFoundLine = true;
                        break;
                    }
                    else if (line.Equals("#if true //MODULE_MAKER"))
                    {
                        lineToWrite = "#if false //MODULE_MAKER";
                        hasFoundLine = true;
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

            string backupFile = Path.Combine(packagePath, "OmniAdsManager/AdsManagerTemplates/AdsManagerAdmob.cs.bak"); ;
            File.Delete(backupFile);
            File.Move(sourceFile, backupFile);
            File.Move(tempFile, destinationFile);
            Debug.Log("AdsManager Admob has been modified.");
        }
    }
}