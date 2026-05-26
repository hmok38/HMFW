using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;


namespace HMFW
{
    public class FguiHelper
    {
        public readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        public FguiHelper()
        {
            //下面两种资源采用自定义卸载方式,主要是fgui只提供了这两种
            NTexture.CustomDestroyMethod += CustomUnloadTexture;
            NAudioClip.CustomDestroyMethod += CustomUnloadAudioClip;
        }

        /// <summary>
        /// 通过fgui的图片资源生成sprite,可以用来给unity其他系统使用,默认锚点在中心点
        /// </summary>
        /// <param name="pkgName"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public async UniTask<Sprite> CreatSpriteFromFguiAsset(string pkgName, string spriteName)
        {
            return await CreatSpriteFromFguiAsset(pkgName, spriteName, Vector2.one * 0.5f);
        }

        /// <summary>
        /// 通过fgui的图片资源生成sprite,可以用来给unity其他系统使用
        /// </summary>
        /// <param name="pkgName"></param>
        /// <param name="spriteName"></param>
        /// <param name="spritePivot">自定义的锚点,注意,最大只能是 v2(1,1),x从左到右,y是从下到上 </param>
        /// <returns></returns>
        public async UniTask<Sprite> CreatSpriteFromFguiAsset(string pkgName, string spriteName,
            Vector2 spritePivot)
        {
            var spriteNameTmp = $"ui://{pkgName}/{spriteName}:{spritePivot.x}:{spritePivot.y}";
            if (Sprites.TryGetValue(spriteNameTmp, out var tempSprite))
            {
                if (tempSprite != null && tempSprite.texture != null)
                {
                    return tempSprite;
                }
            }

            var nObj = UIPackage.GetItemAsset(pkgName, spriteName);
            if (nObj == null)
            {
                Debug.LogError($"获取fgui资源时,获取失败 {pkgName} spriteName:{spriteName},请确保资源存在且包已经被加载");
                return null;
            }

            if (nObj is not NTexture)
            {
                Debug.LogError($"获取fgui资源时,资源类型错误 {pkgName} spriteName:{spriteName},请确保这个资源是图片");
                return null;
            }

            NTexture nTexture = nObj as NTexture;
            await UniTask.WaitUntil(() => nTexture.nativeTexture != null);

            var region = typeof(NTexture).GetField("_region", BindingFlags.Instance | BindingFlags.NonPublic);
            Rect rect = (Rect)region.GetValue(nTexture);
            Rect realRect = new Rect(rect.x, nTexture.nativeTexture.height - rect.height - rect.y, rect.width,
                rect.height);
            Sprite sprite = Sprite.Create(nTexture.nativeTexture as Texture2D, realRect,
                spritePivot, 100, 0, SpriteMeshType.FullRect);
            sprite.name = spriteName;
            Sprites[spriteNameTmp] = sprite;
            return sprite;
        }

        /// <summary>
        /// 加载包,路径只需要到包名为止,后面的 _fui.bytes 需要去掉
        /// </summary>
        /// <param name="packagePath"></param>
        public async UniTask LoadPackage(string packagePath)
        {
            var pkgName = GetPkgNameByPkgPath(packagePath);
            if (UIPackage.GetPackages().FindIndex(x => x.name.Equals(pkgName)) >= 0)
            {
                return;
            }

            var descDataAssetUIHome =
                await FW.AssetsMgr.LoadAsync<TextAsset>($"{packagePath}_fui.bytes");
            var descData = descDataAssetUIHome.bytes;
            UIPackage.AddPackage(descData, packagePath, OnLoadResourceAsync);
        }

        private async void OnLoadResourceAsync(string name, string extension, Type type, PackageItem item)
        {
            if (item == null || item.owner == null)
            {
                return;
            }
#if UNITY_EDITOR
            // 在编辑中，模拟延迟加载
            await UniTask.DelayFrame(10);
#endif
            var obj = await FW.AssetsMgr.LoadAsync<UnityEngine.Object>(name + extension);

            var method = DestroyMethod.None;

            switch (item.type)
            {
                //如果是下面的资源,采用自定义的unload方式从资源管理器中去掉
                case PackageItemType.Atlas:
                case PackageItemType.Sound:
                    method = DestroyMethod.Custom;

                    break;
            }

            if (method != DestroyMethod.None)
            {
                await UniTask.DelayFrame(1); //因为fgui中的代码Bug,必须等待一帧,否则设置的销毁方式会被重置为DestroyMethod.None
            }

            item.owner.SetItemAsset(item, obj, method);
        }

        /// <summary>
        /// 卸载包
        /// </summary>
        /// <param name="packageName"></param>
        public void UnloadPackage(string packageName)
        {
            UIPackage.RemovePackage(packageName);
        }


        private void CustomUnloadTexture(Texture texture)
        {
            if (texture == null) return;
            FW.AssetsMgr.ReleaseRes(texture);
        }

        private void CustomUnloadAudioClip(AudioClip audioClip)
        {
            if (audioClip == null) return;
            FW.AssetsMgr.ReleaseRes(audioClip);
        }

        /// <summary>
        /// 根据资源路径获得包名
        /// </summary>
        /// <param name="pkgPath">加载包,路径只需要到包名为止,后面的 _fui.bytes 需要去掉</param>
        /// <returns></returns>
        public string GetPkgNameByPkgPath(string pkgPath)
        {
            return Path.GetFileName(pkgPath);
        }

        /// <summary>
        /// 发出加载某个包全部资源的指令,因为是异步的,如果需要直接使用资源,请检查是否加载完成
        /// </summary>
        /// <param name="pkgNameOrPkgPath"></param>
        public void PkgLoadAllAssets(string pkgNameOrPkgPath)
        {
            var pkn = GetPkgNameByPkgPath(pkgNameOrPkgPath);
            var pkg = UIPackage.GetByName(pkn);
            if (pkg == null)
            {
                Debug.LogError($"{pkgNameOrPkgPath} 包,不存在,请先加载它,或检查名字");
            }
            else
            {
                pkg.LoadAllAssets();
            }
        }

        /// <summary>
        /// 检查这个包的所有资源是否都加载完毕的,主要是配合上面的 PkgLoadAllAssets 的接口就行判断
        /// </summary> 
        /// <param name="pkgNameOrPkgPath"></param>
        /// <returns></returns>
        public bool CheckPkgAllAssetsLoaded(string pkgNameOrPkgPath)
        {
            var pkn = GetPkgNameByPkgPath(pkgNameOrPkgPath);
            var pkg = UIPackage.GetByName(pkn);
            if (pkg == null)
            {
                Debug.LogError($"{pkgNameOrPkgPath} 包,不存在,请先加载它,或检查名字");
                return false;
            }

            var items = pkg.GetItems();
            if (items == null)
            {
                Debug.LogError($"{pkgNameOrPkgPath} 包的items列表为空");
                return false;
            }

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    Debug.LogError($"{pkgNameOrPkgPath} 包的第 {i} 个item不存在");
                    return false;
                }

                switch (item.type)
                {
                    case PackageItemType.Image:
                    case PackageItemType.Atlas:
                        if (item.texture == null)
                            return false;
                        break;

                    case PackageItemType.Sound:
                        if (item.audioClip == null)
                            return false;
                        break;

                    case PackageItemType.Font:
                        if (item.bitmapFont == null)
                            return false;
                        break;

                    case PackageItemType.MovieClip:
                        if (item.frames == null)
                            return false;
                        break;

                    case PackageItemType.Component:
                        break; //一定有

                    case PackageItemType.Misc:
                        break; //音频不管

                    case PackageItemType.Spine:
                    case PackageItemType.DragoneBones:
                        if (item.skeletonAsset == null)
                            return false;
                        break;
                }
            }

            return true;
        }

        // ~FguiHelper()
        // {
        //     if (Sprites != null)
        //     {
        //         foreach ((string k, Sprite v) in Sprites)
        //         {
        //             if (v != null)
        //             {
        //                 Object.DestroyImmediate(v);
        //             }
        //         }
        //
        //         Sprites.Clear();
        //     }
        // }
    }

    public static class FguiHelperExtension
    {
        private static FguiHelper _fguiHelper;

        public static FguiHelper FguiHelper(this FW fw)
        {
            if (_fguiHelper == null)
            {
                _fguiHelper = new FguiHelper();
            }

            return _fguiHelper;
        }
    }
}