using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HMFW.Core
{
    /// <summary>
    /// UGUI的基类,子类需要添加UGUIResUrl特性
    /// </summary>
    public abstract class UGUIBase : MonoBehaviour
    {
        /// <summary>
        /// UI开启的回调,UIManager.Open的时候会运行完成此函数后再返回
        /// </summary>
        /// <param name="args"></param>
        public virtual UniTask OnUIOpen(params object[] args)
        {
            return default;
        }

        /// <summary>
        /// UI关闭的回调,UIManager.Close的时候会运行完成此函数后再返回
        /// </summary>
        /// <param name="args"></param>
        public virtual UniTask OnUIClose(params object[] args)
        {
            return default;
        }
    }
}