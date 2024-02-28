using System;
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


            FW.API.GameFsmMgr.ChangeState<GameStateInit>();
            
            /*
             * 注意:首个场景中的其他脚本尽量不要在Awake中调用框架的逻辑和接口,
             * 因为不能保证这个脚本的awake是第一个被调用,
             * 也就是说不能保证GameStateInit已经被执行,
             * 而大部分的框架内容会在GameStateInit内进行初始化
             */
        }
    }
}