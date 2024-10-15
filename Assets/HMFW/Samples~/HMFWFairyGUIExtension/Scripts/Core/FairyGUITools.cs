using System.Reflection;
using FairyGUI;
using UnityEngine;

namespace HMFW
{
    public static class FGUITools
    {
        /// <summary>
        /// 通过fgui的图片资源生成sprite,可以用来给unity其他系统使用,默认锚点在中心点
        /// </summary>
        /// <param name="pkgName"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public static Sprite CreatSpriteFromFguiAsset(string pkgName, string spriteName)
        {
            return CreatSpriteFromFguiAsset(pkgName, spriteName, Vector2.one * 0.5f);
        }

        /// <summary>
        /// 通过fgui的图片资源生成sprite,可以用来给unity其他系统使用
        /// </summary>
        /// <param name="pkgName"></param>
        /// <param name="spriteName"></param>
        /// <param name="spritePivot">自定义的锚点</param>
        /// <returns></returns>
        public static Sprite CreatSpriteFromFguiAsset(string pkgName, string spriteName, Vector2 spritePivot)
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
            var region = typeof(NTexture).GetField("_region", BindingFlags.Instance | BindingFlags.NonPublic);
            Rect rect = (Rect)region.GetValue(nTexture);
            Rect realRect = new Rect(rect.x, nTexture.nativeTexture.height - rect.height - rect.y, rect.width,
                rect.height);
            Sprite sprite = Sprite.Create(nTexture.nativeTexture as Texture2D, realRect,
                Vector2.zero);
            return sprite;
        }
    }
}