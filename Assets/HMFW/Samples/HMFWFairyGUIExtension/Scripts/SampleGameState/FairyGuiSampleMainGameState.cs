using Cysharp.Threading.Tasks;

namespace HMFW
{
    public class FairyGuiSampleMainGameState : HMFW.Core.GameStateBase
    {
        public override async UniTask EnterState(params object[] args)
        {
            await FW.AssetsMgr.LoadSceneAsync(
                "Assets/HMFW/Samples/HMFWFairyGUIExtension/Scenes/FairyGuiSampleMain.unity");

            //打开别名为Home的FgUI,其脚本为 Assets/HMFW/Samples/HMFWFairyGUIExtension/Scripts/UI/Home.cs
            //别名的定义也在脚本上
            await FW.UIMgr.OpenUI("Home", 201);
            //打开FguiSample.cs的Fgui
            await FW.UIMgr.OpenUI("FguiSample", 200);

            FW.UIMgr.CloseUI("FguiSampleLoadingUI"); //关闭ui
        }

        public override UniTask LeaveState(params object[] args)
        {
            return default;
        }

        public override void OnUpdate()
        {
        }
    }
}