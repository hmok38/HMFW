using UnityEngine;

namespace Samples.HMFWFairyGUIExtension.Scripts
{
    public class FairyGuiSampleStart : MonoBehaviour
    {
        private void Awake()
        {
            FW.GameFsmMgr.ChangeState<FairyGuiSampleInitGameState>(); //这是访问框架自带模块的方式
        }
    }
}