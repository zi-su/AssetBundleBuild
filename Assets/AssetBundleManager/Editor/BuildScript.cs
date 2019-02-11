using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
namespace AssetBundles
{
    public class BuildScript
    {
        public static string overloadedDevelopmentServerURL = "";

        static public string CreateAssetBundleDirectory()
        {
            // Choose the output path according to the build target.
            string outputPath = Path.Combine("StreamingAssets", "Windows");
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            return outputPath;
        }

        [System.Serializable]
        public struct AssetBundleData
        {
            public string bundleName;
            public string[] assetPaths;
        }

        public struct JsonAssetBundleData
        {
            public AssetBundleData[] data;
        }

        public static void BuildAssetBundles()
        {
            List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
            var jsonPath = Path.Combine(Application.dataPath, "jsonBundle.json");
            StreamReader reader = new StreamReader(jsonPath);
            var jsonText = reader.ReadToEnd();
            reader.Close();
            JsonAssetBundleData jsonData = JsonUtility.FromJson<JsonAssetBundleData>(jsonText);
            foreach(var item in jsonData.data)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = item.bundleName;
                foreach(var path in item.assetPaths)
                {
                    string[] searchFolder = { path };
                    var guids = AssetDatabase.FindAssets("t:Object", searchFolder);
                    List<string> assetNameList = new List<string>();
                    foreach(var guid in guids)
                    {
                        var assetName = AssetDatabase.GUIDToAssetPath(guid);
                        if (assetNameList.Exists( l => { return string.Equals(l, assetName); } ))
                        {
                            continue;
                        }
                        assetNameList.Add(assetName);
                    }
                    build.assetNames = assetNameList.ToArray();
                }
                buildList.Add(build);
            }
            AssetBundleBuild[] buildArray = buildList.ToArray();
            BuildAssetBundles(buildArray);
        }

        public static void BuildAssetBundles(AssetBundleBuild[] builds)
        {
            // Choose the output path according to the build target.
            string outputPath = CreateAssetBundleDirectory();

            var options = BuildAssetBundleOptions.None;

            if (builds == null || builds.Length == 0)
            {
                //@TODO: use append hash... (Make sure pipeline works correctly with it.)
                BuildPipeline.BuildAssetBundles(outputPath, options, EditorUserBuildSettings.activeBuildTarget);
            }
            else
            {
                BuildPipeline.BuildAssetBundles(outputPath, builds, options, EditorUserBuildSettings.activeBuildTarget);
            }
        }
    }
}
