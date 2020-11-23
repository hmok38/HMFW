
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// hmok的游戏框架
/// </summary>
namespace HMFW
{
    /// <summary>
    /// 全局事件管理器_请在EventDefine中自定义事件类型 editdate: 20201123
    /// </summary>
    public static class EventManager
    {
        public delegate void Callback();
        public delegate void Callback<T>(T arg);
        public delegate void Callback<T, X>(T arg0, X arg1);
        public delegate void Callback<T, X, Y>(T arg0, X arg1, Y arg2);
        public delegate void Callback<T, X, Y, Z>(T arg0, X arg1, Y arg2, Z arg3);
        public delegate void Callback<T, X, Y, Z, W>(T arg0,X arg1,Y arg2,Z arg3,W arg4);
       
        private static Dictionary<EventName, Delegate> eventMap = new Dictionary<EventName, Delegate>();

        public static void MyAddListener(EventName key ,Delegate handler)
        {
            if (!eventMap.ContainsKey(key))
            {
                eventMap.Add(key, handler);
            }
            else
            {
                if (eventMap[key] != null)
                {
                   
                    try
                    {
                        eventMap[key] = Delegate.Combine(eventMap[key], handler);
                    }
                    catch(ArgumentException e)
                    {
                        Debug.LogErrorFormat("事件注册_类型错误Key={0}:{1}", key, e.Message);

                    }
                  
                }
                else
                {
                    eventMap[key] = handler;
                }

            }
        }
        /// <summary>
        /// 增加事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener(EventName key, Callback handler)
        {
            MyAddListener(key, handler);

        }
        /// <summary>
        /// 增加事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener<T>(EventName key, Callback<T> handler) 
        {
            MyAddListener(key, handler);
        }
        /// <summary>
        /// 增加事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener<T,X>(EventName key, Callback<T, X> handler)
        {
            MyAddListener(key, handler);
        }
        /// <summary>
        /// 增加事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener<T,X,Y>(EventName key, Callback<T, X, Y> handler)
        {
            MyAddListener(key, handler);
        }

        /// <summary>
        /// 增加事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener<T, X, Y,Z>(EventName key, Callback<T, X, Y,Z> handler)
        {
            MyAddListener(key, handler);
        }
        /// <summary>
        /// 增加事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener<T, X, Y, Z,W>(EventName key, Callback<T, X, Y, Z,W> handler)
        {
            MyAddListener(key, handler);
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        public static void SendEvent(EventName key, params object[] args)
        {
           
            if (eventMap.ContainsKey(key) && eventMap[key] != null)
            {
                try
                {
                    eventMap[key].DynamicInvoke(args);
                }
                catch(TargetParameterCountException e)
                {
                    Debug.LogErrorFormat("事件系统派发参数错误key={0}:{1}", key, e.Message);
                 }
               
            }
        }
        /// <summary>
        /// 移除全部事件
        /// </summary>
        /// <param name="key"></param>
        public static void RemoveAllListener()
        {
            eventMap = new Dictionary<EventName, Delegate>();
            
        }
        /// <summary>
        /// 移除某个事件所有的
        /// </summary>
        /// <param name="key"></param>
        public static void RemoveAllListenerByKey(EventName key)
        {
            if (eventMap.ContainsKey(key) && eventMap[key] != null)
            {
                eventMap.Remove(key);
            }
        }
        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="handler"></param>
        public static void RemoveListener(EventName key, Callback handler)
        {
            MyRemoveListener(key, handler);
        }
        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="handler"></param>
        public static void RemoveListener<T>(EventName key, Callback<T> handler)
        {
            MyRemoveListener(key, handler);
        }
        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="handler"></param>
        public static void RemoveListener<T, X>(EventName key, Callback<T, X> handler)
        {
            MyRemoveListener(key, handler);
        }
        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="handler"></param>
        public static void RemoveListener<T, X, Y>(EventName key, Callback<T, X, Y> handler)
        {
            MyRemoveListener(key, handler);
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="handler"></param>
        public static void RemoveListener<T, X, Y, Z>(EventName key, Callback<T, X, Y, Z> handler)
        {
            MyRemoveListener(key, handler);
        }
        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="handler"></param>
        public static void RemoveListener<T, X, Y, Z, W>(EventName key, Callback<T, X, Y, Z, W> handler)
        {
            MyRemoveListener(key, handler);
        }


        private static void MyRemoveListener(EventName key,Delegate handler) 
        {
            if (eventMap.ContainsKey(key)&& eventMap[key]!=null)
            {
                try
                {
                    eventMap[key]= Delegate.Remove(eventMap[key], handler);
                }
                catch (ArgumentException e)
                {
                    Debug.LogErrorFormat("事件注销_类型错误Key={0}:{1}", key, e.Message);

                }
              
            }

        }
    }

}


