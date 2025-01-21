using System;
using System.Collections.Generic;
using HMFW.Core;
using UnityEngine;

namespace HMFW
{
    /// <summary>
    /// 返回键队列管理器
    /// </summary>
    public class BackBtnQueueMgr : BackBtnQueueMgrBase
    {
        private readonly List<BackBtnQueueInfo> _list = new List<BackBtnQueueInfo>();

        /// <summary>
        /// 添加UI到返回键按钮队列中,当这个ui在队列最高的时,按返回后会自动关闭这个ui并将其从队列中移除
        /// 自动设置:将ui的UIBase类的beBackBtnQueueUI=true就自动添加到队列中
        /// 如果接受返回键的操作是隐藏或者其他操作,请传入onBackBtnFunc事件
        /// </summary>
        /// <param name="uiBase"></param>
        public override void AddToQueue(UIBase uiBase)
        {
            _list.Add(new BackBtnQueueInfo()
            {
                BeUI = true,
                TargetObj = uiBase
            });
        }

        /// <summary>
        /// 从返回键队列中移除
        /// </summary>
        /// <param name="uiBase"></param>
        public override void RemoveQueue(UIBase uiBase)
        {
            _list.RemoveAll(x =>
                x.BeUI && (x.TargetObj == null || x.TargetObj.Equals(uiBase)));
        }

        /// <summary>
        /// 添加UI到返回键按钮队列中,当这个ui在队列最高的时,按返回后会自动关闭这个ui并将其从队列中移除
        /// 自动设置:将ui的UIBase类的beBackBtnQueueUI=true就自动添加到队列中
        /// 如果接受返回键的操作是隐藏或者其他操作,请传入onBackBtnFunc事件
        /// </summary>
        /// <param name="targetObj"></param>
        /// <param name="onBackBtnFunc">当返回键被按下时调用的func,返回值为true则会移除事件,为false为不移除事件(例如等待玩家选择退出游戏等操作,需要确认的)</param>
        public override void AddToQueue(object targetObj, Func<object, bool> onBackBtnFunc)
        {
            if (onBackBtnFunc == null)
            {
                Debug.LogError("非ui加入返回键队列,必须传入onBackBtnFunc回调");
                return;
            }

            _list.Add(new BackBtnQueueInfo()
            {
                TargetObj = targetObj,
                OnBackBtnAction = onBackBtnFunc,
            });
        }


        /// <summary>
        /// 从返回键队列中移除
        /// </summary>
        /// <param name="targetObj"></param>
        /// <param name="onBackBtnFunc"></param>
        public override void RemoveQueue(object targetObj, Func<object, bool> onBackBtnFunc)
        {
            _list.RemoveAll(x =>
                !x.BeUI && (x.TargetObj == null || x.TargetObj.Equals(targetObj)) &&
                (x.OnBackBtnAction == null || x.OnBackBtnAction.Target == null ||
                 x.OnBackBtnAction.Equals(onBackBtnFunc)));
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Escape))
            {
                OnReturnBtn();
            }
        }

        private void OnReturnBtn()
        {
            for (int i = _list.Count - 1; i >= 0; i--)
            {
                var info = _list[i];
                if (info?.TargetObj == null)
                {
                    _list.Remove(info);

                    if (info != null)
                    {
                        Debug.LogError($"某个返回键队列元素已经为空,但其回调未被释放,请检查 {info.OnBackBtnAction?.Method.Name}");
                    }
                }
                else
                {
                    if (info.BeUI)
                    {
                        FW.UIMgr.CloseUI(info.TargetObj as UIBase);
                        break;
                    }
                    else
                    {
                        if (info.OnBackBtnAction == null || info.OnBackBtnAction.Target == null)
                        {
                            _list.Remove(info);
                        }
                        else
                        {
                            var needRemove = info.OnBackBtnAction.Invoke(info.TargetObj);
                            if (needRemove)
                            {
                                _list.Remove(info);
                            }

                            break;
                        }
                    }
                }
            }
        }

        private class BackBtnQueueInfo
        {
            public bool BeUI;
            public object TargetObj;

            /// <summary>
            /// 返回true就是调用结束后移除这个调用,否则不移除
            /// </summary>
            public Func<object, bool> OnBackBtnAction;
        }
    }

    public abstract class BackBtnQueueMgrBase : UnityEngine.MonoBehaviour
    {
        /// <summary>
        /// 添加UI到返回键按钮队列中,当这个ui在队列最高的时,按返回后会自动关闭这个ui并将其从队列中移除
        /// 自动设置:将ui的UIBase类的beBackBtnQueueUI=true就自动添加到队列中
        /// 如果接受返回键的操作是隐藏或者其他操作,请传入onBackBtnFunc事件
        /// </summary>
        /// <param name="uiBase"></param>
        public abstract void AddToQueue(UIBase uiBase);

        /// <summary>
        /// 添加UI到返回键按钮队列中,当这个ui在队列最高的时,按返回后会自动关闭这个ui并将其从队列中移除
        /// 自动设置:将ui的UIBase类的beBackBtnQueueUI=true就自动添加到队列中
        /// 如果接受返回键的操作是隐藏或者其他操作,请传入onBackBtnFunc事件
        /// </summary>
        /// <param name="targetObj"></param>
        /// <param name="onBackBtnFunc">当返回键被按下时调用的func,返回值为true则会移除事件,为false为不移除事件(例如等待玩家选择退出游戏等操作,需要确认的)</param>
        public abstract void AddToQueue(object targetObj, Func<object, bool> onBackBtnFunc);

        /// <summary>
        /// 从返回键队列中移除
        /// </summary>
        /// <param name="targetObj"></param>
        /// <param name="onBackBtnFunc"></param>
        public abstract void RemoveQueue(object targetObj, Func<object, bool> onBackBtnFunc);

        /// <summary>
        /// 从返回键队列中移除
        /// </summary>
        /// <param name="uiBase"></param>
        public abstract void RemoveQueue(UIBase uiBase);
    }
}