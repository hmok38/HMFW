using System;
using Cysharp.Threading.Tasks;
using HMFW.Core;
using HMFW.SampleURP.GameState;
using UnityEngine;

namespace HMFW.SampleURP.UI
{
    [HMFW.Core.UGUIRes("Assets/HMFWSampleBundle/UI/UISampleLoading.prefab", "UISampleLoading", true)]
    public class UISampleLoading : HMFW.Core.UIBase
    {
        public override UISystem MyUISystem => UISystem.UGUI;
        public UnityEngine.UI.Button btn;

        private void Awake()
        {
            btn.onClick.AddListener(() => { FW.GameFsmMgr.ChangeState<GameStateSampleLoading>(); });
        }

        public override UniTask OnUIOpen(params object[] args)
        {
            if (args != null && args.Length >= 1)
            {
                this.transform.Find("Image").GetComponent<UnityEngine.UI.Image>().color = (Color)args[0];
            }

            return default;
        }

        public override async UniTask OnUIClose(params object[] args)
        {
            var img = this.transform.Find("Image").GetComponent<UnityEngine.UI.Image>();
            for (int i = 0; i < 100; i++)
            {
                if (img == null) break;
                var color = img.color;
                color.r += 0.01f;
                color.g += 0.01f;
                color.b += 0.01f;
                color.a -= 0.003f;
                img.color = color;
                await UniTask.Delay(30);
            }
        }

        public void OnGUI()
        {
            if (GUILayout.Button("播放音乐"))
            {
                FW.AudioMgr.PlayMusic(AudioEnum.BgMusic, true, "背景音乐",
                    (x, y) => { Debug.Log($"MusicPlay Complete {x}  {y}"); });
            }

            if (GUILayout.Button("播放音乐2"))
            {
                FW.AudioMgr.PlayMusic(AudioEnum.BgMusic2, true, "背景音乐2",
                    (x, y) => { Debug.Log($"MusicPlay Complete {x}  {y}"); });
            }

            if (GUILayout.Button("播放音效果"))
            {
                FW.AudioMgr.PlaySound(AudioEnum.Sound);
            }

            if (GUILayout.Button("播放音效果2"))
            {
                FW.AudioMgr.PlaySound(AudioEnum.Sound2);
            }

            if (GUILayout.Button("音乐变小"))
            {
                FW.AudioMgr.musicVolume -= 0.1f;
            }

            if (GUILayout.Button("音乐变大"))
            {
                FW.AudioMgr.musicVolume += 0.1f;
            }

            if (GUILayout.Button("音效变小"))
            {
                FW.AudioMgr.soundVolume -= 0.1f;
            }

            if (GUILayout.Button("音效变大"))
            {
                FW.AudioMgr.soundVolume += 0.1f;
            }
        }
    }
}