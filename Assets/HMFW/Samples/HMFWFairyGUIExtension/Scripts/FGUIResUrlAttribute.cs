using System;

namespace HMFW
{
    /// <summary>
    /// FairyGUI需要的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FGUIResUrlAttribute : Attribute
    {
        public readonly string PackageFileUrl;
        public readonly string UIAlias;
        public readonly string[] DependencyPackagesFileUrl;
        public readonly string UIName;
        public readonly string PackageName;
        public readonly bool BeFairyBatching;
        /// <summary>
        /// 创建FairyGUI的UI信息特性
        /// </summary>
        /// <param name="uIName">FGUI的UI名(组件名)</param>
        /// <param name="packageName">FGUI的包名</param>
        /// <param name="packageFileUrl">FGUI的包文件所在加载路径(AA资源包加载路径)只要到包名即可,后面的_Fgui.bytes不用</param>
        /// <param name="uiAlias">这个UI的别名,可以通过完整类名或者别名打开这个UI</param>
        /// <param name="dependencyPackagesFileUrl">依赖的包文件所在,FGUI不处理包依赖,需要自己处理包依赖</param>
        /// <param name="beFairyBatching">是否需要开启合批</param>
        public FGUIResUrlAttribute(string uIName, string packageName, string packageFileUrl, string uiAlias = null,
            string[] dependencyPackagesFileUrl = null,
            bool beFairyBatching = false)
        {
            this.PackageFileUrl = packageFileUrl;
            this.UIAlias = uiAlias;
            this.DependencyPackagesFileUrl = dependencyPackagesFileUrl;
            this.BeFairyBatching = beFairyBatching;
            this.UIName = uIName;
            this.PackageName = packageName;
        }
    }
}