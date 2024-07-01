using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HMFW.SampleURP.GameState
{
    public class GameStateSampleInit : HMFW.Core.GameStateBase
    {
        public override async UniTask EnterState(params object[] args)
        {
            Debug.Log($"进入{this.GetType()}");
            FW.UIMgr = new UIMgrFairyGUI();
            FW.UIMgr.Init();
            var setting = FW.UIMgr.GetGroupSetting(200);
            
            setting.BusyLimit = 0;
            //   await FW.UIMgr.OpenUI("UISampleLoading",1000);
            await FW.UIMgr.OpenUI("UISampleLoading", 200, UIOpenType.Wait, Color.red);
          
            // await UniTask.Delay(300);
            // await FW.UIMgr.OpenUI("PlayerCoin", 200);
            //
            // await UniTask.Delay(300);
            // await FW.UIMgr.OpenUI("UISampleLoading", 200);
            // await UniTask.Delay(3000);
            //
            // // await FW.UIMgr.CloseUI("PlayerCoin");
            // //await UniTask.Delay(2000);
            // //await FW.UIMgr.CloseUI("UISampleLoading");
            // Debug.Log("开始关闭");
            // await FW.UIMgr.CloseAllUI(UICloseType.Waiting, new []{"PlayerCoin"});
            //
            // // await UniTask.Delay(1000);
            // // await FW.UIMgr.CloseUI("PlayerCoin");
            //  await UniTask.Delay(1000);
            //  await FW.UIMgr.CloseUI("UISampleLoading");
            //  await UniTask.Delay(1000);
            Debug.Log("完成关闭");
        }

        public override async UniTask LeaveState(params object[] args)
        {
            Debug.Log($"开始离开{this.GetType()}");
            await UniTask.Delay(1000); //这里可以做一些需要时间的操作,例如加载UI,打开UI等,状态机会等待完成后再走下一步流程,不会影响到切换的流程;
            Debug.Log($"离开完成{this.GetType()}");
        }

        public override void OnUpdate()
        {
            /*
             *  update可以保证在EnterState/LeaveState没有完成之前不调用
             */
            //Debug.Log($"{this.GetType()} OnUpdate");
            // FW.GameFsmMgr.ChangeState<GameStateSampleLoading>();
        }
    }
}