using Cysharp.Threading.Tasks;
using HMFW;

public class FairyGuiSampleInitGameState : HMFW.Core.GameStateBase
{
    public override UniTask EnterState(params object[] args)
    {
        FW.UIMgr = new UIMgrFairyGUI();//替换框架的ui管理器为扩展的ui管理器

        FW.UIMgr.OpenUI("FguiSampleLoadingUI");//打开UGui的loadingUI;
        
        return default;
    }

    public override UniTask LeaveState(params object[] args)
    {
        return default;
    }

    public override void OnUpdate()
    {
    }
}