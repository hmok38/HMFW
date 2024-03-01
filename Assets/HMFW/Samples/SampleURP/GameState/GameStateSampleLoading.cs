using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HMFW.SampleURP.GameState
{
    public class GameStateSampleLoading : HMFW.Core.GameStateBase
    {
        public override async UniTask EnterState(params object[] args)
        {
            Debug.Log($"开始进入{this.GetType()}");
            /*
             * 这里也可以使用一些耗时操作.例如加载某个很大的场景,最好是异步
             * 游戏的整体逻辑不会受到影响,等完成后再关掉之前开启的遮蔽用的UI
             */
            await UniTask.Delay(4000);
            await FW.API.AssetsMgr.LoadSceneAsync("Assets/HMFW/Samples/SampleURP/Scenes/SampleMainScene.unity");
            // FW.API.UIMgr.
            Debug.Log($"进入完成{this.GetType()}");
        }

        public override async UniTask LeaveState(params object[] args)
        {
            Debug.Log($"开始离开{this.GetType()}");
            await UniTask.Delay(1000);

            Debug.Log($"离开完成{this.GetType()}");
        }

        public override void OnUpdate()
        {
            
        }
    }
}