using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace HMFW.Editor
{
    public static class CopySampleBundle
    {
        [DidReloadScripts]
        public static async void CopyToAssets()
        {
            var allSo = SampleBundleSo.GetAllSo();
            bool v = false;
            foreach (var sampleBundleSo in allSo)
            {
                var beCopy = SoHandle(sampleBundleSo);
                if (beCopy) v = true;
            }
            
            if (v)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                await UniTask.DelayFrame(2);
                AssetDatabase.Refresh();
            }
        }

        private static bool SoHandle(SampleBundleSo bundleSo)
        {
            var assets = bundleSo.needCopyAssets;
            bool v = false;
            foreach (var asset in assets)
            {
                var beCopy = CopyToPath(asset);
                if (beCopy) v = true;
            }

            return v;
        }

        private static bool CopyToPath(Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            var pathP = path.Split("/Bundle/");
            if (pathP.Length < 2)
            {
                pathP = path.Split("\\Bundle\\");
            }

            if (pathP.Length < 2)
            {
                Debug.LogError($"资源无法分析路径,必须放入带有/Bundle/的路径下 {path}");
                return false;
            }

            var targetPath = Path.Combine(SampleBundleSo.SampleBundleBasePath, pathP[1]);
            // Debug.Log($"目标路径: {targetPath} 资源路径: {path}");
            if (File.Exists(targetPath))
            {
                //有这个文件
                return false;
            }

            FileInfo fileInfo = new FileInfo(targetPath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }


            //开始拷贝
            var suc = AssetDatabase.CopyAsset(path, targetPath);

            Debug.Log($"目标路径: {targetPath} 资源路径: {path} copy成功:{suc}");
            return true;
        }
    }
}