using Cysharp.Threading.Tasks;
using HMFW;
using UnityEngine;

public class FairyGuiSampleInitGameState : HMFW.Core.GameStateBase
{
    public override UniTask EnterState(params object[] args)
    {
        FW.BackBtnQueueMgr.AddToQueue(this, (ob) =>
        {
            Debug.Log($"测试返回键队列1-调用后不会删除自己");
            return false;
        });
            
        FW.BackBtnQueueMgr.AddToQueue(this, (ob) =>
        {
            Debug.Log($"测试返回键队列2-调用后会删除自己");
            return true;
        });
        
        FW.UIMgr = new UIMgrFairyGUI();//替换框架的ui管理器为扩展的ui管理器

        FW.UIMgr.OpenUI("FguiSampleLoadingUI",200);//打开UGui的loadingUI;
        
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