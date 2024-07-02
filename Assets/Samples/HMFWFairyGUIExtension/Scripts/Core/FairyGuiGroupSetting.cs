using FairyGUI;
using UnityEngine;

namespace HMFW
{
    /// <summary>
    /// 继承UIGroupSetting,用于FairyGui扩展用的组设置
    /// </summary>
    public class FairyGuiGroupSetting : UIGroupSetting
    {
        /// <summary>
        /// fgui的每个UI组的根节点
        /// </summary>
        public Transform FguiGroupRoot;
        /// <summary>
        ///  fgui的每个UI组的根节点GComponent
        /// </summary>
        public GComponent FguiGroupRootGComponent;
    }
}