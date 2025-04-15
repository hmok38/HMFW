using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HMFW.SampleURP.GameState
{
    public class GameStateSampleInit : HMFW.Core.GameStateBase
    {
        public override async UniTask EnterState(params object[] args)
        {
            Debug.Log($"进入{this.GetType()}");
            FW.TimeSyncMgr.SyncTime(); //时间同步管理器
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
            //Ugui设置UI的参数
            FW.UIMgr.UguiOrderInLayer = 8;
            FW.UIMgr.UguiOrderInLayer = 101;
            FW.UIMgr.UguiRenderMode = RenderMode.ScreenSpaceOverlay;
            
            var setting = FW.UIMgr.GetGroupSetting(200); //获取组设置,
            setting.BusyLimit = 0; //设置这一组是否限制最大显示的ui数,多余的会等待,
            await FW.UIMgr.OpenUI("UISampleLoading", 200, UIOpenType.Wait, Color.gray);

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