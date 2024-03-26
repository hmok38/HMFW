using HMFW.SampleURP.GameState;
using UnityEngine;

namespace HMFW.SampleURP
{
    public class AppStart : MonoBehaviour
    {
        private void Awake()
        {
            /*
             * 如果想用自定义的FSM管理游戏逻辑,那么再使用GameFsmMgr之前就可以定义一个新的,但必须是继承GameFsmMgrBase的
             * 如果不想换的话,就不用自己定义和赋值(下面这一句),直接使用就可以了
             */
            //FW.API.GameFsmMgr = new GameObject("GM").AddComponent<GameFsmMgr>();


            FW.API.GameFsmMgr.ChangeState<GameStateSampleInit>();

            /*
             * 注意:首个场景中的其他脚本尽量不要在Awake中调用框架的逻辑和接口,
             * 因为不能保证这个脚本的awake是第一个被调用,
             * 也就是说不能保证GameStateInit已经被执行,
             * 而大部分的框架内容会在GameStateInit内进行初始化
             */


            // FW.API.GDataMgr.SetData(0, new Vector3(1, 1, 1));
            // FW.API.GDataMgr.SetData(0, new Vector3(2, 2, 2), 1); //相同数据的不同副本
            // FW.API.GDataMgr.SetData(1, new Vector2(1, 1));
            // FW.API.GDataMgr.SetData(2, 3);
            // FW.API.GDataMgr.SetData(3, this);
            // FW.API.GDataMgr.SetData(4, "你好");
            //
            //
            // var v0 = FW.API.GDataMgr.GetData<Vector3>(0);
            // var v01 = FW.API.GDataMgr.GetData<Vector3>(0, 1);
            // var v1 = FW.API.GDataMgr.GetData<Vector2>(1);
            // var v2 = FW.API.GDataMgr.GetData<int>(2);
            // var v3 = FW.API.GDataMgr.GetData<AppStart>(3);
            // var v4 = FW.API.GDataMgr.GetData<string>(4);
        }
    }
}