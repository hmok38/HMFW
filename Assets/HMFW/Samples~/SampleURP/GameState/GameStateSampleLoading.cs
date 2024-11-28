using System.Collections.Generic;
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

            await FW.AssetsMgr.LoadSceneAsync("Assets/HMFWSampleBundle/Scenes/SampleMainScene.unity");

            await UniTask.NextFrame(); //等待一帧

            await FW.UIMgr.OpenUI("PlayerCoin", 201);

            await FW.UIMgr.CloseUI("UISampleLoading"); //慢慢关闭ui

            //打开一个黑色的,因为他的优先级为200,虽然跟上面的PlayerCoin在同一组,但是低于其201,所以显示到下面
            await FW.UIMgr.OpenUI("UISampleLoading", 200);
            Debug.Log($"进入完成{this.GetType()}");


            var musicClip =
                await FW.AssetsMgr.LoadAsync<AudioClip>(
                    "Assets/HMFWSampleBundle/Audio/sound_background_ChristmasIsland.mp3");
            var soundClip = await FW.AssetsMgr.LoadAsync<AudioClip>("Assets/HMFWSampleBundle/Audio/Button_GetGlod.mp3");
            var musicClip2 =
                await FW.AssetsMgr.LoadAsync<AudioClip>(
                    "Assets/HMFWSampleBundle/Audio/sound_background_ActivityIsland.mp3");
            var soundClip2 = await FW.AssetsMgr.LoadAsync<AudioClip>("Assets/HMFWSampleBundle/Audio/Button_Free.mp3");

            FW.AudioMgr.AddAudioClip(AudioEnum.BgMusic, musicClip);
            FW.AudioMgr.AddAudioClip(AudioEnum.Sound, soundClip);
            FW.AudioMgr.AddAudioClip(AudioEnum.BgMusic2, musicClip2);
            FW.AudioMgr.AddAudioClip(AudioEnum.Sound2, soundClip2);

            FW.AudioMgr.PlayMusic(AudioEnum.BgMusic, true, "背景音乐",
                (x, y) => { Debug.Log($"MusicPlay Complete {x}  {y}"); });

            await ObjectPoolMgrDemo();
        }

        private async UniTask ObjectPoolMgrDemo()
        {
            //初始化对象池-GameObject 类型 TestGO的对象池
            {
                FW.ObjectPoolMgr.InitPool(() => new GameObject("TestGO"), "TestGO", (go) =>
                {
                    go.name = "TestGO-action";
                    go.SetActive(true);
                    GameObject.DontDestroyOnLoad(go); //因为是unity对象,如果要保持自己管理的话,那就不销毁它,否则切换场景就会被删除掉
                }, (go) =>
                {
                    go.name = "TestGO-releaseGo";
                    go.SetActive(false);
                }, (go) => { GameObject.Destroy(go); });

                //获取对象
                List<GameObject> list = new List<GameObject>();
                for (int i = 0; i < 100; i++)
                {
                    var go = FW.ObjectPoolMgr.GetPool<GameObject>("TestGO").Get(); //获取对象
                    list.Add(go);
                }

                await UniTask.Delay(5000);

                //释放对象
                foreach (var go in list)
                {
                    FW.ObjectPoolMgr.GetPool<GameObject>("TestGO").Release(go);
                }

                list.Clear();
            }
            //初始化对象池-GameObject 类型 TestGO2的对象池  -这个对象池管理器可以处理相同类型的对象的对象池,使用对象池名字区分
            {
                FW.ObjectPoolMgr.InitPool(() => { return new GameObject("TestGO2"); }, "TestGO2", (go) =>
                {
                    go.name = "TestGO2-action";
                    go.SetActive(true);
                    GameObject.DontDestroyOnLoad(go); //因为是unity对象,如果要保持自己管理的话,那就不销毁它,否则切换场景就会被删除掉
                }, (go) =>
                {
                    go.name = "TestGO2-releaseGo";
                    go.SetActive(false);
                }, (go) => { GameObject.Destroy(go); });

                //获取对象
                List<GameObject> list = new List<GameObject>();
                for (int i = 0; i < 100; i++)
                {
                    var go = FW.ObjectPoolMgr.GetPool<GameObject>("TestGO2").Get(); //获取对象
                    list.Add(go);
                }

                await UniTask.Delay(5000);

                //释放对象
                foreach (var go in list)
                {
                    FW.ObjectPoolMgr.GetPool<GameObject>("TestGO2").Release(go);
                }

                list.Clear();
            }

            //初始化对象池-非unity对象池,例如list
            {
                FW.ObjectPoolMgr.InitPool(() => new List<int>(), null, (lis) => { lis.Clear(); },
                    (lis) => { lis.Clear(); });

                //获取对象
                List<List<int>> list = new List<List<int>>();
                for (int i = 0; i < 100; i++)
                {
                    var item = FW.ObjectPoolMgr.GetPool<List<int>>().Get(); //获取对象
                    list.Add(item);
                }

                await UniTask.Delay(5000);

                //释放对象
                foreach (var go in list)
                {
                    FW.ObjectPoolMgr.GetPool<List<int>>().Release(go);
                }

                list.Clear();
            }

            await UniTask.Delay(2000);
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

    public enum AudioEnum
    {
        BgMusic,
        BgMusic2,
        Sound,
        Sound2,
    }
}