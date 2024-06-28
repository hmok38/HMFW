﻿using System;
using Cysharp.Threading.Tasks;
using HMFW.Core;
using HMFW.SampleURP.GameState;

namespace HMFW.SampleURP.UI
{
    [HMFW.Core.UGUI("Assets/HMFW/Samples/SampleURP/Bundle/UI/UISampleLoading.prefab", "UISampleLoading", true)]
    public class UISampleLoading : HMFW.Core.UIBase
    {
        public override UISystem MyUISystem => UISystem.UGUI;
        public UnityEngine.UI.Button btn;

        private void Awake()
        {
            btn.onClick.AddListener(() => { FW.GameFsmMgr.ChangeState<GameStateSampleLoading>(); });
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
    }
}