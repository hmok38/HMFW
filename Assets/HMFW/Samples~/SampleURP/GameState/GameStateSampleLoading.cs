﻿using Cysharp.Threading.Tasks;
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


            var musicClip = await FW.AssetsMgr.LoadAsync<AudioClip>("Assets/HMFWSampleBundle/Audio/sound_background_ChristmasIsland.mp3");
            var soundClip = await FW.AssetsMgr.LoadAsync<AudioClip>("Assets/HMFWSampleBundle/Audio/Button_GetGlod.mp3");
            var musicClip2 = await FW.AssetsMgr.LoadAsync<AudioClip>("Assets/HMFWSampleBundle/Audio/sound_background_ActivityIsland.mp3");
            var soundClip2 = await FW.AssetsMgr.LoadAsync<AudioClip>( "Assets/HMFWSampleBundle/Audio/Button_Free.mp3");
            
            FW.AudioMgr.AddAudioClip(AudioEnum.BgMusic, musicClip);
            FW.AudioMgr.AddAudioClip(AudioEnum.Sound, soundClip);
            FW.AudioMgr.AddAudioClip(AudioEnum.BgMusic2, musicClip2);
            FW.AudioMgr.AddAudioClip(AudioEnum.Sound2, soundClip2);

            FW.AudioMgr.PlayMusic(AudioEnum.BgMusic, true, "背景音乐",
                (x, y) => { Debug.Log($"MusicPlay Complete {x}  {y}"); });
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