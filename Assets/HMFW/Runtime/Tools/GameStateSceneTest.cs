using UnityEngine;

// ReSharper disable once CheckNamespace
namespace HMFW.Tools
{
    public class GameStateSceneTest : UnityEngine.MonoBehaviour
    {
        [Header("此脚本为编辑器中不通过初始化直接运行某个场景的脚本")]
        [Header("这个状态名为直接调用指定GameState的类名")]
        [Header("进入PlayMode后,会直接进入这个GameState的EnterState函数")]
        public string gameStateName = "";

#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP_RUNTIME_WORLD || UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
        [Header("__________________________________________")]
        [Header("注意:请在具有subScene的场景中使用 GameStateSceneTestWithSubScene脚本,其在HMFW包的HMFWEcsExtension示例中")]
        public bool needLoadReLoadSubScene = true;
#endif
#if UNITY_EDITOR
        private void Start()
        {
            if (string.IsNullOrEmpty(gameStateName)) return;


            if (!FW.GameFsmMgr.CheckCurrentState(gameStateName))
            {
                FW.GameFsmMgr.ChangeState(gameStateName, "编辑器测试");
            }
        }
#endif
    }
}