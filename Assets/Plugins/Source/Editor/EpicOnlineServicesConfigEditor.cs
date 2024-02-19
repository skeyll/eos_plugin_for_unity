/*
* Copyright (c) 2021 PlayEveryWare
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

//#define ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE

namespace PlayEveryWare.EpicOnlineServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using Random = System.Random;

    [Serializable]
    public class EpicOnlineServicesConfigEditor : EOSEditorWindow
    {
        private static readonly string WindowTitle = "EOS Dev Portal Configuration";
        private const string IntegratedPlatformConfigFilenameForSteam = "eos_steam_config.json";

        private List<IConfigEditor> platformSpecificConfigEditors;

        int toolbarInt;
        string[] toolbarTitleStrings;

        ConfigHandler<EOSConfig> mainEOSConfigFile;

#if ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE
        string eosGeneratedCFilePath = "";
#endif
        bool prettyPrint;

        ConfigHandler<EOSSteamConfig> steamEOSConfigFile;

        [MenuItem("Tools/EOS Plugin/Dev Portal Configuration")]
        public static void ShowWindow()
        {
            GetWindow<EpicOnlineServicesConfigEditor>(WindowTitle);
        }


        [SettingsProvider]
        public static SettingsProvider CreateProjectSettingsProvider()
        {
            var eosPluginEditorConfigEditor = CreateInstance<EpicOnlineServicesConfigEditor>();
            var keywords = eosPluginEditorConfigEditor.GetKeywords();
            // mark the editor window as being embedded, so it skips auto formatting stuff.
            eosPluginEditorConfigEditor.SetIsEmbedded(true);
            var provider = new SettingsProvider($"Preferences/{WindowTitle}", SettingsScope.Project)
            {
                label = WindowTitle,
                keywords = keywords,
                guiHandler = searchContext =>
                {
                    eosPluginEditorConfigEditor.OnGUI();
                }
            };

            return provider;
        }

        private static string GetConfigDirectory()
        {
            return Path.Combine("Assets", "StreamingAssets", "EOS");
        }

        public static string GetConfigPath(string configFilename)
        {
            return Path.Combine(GetConfigDirectory(), configFilename);
        }

        private string GetWindowsPluginDirectory()
        {
            return "";
        }

        private string GenerateEOSGeneratedFile(EOSConfig aEOSConfig)
        {
            return string.Format(String.Join("\n", "#define EOS_PRODUCT_NAME \"{0}\"", "#define EOS_PRODUCT_VERSION \"{1}\"", "#define EOS_SANDBOX_ID \"{2}\"", "#define EOS_PRODUCT_ID \"{3}\"", "#define EOS_DEPLOYMENT_ID \"{4}\"", "#define EOS_CLIENT_SECRET \"{5}\"", "#define EOS_CLIENT_ID \"{6}\""), aEOSConfig.productName,
            aEOSConfig.productVersion,
            aEOSConfig.productID,
            aEOSConfig.sandboxID,
            aEOSConfig.deploymentID,
            aEOSConfig.clientSecret,
            aEOSConfig.clientID) +
            @"
_WIN32 || _WIN64
#define PLATFORM_WINDOWS 1
#endif

#if _WIN64
#define PLATFORM_64BITS 1
#else
#define PLATFORM_32BITS 1
#endif

        extern ""C"" __declspec(dllexport) char*  __stdcall GetConfigAsJSONString()
{
            return ""{""
              ""productName:"" EOS_PRODUCT_NAME "",""
              ""productVersion: "" EOS_PRODUCT_VERSION "",""
              ""productID: ""  EOS_PRODUCT_ID "",""
              ""sandboxID: ""  EOS_SANDBOX_ID "",""
              ""deploymentID: "" EOS_DEPLOYMENT_ID "",""
              ""clientSecret: ""  EOS_CLIENT_SECRET "",""
              ""clientID: ""  EOS_CLIENT_ID

           ""}""
        ;
        }";
        }


        // read data from json file, if it exists
        // TODO: Handle different versions of the file?
        private void LoadConfigFromDisk()
        {
            if (!Directory.Exists(GetConfigDirectory()))
            {
                Directory.CreateDirectory(GetConfigDirectory());
            }

            mainEOSConfigFile.Read();
            steamEOSConfigFile.Read();

            foreach (var platformSpecificConfigEditor in platformSpecificConfigEditors)
            {
                platformSpecificConfigEditor.Load();
            }
        }

        public IList<string> GetKeywords()
        {
            if (null == platformSpecificConfigEditors)
            {
                Setup();
            }

            IList<string> keywords = new List<string>();
            foreach (var section in platformSpecificConfigEditors)
            {
                keywords.Add(section.GetLabelText());
            }

            return keywords;
        }

        protected override void Setup()
        {
            mainEOSConfigFile = new ConfigHandler<EOSConfig>(GetConfigPath(EOSPackageInfo.ConfigFileName));
            steamEOSConfigFile = new ConfigHandler<EOSSteamConfig>(GetConfigPath(IntegratedPlatformConfigFilenameForSteam));

            platformSpecificConfigEditors ??= new List<IConfigEditor>
                {
                    new LinuxConfigEditor(),
                    new AndroidConfigEditor(),
                    new IOSConfigEditor(),
                    new MacOSConfigEditor()
                };

            toolbarTitleStrings = new string[2 + platformSpecificConfigEditors.Count];
            toolbarTitleStrings[0] = "Main";
            toolbarTitleStrings[1] = "Steam";

            int i = 2;
            foreach (var platformSpecificConfigEditor in platformSpecificConfigEditors)
            {
                platformSpecificConfigEditor.Load();
                toolbarTitleStrings[i] = platformSpecificConfigEditor.GetLabelText();
                i++;
            }

            LoadConfigFromDisk();
        }

        private void SaveToJSONConfig(bool prettyPrint)
        {
            mainEOSConfigFile.Write(prettyPrint);
            steamEOSConfigFile.Write(prettyPrint);

            foreach (var platformSpecificConfigEditor in platformSpecificConfigEditors)
            {
                platformSpecificConfigEditor.Save(prettyPrint);
            }

#if ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE
            string generatedCFile = GenerateEOSGeneratedFile(mainEOSConfigFile.Data);
            File.WriteAllText(Path.Combine(eosGeneratedCFilePath, "EOSGenerated.c"), generatedCFile);
#endif

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        private void OnDefaultGUI()
        {
            GUILayout.Label("Epic Online Services", EditorStyles.boldLabel);

            float originalLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = 200;

            // TODO: Id the Product Name userfacing? If so, we need loc
            GUIEditorHelper.AssigningTextField("Product Name", ref mainEOSConfigFile.Data.productName, tooltip: "Product Name defined in the EOS Development Portal");

            // TODO: bool to take product version form application version; should be automatic?
            GUIEditorHelper.AssigningTextField("Product Version", ref mainEOSConfigFile.Data.productVersion, tooltip: "Version of Product");
            GUIEditorHelper.AssigningTextField("Product ID", ref mainEOSConfigFile.Data.productID, tooltip: "Product ID defined in the EOS Development Portal");
            GUIEditorHelper.AssigningTextField("Sandbox ID", ref mainEOSConfigFile.Data.sandboxID, tooltip: "Sandbox ID defined in the EOS Development Portal");
            GUIEditorHelper.AssigningTextField("Deployment ID", ref mainEOSConfigFile.Data.deploymentID, tooltip: "Deployment ID defined in the EOS Development Portal");

            GUIEditorHelper.AssigningBoolField("Is Server", ref mainEOSConfigFile.Data.isServer, tooltip: "Set to 'true' if the application is a dedicated game serve");

            EditorGUILayout.LabelField("Sandbox Deployment Overrides");
            if (mainEOSConfigFile.Data.sandboxDeploymentOverrides == null)
            {
                mainEOSConfigFile.Data.sandboxDeploymentOverrides = new List<SandboxDeploymentOverride>();
            }
            for (int i = 0; i < mainEOSConfigFile.Data.sandboxDeploymentOverrides.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                GUIEditorHelper.AssigningTextField("Sandbox ID", ref mainEOSConfigFile.Data.sandboxDeploymentOverrides[i].sandboxID, tooltip: "Deployment ID will be overridden when Sandbox ID is set to this", labelWidth: 70);
                mainEOSConfigFile.Data.sandboxDeploymentOverrides[i].sandboxID = mainEOSConfigFile.Data.sandboxDeploymentOverrides[i].sandboxID.Trim();
                GUIEditorHelper.AssigningTextField("Deployment ID", ref mainEOSConfigFile.Data.sandboxDeploymentOverrides[i].deploymentID, tooltip: "Deployment ID to use for override", labelWidth: 90);
                mainEOSConfigFile.Data.sandboxDeploymentOverrides[i].deploymentID = mainEOSConfigFile.Data.sandboxDeploymentOverrides[i].deploymentID.Trim();
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(70)))
                {
                    mainEOSConfigFile.Data.sandboxDeploymentOverrides.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add", GUILayout.MaxWidth(100)))
            {
                mainEOSConfigFile.Data.sandboxDeploymentOverrides.Add(new SandboxDeploymentOverride());
            }

            GUIEditorHelper.AssigningULongToStringField("Thread Affinity: networkWork", ref mainEOSConfigFile.Data.ThreadAffinity_networkWork,
                tooltip: "(Optional) Specifies thread affinity for network management that is not IO");
            GUIEditorHelper.AssigningULongToStringField("Thread Affinity: storageIO", ref mainEOSConfigFile.Data.ThreadAffinity_storageIO,
                tooltip: "(Optional) Specifies affinity for threads that will interact with a storage device");
            GUIEditorHelper.AssigningULongToStringField("Thread Affinity: webSocketIO", ref mainEOSConfigFile.Data.ThreadAffinity_webSocketIO,
                tooltip: "(Optional) Specifies affinity for threads that generate web socket IO");
            GUIEditorHelper.AssigningULongToStringField("Thread Affinity: P2PIO", ref mainEOSConfigFile.Data.ThreadAffinity_P2PIO,
                tooltip: "(Optional) Specifies affinity for any thread that will generate IO related to P2P traffic and management");
            GUIEditorHelper.AssigningULongToStringField("Thread Affinity: HTTPRequestIO", ref mainEOSConfigFile.Data.ThreadAffinity_HTTPRequestIO,
                tooltip: "(Optional) Specifies affinity for any thread that will generate http request IO");
            GUIEditorHelper.AssigningULongToStringField("Thread Affinity: RTCIO", ref mainEOSConfigFile.Data.ThreadAffinity_RTCIO,
                tooltip: "(Optional) Specifies affinity for any thread that will generate IO related to RTC traffic and management");

            string timeBudgetAsSting = "";

            if (mainEOSConfigFile.Data.tickBudgetInMilliseconds != 0)
            {
                timeBudgetAsSting = mainEOSConfigFile.Data.tickBudgetInMilliseconds.ToString();
            }
            GUIEditorHelper.AssigningTextField("Time Budget in milliseconds", ref timeBudgetAsSting, tooltip: "(Optional) Define the maximum amount of execution time the EOS SDK can use each frame");

            if (timeBudgetAsSting.Length != 0)
            {
                try
                {
                    mainEOSConfigFile.Data.tickBudgetInMilliseconds = Convert.ToUInt32(timeBudgetAsSting, 10);
                }
                catch
                {

                }
            }
            else
            {
                mainEOSConfigFile.Data.tickBudgetInMilliseconds = 0;
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;

            // This will be used on Windows via the nativerender code, unless otherwise specified
            EditorGUILayout.Separator();
            GUILayout.Label("Default Client Credentials", EditorStyles.boldLabel);
            GUIEditorHelper.AssigningTextField("Client ID", ref mainEOSConfigFile.Data.clientID, tooltip: "Client ID defined in the EOS Development Portal");
            GUIEditorHelper.AssigningTextField("Client Secret", ref mainEOSConfigFile.Data.clientSecret, tooltip: "Client Secret defined in the EOS Development Portal");
            GUI.SetNextControlName("KeyText");
            GUIEditorHelper.AssigningTextField("Encryption Key", ref mainEOSConfigFile.Data.encryptionKey, tooltip: "Used to decode files previously encoded and stored in EOS");
            GUI.SetNextControlName("GenerateButton");
            if (GUILayout.Button("Generate"))
            {
                //generate random 32-byte hex sequence
                var rng = new Random(SystemInfo.deviceUniqueIdentifier.GetHashCode() * (int)(EditorApplication.timeSinceStartup * 1000));
                var keyBytes = new byte[32];
                rng.NextBytes(keyBytes);
                mainEOSConfigFile.Data.encryptionKey = BitConverter.ToString(keyBytes).Replace("-", "");
                //unfocus key input field so the new key is shown
                if (GUI.GetNameOfFocusedControl() == "KeyText")
                {
                    GUI.FocusControl("GenerateButton");
                }
            }

            if (!mainEOSConfigFile.Data.IsEncryptionKeyValid())
            {
                int keyLength = mainEOSConfigFile.Data.encryptionKey?.Length ?? 0;
                EditorGUILayout.HelpBox("Used for Player Data Storage and Title Storage. Must be left blank if unused. Encryption key must be 64 hex characters (0-9,A-F). Current length is " + keyLength + ".", MessageType.Warning);
            }

            GUIEditorHelper.AssigningFlagTextField("Platform Flags (Seperated by '|')", ref mainEOSConfigFile.Data.platformOptionsFlags, 190,
                "Flags used to initialize EOS Platform. Available flags are defined in PlatformFlags.cs");
            GUIEditorHelper.AssigningFlagTextField("Auth Scope Flags (Seperated by '|')", ref mainEOSConfigFile.Data.authScopeOptionsFlags, 210,
                "Flags used to specify Auth Scope during login. Available flags are defined in AuthScopeFlags.cs");

            GUIEditorHelper.AssigningBoolField("Always send Input to Overlay", ref mainEOSConfigFile.Data.alwaysSendInputToOverlay, 190,
                "If true, the plugin will always send input to the overlay from the C# side to native, and handle showing the overlay. This doesn't always mean input makes it to the EOS SDK.");
        }

        private void OnSteamGUI()
        {
            GUILayout.Label("Steam Configuration Values", EditorStyles.boldLabel);
            GUIEditorHelper.AssigningFlagTextField("Steam Flags (Seperated by '|')", ref steamEOSConfigFile.Data.flags, 190);
            GUIEditorHelper.AssigningTextField("Override Library path", ref steamEOSConfigFile.Data.overrideLibraryPath);
            GUIEditorHelper.AssigningUintField("Steamworks SDK major version", ref steamEOSConfigFile.Data.steamSDKMajorVersion, 190);
            GUIEditorHelper.AssigningUintField("Steamworks SDK minor version", ref steamEOSConfigFile.Data.steamSDKMinorVersion, 190);

            if (GUILayout.Button("Update from Steamworks.NET", GUILayout.MaxWidth(200)))
            {
                var steamworksVersion = Steamworks_Utility.GetSteamworksVersion();
                var versionParts = steamworksVersion.Split('.');
                bool success = false;
                if (versionParts.Length >= 2)
                {
                    success = uint.TryParse(versionParts[0], out uint major);
                    success &= uint.TryParse(versionParts[1], out uint minor);
                    if (success)
                    {
                        steamEOSConfigFile.Data.steamSDKMajorVersion = major;
                        steamEOSConfigFile.Data.steamSDKMinorVersion = minor;
                    }
                }
                if (!success)
                {
                    Debug.LogError("Failed to retrive Steamworks SDK version from Steamworks.NET");
                }
            }
        }

        // TODO: create way to hook up new platforms dynamically 
        private string[] CreateToolbarTitles()
        {
            return toolbarTitleStrings;
        }

        protected override void RenderWindow()
        {
            string[] toolbarTitlesToUse = CreateToolbarTitles();
            int xCount = (int)(EditorGUIUtility.currentViewWidth / 200);
            toolbarInt = GUILayout.SelectionGrid(toolbarInt, toolbarTitlesToUse, xCount);
            switch (toolbarInt)
            {
                case 0:
                    OnDefaultGUI();
                    break;
                case 1:
                    OnSteamGUI();
                    break;
                default:
                    if (platformSpecificConfigEditors.Count > toolbarInt - 2)
                    {
                        platformSpecificConfigEditors[toolbarInt - 2].Render();
                    }
                    break;
            }

#if ALLOW_CREATION_OF_EOS_CONFIG_AS_C_FILE
        if (GUILayout.Button("Pick Path For Generated C File"))
        {
            eosGeneratedCFilePath = EditorUtility.OpenFolderPanel("Pick Path For Generated C File", "", "");
        }
#endif
            EditorGUILayout.Separator();
            GUILayout.Label("Config Format Options", EditorStyles.boldLabel);
            GUIEditorHelper.AssigningBoolField("Save JSON in 'Pretty' Format", ref prettyPrint, 190);
            if (GUILayout.Button("Save All Changes"))
            {
                SaveToJSONConfig(prettyPrint);
            }

            if (GUILayout.Button("Show in Explorer"))
            {
                EditorUtility.RevealInFinder(GetConfigDirectory());
            }
        }
    }
}