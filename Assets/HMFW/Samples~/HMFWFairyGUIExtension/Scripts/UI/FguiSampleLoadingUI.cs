using HMFW;
using UnityEngine.UI;

[HMFW.Core.UGUIRes("Assets/HMFWSampleBundle/UI/FguiSampleLoadingUI.prefab",
    "FguiSampleLoadingUI", false)]
public class FguiSampleLoadingUI : HMFW.Core.UIBase
{
    private UnityEngine.UI.Button _button;

    private void Awake()
    {
        this._button = this.transform.Find("Button (Legacy)").GetComponent<Button>();
        this._button.onClick.AddListener(() => { FW.GameFsmMgr.ChangeState<FairyGuiSampleMainGameState>(); });
    }
}