using System;

namespace HMFW.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UGUIResUrlAttribute : Attribute
    {
        public readonly string UILoadUrl;
        public readonly string[] PreloadResUrl;
        public readonly string UIAlias;

        /// <summary>
        /// 设置UI的加载路径和需要预加载资源列表,如果需要接入多国语言的话,请使用[L]标识代替语言路径.
        /// 如"Assets/UI/ChineseSimplified/LoadingUI.prefab"替换成"Assets/UI/[L]/LoadingUI.prefab",
        /// 此UI不需区分多国语言则不需要使用[L]标识
        /// </summary>
        /// <param name="uiLoadUrl">加载预制体的路径</param>
        /// <param name="uiAlias">为开启ui的别名</param>
        /// <param name="preloadResUrl">开启UI前需要预加载的资源</param>
        public UGUIResUrlAttribute(string uiLoadUrl, string uiAlias = null, string[] preloadResUrl = null)
        {
            this.UILoadUrl = uiLoadUrl;
            this.UIAlias = uiAlias;
            if (preloadResUrl != null)
            {
                this.PreloadResUrl = new string[preloadResUrl.Length + 1];
                this.PreloadResUrl[0] = this.UILoadUrl;
                for (var i = 1; i < this.PreloadResUrl.Length; i++)
                {
                    this.PreloadResUrl[i] = preloadResUrl[i - 1];
                }
            }
            else
            {
                this.PreloadResUrl = new[] {this.UILoadUrl};
            }
        }
    }
}