using UnityEngine;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs
{
    public class GameStateSceneTestWithSubScene : UnityEngine.MonoBehaviour
    {
        [Header("此脚本为编辑器中不通过初始化直接运行某个场景的脚本")]
        [Header("这个状态名为直接调用指定GameState的类名")]
        [Header("进入PlayMode后,会直接进入这个GameState的EnterState函数")]
        [Header("这个脚本是为场景中具有ECS SubScene专用")]
        public string gameStateName = "";

#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP_RUNTIME_WORLD || UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
        [Header("__________________________________________")]
        [Header("是否需要重新加载场景中的子场景,因为子场景中SubScene会在初始化世界之前被加载,所以需要重新加载一次")]
        public bool needLoadReLoadSubScene = true;
#endif
#if UNITY_EDITOR
        private void Start()
        {
            if (string.IsNullOrEmpty(gameStateName)) return;

            if (!FW.GameFsmMgr.CheckCurrentState(gameStateName))
            {
                FW.GameFsmMgr.ChangeState(gameStateName, "编辑器测试");

                //系统不自动创建默认世界的情况下,就需要重新reload存在场景的subScene
#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP_RUNTIME_WORLD || UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
                if (needLoadReLoadSubScene)
                {
                    var subScenes =
                        UnityEngine.GameObject.FindObjectsOfType(typeof(Unity.Scenes.SubScene)) as Unity.Scenes.SubScene
                            [];
                    for (var i = 0; i < subScenes.Length; i++)
                    {
                        var subScenesObj = subScenes[i];
                        Unity.Scenes.SceneSystem.LoadSceneAsync(
                            Unity.Entities.World.DefaultGameObjectInjectionWorld.Unmanaged,
                            new Unity.Entities.Serialization.EntitySceneReference(subScenesObj.SceneAsset));
                    }
                }

#endif
            }
        }
#endif
    }
}