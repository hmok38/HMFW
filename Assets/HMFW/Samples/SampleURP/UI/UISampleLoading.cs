using System;
using HMFW.SampleURP.GameState;

namespace HMFW.SampleURP.UI
{
    [HMFW.Core.UGUIResUrl("Assets/HMFW/Samples/SampleURP/Bundle/UI/UISampleLoading.prefab")]
    public class UISampleLoading:HMFW.Core.UGUIBase
    {
        public UnityEngine.UI.Button btn;
        private void Awake()
        {
            btn.onClick.AddListener(() =>
            {
                HMFW.FW.API.GameFsmMgr.ChangeState<GameStateSampleLoading>();
            });
        }
    }
}