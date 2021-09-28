using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace HKVR
{
    public class AssetManager
    {
        private static Dictionary<string, Object> _assets = new();

        public static void Initialize()
        {
            LoadAssets();
        }

        private static void LoadAssets()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) continue;

                    var bundle = AssetBundle.LoadFromStream(stream);
                    foreach (var asset in bundle.LoadAllAssets())
                    {
                        if (_assets.ContainsKey(asset.name)) continue;
                        Modding.Logger.Log("Adding asset: " + asset.name);
                        _assets.Add(asset.name, asset);
                    }

                    stream.Dispose();
                }
            }
        }

        public static T FetchAsset<T>(string name) where T : Object
        {
            if (_assets.ContainsKey(name))
            {
                return _assets[name] as T;
            }
            else
            {
                Modding.Logger.LogError($"Asset {name} does not exist.");
                return null;
            }
        }
    }
}
