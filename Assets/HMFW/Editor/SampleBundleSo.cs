using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HMFW.Editor
{
    [CreateAssetMenu(fileName = "SampleBundleSo", menuName = "HMFW/SampleBundleSo")]
    public class SampleBundleSo : ScriptableObject
    {
        public static string SampleBundleBasePath = "Assets/HMFWSampleBundle";
        public Object[] needCopyAssets;

        public static SampleBundleSo[] GetAllSo()
        {
            var allGuids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(SampleBundleSo).Name}");
            return allGuids.Select(guid =>
                AssetDatabase.LoadAssetAtPath<SampleBundleSo>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
        }
    }
}