using HMFW.Core;
using UnityEngine;

namespace HMFW
{
    /// <summary>
    /// FairyGUI的基类,其继承于HMFW.Core.UIBase
    /// </summary>
    public abstract class FairyGUIBase : HMFW.Core.UIBase
    {
        public override UISystem MyUISystem => UISystem.FairyGui;

        /// <summary>
        /// 此UI的根节点
        /// </summary>
        [System.NonSerialized, Header("此UI的根节点")]
        public FairyGUI.GObject MyGObject;
    }
}