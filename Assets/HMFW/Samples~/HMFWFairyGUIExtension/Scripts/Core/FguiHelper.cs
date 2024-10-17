﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;

namespace HMFW
{
    public class FguiHelper
    {
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
                spritePivot);
            sprite.name = spriteName;
            return sprite;
        }
        
        /// <summary>
        /// 加载包,路径只需要到包名为止,后面的 _fui.bytes 需要去掉
        /// </summary>
        /// <param name="packagePath"></param>
        public async UniTask LoadPackage(string packagePath)
        {
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

            var obj = await FW.AssetsMgr.LoadAsync<UnityEngine.Object>(item.file);

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