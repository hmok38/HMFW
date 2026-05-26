using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HMFW.Core
{
    /// <summary>
    /// UGUI的基类,子类需要添加UGUIResUrl特性
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        /// <summary>
        /// 是不是返回按钮队列的ui,设置为ture,会在开启和关闭时自动添加到返回键队列管理器(FW.BackBtnQueueMgr)中
        /// </summary>
        public virtual bool beBackBtnQueueUI { get; }

        public UIInfo UiInfo { get; set; }

        /// <summary>
        /// 是属于哪个ui系统下的
        /// </summary>
        public virtual UISystem MyUISystem => UISystem.UGUI;

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

    /// <summary>
    /// UI系统
    /// </summary>
    public enum UISystem
    {
        Error,
        UGUI,
        FairyGui,
        Other0,
        Other1,
    }

    /// <summary>
    /// UI的形态类型
    /// </summary>
    public enum UIShapeType
    {
        /// <summary>
        /// 界面UI,有透视,可以看到场景,比如主界面,设置界面等,一般来说界面UI会有一个固定的位置,并且在打开的时候会关闭其他界面UI,界面UI之间一般不同时存在
        /// </summary>
        View,

        /// <summary>
        /// 全屏界面ui,无透视,无法看到这个界面ui下面的任何ui和场景
        /// </summary>
        FullCoverView,

        /// <summary>
        /// 弹窗UI,有透视,可以看到界面UI和场景
        /// </summary>
        PopUp,

        /// <summary>
        /// 全屏弹窗ui,无透视,无法看到这个全屏弹窗ui下面的任何ui和场景,会遮蔽下面所有的内容
        /// </summary>
        FullCoverModal
    }
}