using System;
using HMFW.Core;
using UnityEngine;

namespace HMFW
{
    /// <summary>
    /// UI组设置
    /// </summary>
    public class UIGroupSetting
    {
        /// <summary>
        /// 取值0,100,200等,每一个100段拥有一个UIGroupSetting类,可以限制每个组最多同时显示的ui数量
        /// </summary>
        public uint GroupId;

        /// <summary>
        /// 此组的最大显示Ui的数量-0代表不限制
        /// </summary>
        public uint BusyLimit;

        public Transform GroupRoot;
    }

    /// <summary>
    /// ui的打开方式
    /// </summary>
    public enum UIOpenType
    {
        /// <summary>
        /// 正常打开UI,如果触发当前组最大限制,会返回false,打开失败
        /// </summary>
        Normal,

        /// <summary>
        /// 打开UI,如果触发当前组最大限制,会返回ture,并排队等待当前组空闲后自动打开
        /// </summary>
        Wait,

        /// <summary>
        /// 打开UI,如果触发当前组最大限制,会返回ture,并排队(插入首位)等待当前组空闲后自动打开
        /// </summary>
        WaitFirst,

        /// <summary>
        /// 立刻打开,无视当前组限制
        /// </summary>
        Now,

        /// <summary>
        /// 立刻打开,无视当前组限制,并且隐藏当前组之前打开的UI
        /// </summary>
        CoveredNow,

        /// <summary>
        /// 立刻打开,无视当前组限制,并且隐藏当前组之前打开的UI.如果有相同的页面则等待
        /// </summary>
        CoveredOrWait
    }

    /// <summary>
    /// ui的关闭方式
    /// </summary>
    public enum UICloseType
    {
        /// <summary>
        /// 关闭全部的,包含已经显示的和正在排队的
        /// </summary>
        All,

        /// <summary>
        /// 关闭显示了的,队列中的不用管
        /// </summary>
        Showed,

        /// <summary>
        /// 关闭正在排队的,取消队列
        /// </summary>
        Waiting
    }

    /// <summary>
    /// UI的当前状态
    /// </summary>
    public enum UIState
    {
        /// <summary>
        /// UI处于等待中
        /// </summary>
        Wait,

        /// <summary>
        /// UI正在加载中(已经算显示了)
        /// </summary>
        Loading,

        /// <summary>
        /// UI显示了
        /// </summary>
        Show,

        /// <summary>
        /// UI显示了,但是因为cover被隐藏了
        /// </summary>
        Hide,

        /// <summary>
        /// UI被销毁了(关闭)
        /// </summary>
        Destroy,

        /// <summary>
        /// UI错误,例如open打开的时候,队列太满,
        /// </summary>
        Error,

        /// <summary>
        /// UI因为不允许多实例,导致打开失败
        /// </summary>
        NotMultiple,
    }

    /// <summary>
    /// UI的信息类
    /// </summary>
    public class UIInfo
    {
        public static uint UIInstanceId;

        public UIInfo()
        {
            UIInstanceId++;
            UIId = UIInstanceId;
        }

        public string UIName;
        public Type UIType;
        public UIBase UIBase;
        public uint Priority;
        public uint UIId;
        public UIState UIState = UIState.Loading;
        public object[] Arg;
        public UIOpenType UIOpenType;

        /// <summary>
        /// 是空
        /// </summary>
        /// <returns></returns>
        public bool IsNull()
        {
            return this.UIBase == null || this.UIBase.gameObject == null;
        }
    }
}