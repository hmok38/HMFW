using Cysharp.Threading.Tasks;
using HMFW.SampleURP.UI;
using UnityEngine;

namespace HMFW.SampleURP.GameState
{
    public class GameStateSampleInit : HMFW.Core.GameStateBase
    {
        public override async UniTask EnterState(params object[] args)
        {
            Debug.Log($"进入{this.GetType()}");
            await FW.UIMgr.OpenUI("UISampleLoading");
            
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