using System;
using Cysharp.Threading.Tasks;
using HMFW;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class FairyGuiSampleInitGameState : HMFW.Core.GameStateBase
{
    public override UniTask EnterState(params object[] args)
    {
        FW.UIMgr = new UIMgrFairyGUI();
        if (!FW.UIMgr.SetUITypeListJson(CreateSampleUITypeListJson()))
        {
            return default;
        }

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
        
        FW.GEventMgr.Add(FW.UIMgr.UiPreOpenEvent, (string uiName,uint g) =>
        {
            Debug.Log($"UI {uiName} 准备打开 优先级:{g}");
        });
        FW.GEventMgr.Add(FW.UIMgr.UiOpenedEvent, (string uiName,uint g) =>
        {
            Debug.Log($"UI {uiName} 被打开 优先级:{g}");
        });
        FW.GEventMgr.Add(FW.UIMgr.UiCloseEvent, (string uiName,uint g) =>
        {
            Debug.Log($"UI {uiName} 被关闭 优先级:{g}");
        });
        FW.UIMgr.OpenUI("FguiSampleLoadingUI",200);//打开UGui的loadingUI;
        
        return default;
    }

    private static string CreateSampleUITypeListJson()
    {
        var types = new JArray(
            typeof(FguiSampleLoadingUI).AssemblyQualifiedName,
            typeof(FguiSample).AssemblyQualifiedName,
            typeof(Home).AssemblyQualifiedName);

        // 两个示例可以单独导入。共同使用时，编辑器校对的列表也包含 SampleURP 的 UI。
        var sampleUrpTypes = new[]
        {
            "HMFW.SampleURP.UI.UISampleLoading, HMFW.SampleURP",
            "HMFW.SampleURP.UI.UISamplePlayerCoinInfo, HMFW.SampleURP"
        };
        foreach (var name in sampleUrpTypes)
        {
            if (Type.GetType(name, false) != null) types.Add(name);
        }

        return types.ToString(Formatting.None);
    }

    public override UniTask LeaveState(params object[] args)
    {
        return default;
    }

    public override void OnUpdate()
    {
    }
}