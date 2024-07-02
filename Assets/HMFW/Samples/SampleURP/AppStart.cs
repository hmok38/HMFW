﻿using HMFW.SampleURP.GameState;
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
            //FW.GameFsmMgr = new GameObject("GM").AddComponent<GameFsmMgr>();


            FW.GameFsmMgr.ChangeState<GameStateSampleInit>();//这是访问框架自带模块的方式
            FW.CustomAPI.FWTestMgr();//这是访问自定义扩展模块的方式
            
            
            /*
             * 注意:首个场景中的其他脚本尽量不要在Awake中调用框架的逻辑和接口,
             * 因为不能保证这个脚本的awake是第一个被调用,
             * 也就是说不能保证GameStateInit已经被执行,
             * 而大部分的框架内容会在GameStateInit内进行初始化
             */


            //---GData模块使用演示
            //GDataDemo();
        }


        /// <summary>
        /// GData模块
        /// </summary>
        private void GDataDemo()
        {
            FW.GDataMgr.AddListener(EnumA.A, () =>
            {
                Debug.Log(EnumA.A);
                Debug.Log(FW.GDataMgr.GetData<Vector3>(EnumA.A));
            });
            FW.GDataMgr.SetData(EnumA.A, new Vector3(1, 1, 1));

            FW.GDataMgr.HasData(EnumA.A);
            FW.GDataMgr.AddListener(EnumA.A, () =>
            {
                Debug.Log(EnumA.A + " 1");
                Debug.Log(FW.GDataMgr.GetData<Vector3>(EnumA.A, 1));
            }, 1);


            FW.GDataMgr.SetData(EnumA.A, new Vector3(2, 2, 2), 1); //相同数据的不同副本


            FW.GDataMgr.AddListener(EnumA.B, () =>
            {
                Debug.Log(EnumA.B);
                Debug.Log(FW.GDataMgr.GetData<Vector2>(EnumA.B));
            });
            FW.GDataMgr.SetData(EnumA.B, new Vector2(1, 1));

            FW.GDataMgr.AddListener(EnumA.C, () =>
            {
                Debug.Log(EnumA.C);
                Debug.Log(FW.GDataMgr.GetData<int>(EnumA.C));
            });
            FW.GDataMgr.SetData(EnumA.C, 3);

            FW.GDataMgr.AddListener(EnumB.A, () =>
            {
                Debug.Log(EnumB.A);
                Debug.Log(FW.GDataMgr.GetData<AppStart>(EnumB.A));
            });
            FW.GDataMgr.SetData(EnumB.A, this);

            FW.GDataMgr.AddListener(EnumB.B, () =>
            {
                Debug.Log(EnumB.B);
                Debug.Log(FW.GDataMgr.GetData<string>(EnumB.B));
            });
            FW.GDataMgr.SetData(EnumB.B, "你好");
            FW.GDataMgr.SetData(EnumB.C, 1.34f);


            var v0 = FW.GDataMgr.GetData<Vector3>(EnumA.A);
            var v01 = FW.GDataMgr.GetData<Vector3>(EnumA.A, 1);
            var v1 = FW.GDataMgr.GetData<Vector2>(EnumA.B);
            var v2 = FW.GDataMgr.GetData<int>(EnumA.C);
            var v3 = FW.GDataMgr.GetData<AppStart>(EnumB.A);
            var v4 = FW.GDataMgr.GetData<string>(EnumB.B);
            var v5 = FW.GDataMgr.GetData<float>(EnumB.C);

            FW.GDataMgr.SetData(EnumB.B, "你好!");

            FW.GDataMgr.RemoveData(EnumB.B, 0);
            v4 = FW.GDataMgr.GetData<string>(EnumB.B);

            FW.GDataMgr.RemoveAllTypeData(EnumA.A);
            v0 = FW.GDataMgr.GetData<Vector3>(EnumA.A);
            v01 = FW.GDataMgr.GetData<Vector3>(EnumA.A, 1);

            FW.GDataMgr.RemoveAllDataOnMgr();
            v3 = FW.GDataMgr.GetData<AppStart>(EnumB.A);
        }
    }

    public enum EnumA
    {
        A = 0,
        B,
        C
    }

    public enum EnumB
    {
        A = 0,
        B,
        C
    }
}