using System;

namespace HMFW.Core
{
    /// <summary>
    /// UGUI需要的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public abstract class UIAttribute : Attribute
    {
        public readonly bool BeMultiple;
        public readonly string UIAlias;
        public readonly UISystem UISystem;

        public UIAttribute(string uiAlias, UISystem uiSystem, bool beMultiple = false)
        {
            BeMultiple = beMultiple;
            UIAlias = uiAlias;
            UISystem = uiSystem;
        }
    }

    /// <summary>
    /// UGUI需要的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UGUIResAttribute : UIAttribute
    {
        public readonly string UILoadUrl;
        public readonly string[] PreloadResUrl;


        /// <summary>
        /// 设置UI的加载路径和需要预加载资源列表,如果需要接入多国语言的话,请使用[L]标识代替语言路径.
        /// 如"Assets/UI/ChineseSimplified/LoadingUI.prefab"替换成"Assets/UI/[L]/LoadingUI.prefab",
        /// 此UI不需区分多国语言则不需要使用[L]标识
        /// </summary>
        /// <param name="uiLoadUrl">加载预制体的路径</param>
        /// <param name="uiAlias">为开启ui的别名</param>
        /// <param name="beMultiple">是否允许多个实例同时存在</param>
        /// <param name="preloadResUrl">开启UI前需要预加载的资源</param>
        public UGUIResAttribute(string uiLoadUrl, string uiAlias, bool beMultiple = false,
            string[] preloadResUrl = null) : base(uiAlias, UISystem.UGUI, beMultiple)
        {
            this.UILoadUrl = uiLoadUrl;
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
                this.PreloadResUrl = new[] { this.UILoadUrl };
            }
        }
    }
}