using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEditor;

namespace MTAssets
{
    /*
    * This class is responsible for installing MT Assets Patcher. MT Assets Patcher helps keep your assets
    * up-to-date and provides emergency fixes to MT Assets files, preventing you from having problems,
    * and no need to wait days for a new update to be released.
    *
    * Don't worry, no files obtained by MT Assets Patcher will be present in your game builds.
    */

    [InitializeOnLoad]
    class PatcherUpdater
    {
        [MenuItem("Tools/MT Assets Patcher/Force Update", false, 10)]
        static void OpenChangeLog()
        {
            if (File.Exists("Assets/MT Assets/_AssetsData/Patcher/LastCheck.ini") == true)
            {
                File.Delete("Assets/MT Assets/_AssetsData/Patcher/LastCheck.ini");
            }
            AssetDatabase.Refresh();
            RunUpdater();
        }

        static PatcherUpdater()
        {
            //Run the patcher after Unity compiles
            EditorApplication.delayCall += RunUpdater;
        }

        static void RunUpdater()
        {
            //Get path of last check file
            string lastCheckPath = "Assets/MT Assets/_AssetsData/Patcher/LastCheck.ini";

            //Create file of last check if not exists
            if (File.Exists(lastCheckPath) == false)
            {
                File.WriteAllText(lastCheckPath, "NoCheck");
            }

            //Verify date of last check
            string lastCheckDate = File.ReadAllText(lastCheckPath);

            //Get date of today
            DateTime date = DateTime.Now;
            string today = date.Year + "/" + date.Month + "/" + date.Day;

            //Verify if date is differente
            if (lastCheckDate != today)
            {
                //Verify if exists update to MT Assets Patcher, if exists, download and update it
                ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                HttpWebRequest request = WebRequest.Create("http://nossl.windsoft.xyz/client/mtassets-patcher/get-current-patcher-code.php") as HttpWebRequest;
                HttpWebResponse response;
                try{response = (HttpWebResponse)request.GetResponse();}
#pragma warning disable CS0168 //Warning supress
                catch (WebException exception) { return; }
#pragma warning restore CS0168 //Warning supress
                using (var reader = new StreamReader(response.GetResponseStream(), ASCIIEncoding.ASCII))
                {
                    //If the connection to the server has worked
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                    {
                        //Download the file
                        string documentDownloaded = reader.ReadToEnd();

                        //Verify that is a valid script
                        if (documentDownloaded.Contains("using UnityEngine;") == true)
                        {
                            //Get the patch to patcher
                            string patcherPath = "Assets/MT Assets/_AssetsData/Patcher/Editor/Patcher.cs";

                            //Verify if patcher no exists, if no, install the new
                            if (File.Exists(patcherPath) == false)
                            {
                                File.WriteAllText(patcherPath, documentDownloaded);
                                File.WriteAllText(lastCheckPath, today);
                                AssetDatabase.Refresh();
                            }

                            //If exists, verify if the local Patcher.cs is differente of server Patcher.cs
                            if (File.Exists(patcherPath) == true)
                            {
                                int charsOfLocalPatcher = File.ReadAllText(patcherPath).Length;

                                if (charsOfLocalPatcher == documentDownloaded.Length)
                                {
                                    File.WriteAllText(lastCheckPath, today);
                                    AssetDatabase.Refresh();
                                }

                                if (charsOfLocalPatcher != documentDownloaded.Length)
                                {
                                    File.WriteAllText(patcherPath, documentDownloaded);
                                    File.WriteAllText(lastCheckPath, today);
                                    AssetDatabase.Refresh();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}