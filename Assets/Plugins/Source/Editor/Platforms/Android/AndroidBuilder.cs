/*
 * Copyright (c) 2024 PlayEveryWare
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

namespace PlayEveryWare.EpicOnlineServices.Editor.Build
{
    using Config;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using UnityEditor.Android;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using Config = EpicOnlineServices.Config;
    public class AndroidBuilder : PlatformSpecificBuilder
    {
        public AndroidBuilder() : base("Plugins/Android", BuildTarget.Android) { }

        public override void PreBuild(BuildReport report)
        {
            InstallEOSDependentLibrary();
            ConfigureGradleTemplateProperties();
            ConfigureEOSDependentLibrary();
            DetermineLibraryLinkingMethod();
        }

        private static string GetAndroidEOSValuesConfigPath()
        {
            string assetsPathname = Path.Combine(Application.dataPath, "Plugins/Android/EOS/");
            return Path.Combine(assetsPathname, "eos_dependencies.androidlib/res/values/eos_values.xml");
        }

        private static void OverwriteCopy(string fileToInstallPathName, string destPathname)
        {
            if (File.Exists(destPathname))
            {
                File.SetAttributes(destPathname, File.GetAttributes(destPathname) & ~FileAttributes.ReadOnly);
            }

            File.Copy(fileToInstallPathName, destPathname, true);
        }

        private static void InstallFiles(string[] filenames, string pathToInstallFrom, string pathToInstallTo)
        {

            if (!string.IsNullOrEmpty(pathToInstallFrom))
            {
                foreach (var fileToInstall in filenames)
                {
                    string fileToInstallPathName = Path.Combine(pathToInstallFrom, fileToInstall);

                    if (File.Exists(fileToInstallPathName))
                    {
                        string fileToInstallParentDirectory =
                            Path.GetDirectoryName(Path.Combine(pathToInstallTo, fileToInstall));

                        if (!Directory.Exists(fileToInstallParentDirectory))
                        {
                            Directory.CreateDirectory(fileToInstallParentDirectory);
                        }

                        string destPathname = Path.Combine(fileToInstallParentDirectory,
                            Path.GetFileName(fileToInstallPathName));

                        OverwriteCopy(fileToInstallPathName, destPathname);
                    }
                    else
                    {
                        Debug.LogError("Missing platform specific file: " + fileToInstall);
                    }
                }
            }
        }

        private static string GetPlatformSpecificAssetsPath(string subpath)
        {
            string packagePathname = Path.GetFullPath(Path.Combine("Packages", EOSPackageInfo.PackageName,
                "PlatformSpecificAssets~", subpath));
            string streamingAssetsSamplesPathname =
                Path.Combine(Application.dataPath, "..", "etc", "PlatformSpecificAssets", subpath);
            string pathToInstallFrom = "";

            if (Directory.Exists(packagePathname))
            {
                // Install from package path
                pathToInstallFrom = packagePathname;
            }
            else if (Directory.Exists(streamingAssetsSamplesPathname))
            {
                pathToInstallFrom = streamingAssetsSamplesPathname;
            }
            else
            {
                Debug.LogError("PreprocessBuildError : EOS Plugin Package Missing");
            }

            return pathToInstallFrom;
        }

        private static bool DoesGradlePropertiesContainSetting(string gradleTemplatePathname, string setting)
        {
            // check if it contains the android.useAndroidX=true
            foreach (string line in File.ReadAllLines(gradleTemplatePathname))
            {
                if (line.Contains(setting) && !line.StartsWith("#"))
                {
                    return true;
                }
            }

            return false;
        }

        private static void DisableGradleProperty(string gradleTemplatePathname, string setting)
        {
            var gradleTemplateToWrite = new List<string>();

            foreach (string line in File.ReadAllLines(gradleTemplatePathname))
            {
                if (line.Contains(setting) && !line.StartsWith("#"))
                {
                }
                else
                {
                    gradleTemplateToWrite.Add(line);
                }
            }

            File.WriteAllLines(gradleTemplatePathname, gradleTemplateToWrite.ToArray());
        }

        private static void ReplaceOrSetGradleProperty(string gradleTemplatePathname, string setting, string value)
        {
            var gradleTemplateToWrite = new List<string>();
            bool wasAdded = false;

            foreach (string line in File.ReadAllLines(gradleTemplatePathname))
            {
                if (line.Contains(setting) && !line.StartsWith("#"))
                {
                    gradleTemplateToWrite.Add($"{setting}={value}");
                    wasAdded = true;
                }
                else
                {
                    gradleTemplateToWrite.Add(line);
                }
            }

            if (!wasAdded)
            {
                gradleTemplateToWrite.Add($"{setting}={value}");
            }

            File.WriteAllLines(gradleTemplatePathname, gradleTemplateToWrite.ToArray());
        }

        private static int GetTargetAPI()
        {
            //  NOTE: The following section is conditionally enabled because otherwise, if the user
            //        does not have the Android module installed, it will cause build errors.
#if UNITY_ANDROID
            var playerApiTarget = PlayerSettings.Android.targetSdkVersion;
            if (playerApiTarget == AndroidSdkVersions.AndroidApiLevelAuto)
            {
                int maxVersion = 0;
                var apiRegex = new Regex(@"android-(\d+)");
                //find max installed android api
                foreach (var dir in Directory.GetDirectories(Path.Combine(AndroidExternalToolsSettings.sdkRootPath,
                             "platforms")))
                {
                    var dirName = Path.GetFileName(dir);
                    var match = apiRegex.Match(dirName);
                    if (match.Success && match.Groups.Count == 2)
                    {
                        if (int.TryParse(match.Groups[1].Value, out int matchVal))
                        {
                            if (matchVal > maxVersion)
                            {
                                maxVersion = matchVal;
                            }
                        }
                    }
                }

                if (maxVersion == 0)
                {
                    return 29;
                }

                return maxVersion;
            }
            else
            {
                return (int)playerApiTarget;
            }
#else
            return -1; // This should never happen.
#endif
        }

        private static string GetBuildTools()
        {
            //  NOTE: The following section is conditionally enabled because otherwise, if the user
            //        does not have the Android module installed, it will cause build errors.
#if UNITY_ANDROID
            var toolsRegex = new Regex(@"(\d+)\.(\d+)\.(\d+)");
            int maxMajor = 0, maxMinor = 0, maxPatch = 0;
            //find highest usable build tools version
            const int highestVersion = 
#if UNITY_2022_3_OR_NEWER
            34
#elif UNITY_2022_2_OR_NEWER
            32
#else
            30
#endif
;

            foreach (var dir in Directory.GetDirectories(Path.Combine(AndroidExternalToolsSettings.sdkRootPath,
                         "build-tools")))
            {
                var dirName = Path.GetFileName(dir);
                var match = toolsRegex.Match(dirName);
                int majorVersion = 0, minorVersion = 0, patchVersion = 0;
                bool success = match.Success && match.Groups.Count == 4 &&
                               int.TryParse(match.Groups[1].Value, out majorVersion) &&
                               int.TryParse(match.Groups[2].Value, out minorVersion) &&
                               int.TryParse(match.Groups[3].Value, out patchVersion);
                if (success)
                {
                    if (majorVersion > highestVersion)
                    {
                        continue;
                    }

                    if (majorVersion > maxMajor ||
                        (majorVersion == maxMajor && minorVersion > maxMinor) ||
                        (majorVersion == maxMajor && minorVersion == maxMinor && patchVersion > maxPatch))
                    {
                        maxMajor = majorVersion;
                        maxMinor = minorVersion;
                        maxPatch = patchVersion;
                    }
                }
            }

            if (maxMajor == 0)
            {
                return "30.0.3";
            }
            else
            {
                return $"{maxMajor}.{maxMinor}.{maxPatch}";
            }
#else
            return "NOBUILDTOOLS"; // This should never happen
#endif
        }

        private static void WriteConfigMacros(string filepath)
        {
            var contents = File.ReadAllText(filepath);
            string apiVersion = GetTargetAPI().ToString();
            string buildTools = GetBuildTools();
            string newContents = contents.Replace("**APIVERSION**", apiVersion)
                .Replace("**TARGETSDKVERSION**", apiVersion).Replace("**BUILDTOOLS**", buildTools);
            File.WriteAllText(filepath, newContents);
        }

         private static void InstallEOSDependentLibrary()
        {
            string packagedPathname = GetPlatformSpecificAssetsPath("EOS/Android/");

            if (Directory.Exists(packagedPathname))
            {
                string assetsPathname = Path.Combine(Application.dataPath, "Plugins", "Android", "EOS");
                string buildGradlePath = "eos_dependencies.androidlib/build.gradle";
                string[] filenames =
                {
                    "eos_dependencies.androidlib/AndroidManifest.xml", buildGradlePath,
                    "eos_dependencies.androidlib/project.properties",
                    "eos_dependencies.androidlib/res/values/eos_values.xml",
                    "eos_dependencies.androidlib/res/values/styles.xml"
                };
                InstallFiles(filenames, packagedPathname, assetsPathname);

#if UNITY_6000_0_OR_NEWER
                OverwriteForUnity6(Path.Combine(assetsPathname, buildGradlePath));
                // ModifyAndroidManifest(Path.Combine(assetsPathname, "eos_dependencies.androidlib/AndroidManifest.xml"));
#endif
                WriteConfigMacros(Path.Combine(assetsPathname, buildGradlePath));
            }
        }
        
        private static void OverwriteForUnity6(string gradlePath)
        {
            if (!File.Exists(gradlePath))
            {
                Debug.LogError($"Gradle file not found at path: {gradlePath}");
                return;
            }

            string content = File.ReadAllText(gradlePath);

            // Find the　insert section
            var buildscriptPattern = "jcenter()";
            var matchbuildscript = Regex.Match(content, buildscriptPattern);
            if (!matchbuildscript.Success)
            {
                Debug.LogError("Could not find 'jcenter()' section in gradle file.");
                return;
            }

            var gradlePattern = @"classpath ""com.android.tools.build:([^""]+)""";
            var matchgradle = Regex.Match(content, gradlePattern);
            if (!matchgradle.Success)
            {
                Debug.LogError("Could not find 'com.android.tools.build:' section in gradle file.");
                return;
            }

            var androidPattern = @"android\s*\{";
            var matchNamespace = Regex.Match(content, androidPattern);
            if (!matchNamespace.Success)
            {
                Debug.LogError("Could not find 'android {' section in gradle file.");
                return;
            }
            
            var configPattern = @"defaultConfig\s*\{[^}]*\}";
            var matchConfig = Regex.Match(content, configPattern, RegexOptions.Singleline);
            if (!matchConfig.Success)
            {
                Debug.LogError("Could not find 'defaultConfig' section in gradle file.");
                return;
            }

            var browserPattern = @"implementation\s+'androidx\.browser:browser:1\.4\.0'";
            var matchbrowser = Regex.Match(content, browserPattern);
            if (!matchbrowser.Success)
            {
                Debug.LogError("Could not find 'browser...' section in gradle file.");
                return;
            }

            var appcompatPattern = @"implementation\s+'androidx\.appcompat:appcompat:1\.5\.1'";
            var matchappcompat = Regex.Match(content, appcompatPattern);
            if (!matchappcompat.Success)
            {
                Debug.LogError("Could not find 'appcompat...' section in gradle file.");
                return;
            }

            // Insert text from back to avoid affect on the Index.
            var updatedContent = new System.Text.StringBuilder(content);
            // library
            var insertPoint = matchbrowser.Index + matchbrowser.Length;
            string insertedText = @"        implementation 'androidx.webkit:webkit:1.7.0'";
            updatedContent.Insert(insertPoint, $"\n{insertedText}\n");

            //option and configurations
            insertPoint = matchConfig.Index + matchConfig.Length;
            insertedText = @"
    compileOptions {
        sourceCompatibility JavaVersion.VERSION_17
        targetCompatibility JavaVersion.VERSION_17
    }
    
}";
//     lint {
//         abortOnError false
//     }
// }

// configurations.all {
//     resolutionStrategy {
//         force 'androidx.appcompat:appcompat:1.6.1'
//         force 'androidx.core:core:1.9.0'
//     }";
//             updatedContent.Insert(insertPoint, insertedText);

            // namespace
            insertPoint = matchNamespace.Index + matchNamespace.Length;
            insertedText = @"    namespace ""com.pew.eos_dependencies""";
            updatedContent.Insert(insertPoint, $"\n{insertedText}\n");

            // appcompat ver
            string newAppcompatVersion = "1.6.1";
            string updatedAppcompatLine = $"implementation \'androidx.appcompat:appcompat:{newAppcompatVersion}\'";
            updatedContent.Replace(matchappcompat.Value, updatedAppcompatLine);

            // gradle ver
            string newGradleVersion = "8.3.0";
            string updatedGradleLine = $"classpath \"com.android.tools.build:gradle:{newGradleVersion}\"";
            updatedContent.Replace(matchgradle.Value, updatedGradleLine);
            
            // build script
            updatedContent = updatedContent.Replace("jcenter()", "mavenCentral()");


            // Write back to file
            try
            {
                File.WriteAllText(gradlePath, updatedContent.ToString());
                Debug.Log("Successfully update build.gradle for Unity6000.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to write updated gradle file: {e.Message}");
            }
        }
     
        private static void ModifyAndroidManifest(string manifestPath)
        {
            if (!File.Exists(manifestPath))
            {
                Debug.LogError($"AndroidManifest file not found at path: {manifestPath}");
                return;
            }

            string content = File.ReadAllText(manifestPath);

            // if (!content.Contains("xmlns:tools="))
            // {
            //     content = content.Replace("<manifest ", "<manifest xmlns:tools=\"http://schemas.android.com/tools\" ");
            // }android\s*

            var applicationPattern = @"<application[^>]*"; //android:theme=""@style/Theme\.AppCompat\.Light\.NoActionBar\.FullScreen"""; //""[^>]*>
            var matchApplication = Regex.Match(content, applicationPattern);
            if (!matchApplication.Success)
            {
                Debug.LogError("Could not find application tag in manifest file.");
                return;
            }

            var updatedContent = new System.Text.StringBuilder(content);
            var insertPoint = matchApplication.Index + matchApplication.Length;
            string insertedText = @"
        android:label=""com.pew.eos_dependencies""";


        // <property
        //     android:name=""android.adservices.AD_SERVICES_CONFIG""
        //     android:resource=""@xml/gma_ad_services_config""
        //     tools:replace=""android:resource"" />";

            updatedContent.Insert(insertPoint, insertedText);

            try
            {
                File.WriteAllText(manifestPath, updatedContent.ToString());
                Debug.Log("Successfully modified AndroidManifest for Unity6000.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to write updated AndroidManifest file: {e.Message}");
            }
        }

        private static void ConfigureGradleTemplateProperties()
        {
            string gradleTemplatePathname = Path.Combine(Application.dataPath, "Plugins", "Android", "gradleTemplate.properties");

            File.Delete(gradleTemplatePathname + ".DISABLED");

            if (File.Exists(gradleTemplatePathname))
            {
                if (!DoesGradlePropertiesContainSetting(gradleTemplatePathname, "android.useAndroidX=true"))
                {
                    ReplaceOrSetGradleProperty(gradleTemplatePathname, "android.useAndroidX", "true");
                }
#if !UNITY_6000_0_OR_NEWER
                ReplaceOrSetGradleProperty(gradleTemplatePathname, "android.defaults.buildfeatures.buildconfig", "true");
#endif
                ReplaceOrSetGradleProperty(gradleTemplatePathname, "android.nonTransitiveRClass", "false");
                ReplaceOrSetGradleProperty(gradleTemplatePathname, "android.nonFinalResIds", "false");
            }
            else
            {
                string bundledGradleTemplatePathname = Path.Combine(GetPlatformSpecificAssetsPath("EOS/Android/"), "gradleTemplate.properties");
                File.Copy(bundledGradleTemplatePathname, gradleTemplatePathname);
            }

        #if UNITY_2022_2_OR_NEWER
            DisableGradleProperty(gradleTemplatePathname, "android.enableR8");
        #endif
        }

        private static void ConfigureEOSDependentLibrary()
        {
            string clientIDAsLower = Config.Get<EOSConfig>().clientID.ToLower();

            var pathToEOSValuesConfig = GetAndroidEOSValuesConfigPath();
            var currentEOSValuesConfigAsXML = new System.Xml.XmlDocument();
            currentEOSValuesConfigAsXML.Load(pathToEOSValuesConfig);

            var node = currentEOSValuesConfigAsXML.DocumentElement.SelectSingleNode("/resources");
            if (node == null) { return; }

            var node2 = node.SelectSingleNode("string[@name=\"eos_login_protocol_scheme\"]");
            if (node2 == null) { return; }

            string eosProtocolScheme = node2.InnerText;
            string storedClientID = eosProtocolScheme.Split('.').Last();

            if (storedClientID != clientIDAsLower)
            {
                node2.InnerText = $"eos.{clientIDAsLower}";
                currentEOSValuesConfigAsXML.Save(pathToEOSValuesConfig);
            }
        }

        private static void DetermineLibraryLinkingMethod()
        {
            string packagePath = Path.GetFullPath("Packages/" + EOSPackageInfo.PackageName +
                                                  "/PlatformSpecificAssets~/EOS/Android/");
            string androidAssetFilepath = Application.dataPath + "/../etc/PlatformSpecificAssets/EOS/Android/";

            string pluginSource =
                Directory.Exists(packagePath)
                    ? packagePath
                    : androidAssetFilepath; //From Package or From Assets(EOS Plugin Repo)

            string linkType = Config.Get<AndroidBuildConfig>().DynamicallyLinkEOSLibrary
                ? "dynamic-stdc++/"
                : "static-stdc++/"; //Dynamic or Static       

            string sourcePath = pluginSource + linkType + "aar";
            string destPath = "Assets/Plugins/Android/aar";

            if (Directory.Exists(destPath))
            {
                FileUtil.DeleteFileOrDirectory(destPath);
            }

            FileUtil.CopyFileOrDirectory(sourcePath, destPath);
        }

        private static void CopyFromSourceToPluginFolder_Android(string sourcePath, string filename, string destPath)
        {
            File.Copy(Path.Combine(sourcePath, filename),
                Path.Combine(destPath, filename), true);
        }
    }
}