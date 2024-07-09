using System;
using Cysharp.Threading.Tasks;
using FairyGUI;
using HMFW.Core;
using UnityEngine;

namespace HMFW
{
    
    [FGUIResUrl("Home", "UIHome", "Assets/HMFWSampleBundle/Fgui/MainScene/UIHome_fui.bytes", "Home",
        new[] { "Assets/HMFWSampleBundle/Fgui/Common/UICommon_fui.bytes" }, true)]
    public class Home : FairyGUIBase
    {
        public override UISystem MyUISystem => UISystem.FairyGui;

        private void Awake()
        {
            
        }

        public override UniTask OnUIOpen(params object[] args)
        {
            Debug.Log("FguiSampleUI被打开");
            return default;
        }

        public override UniTask OnUIClose(params object[] args)
        {
            Debug.Log("FguiSampleUI被关闭");
            return default;
        }
    }
}