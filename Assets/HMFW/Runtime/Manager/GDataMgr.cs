﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace HMFW
{
    public class GDataMgr : GDataMgrBase
    {
        protected readonly Dictionary<Enum, ICollection> MainIdMap = new Dictionary<Enum, ICollection>();

        protected readonly Dictionary<Enum, Dictionary<int, List<Action>>> ChangeActionMap =
            new Dictionary<Enum, Dictionary<int, List<Action>>>();

        protected readonly Dictionary<Enum, Dictionary<int, List<Action<int>>>> ChangeActionWithSubIdMap =
            new Dictionary<Enum, Dictionary<int, List<Action<int>>>>();

        protected readonly Dictionary<Enum, List<int>> ChangeMap = new Dictionary<Enum, List<int>>();

        public override bool HasData(Enum typeEnum, int subId = 0)
        {
            if (!MainIdMap.ContainsKey(typeEnum)) return false;

            return ContainsKey(MainIdMap[typeEnum], subId);
        }

        public override T GetData<T>(Enum typeEnum, int subId = 0)
        {
            if (MainIdMap.TryGetValue(typeEnum, out var map))
            {
                if (map.GetType() != typeof(Dictionary<int, T>))
                {
                    Debug.LogError($"dataType:{typeEnum} 类型错误:{typeof(T)} != {map.GetType()}");
                    return default;
                }

                var subMap = (Dictionary<int, T>)map;
                if (subMap.TryGetValue(subId, out var data))
                {
                    return data;
                }

                return default;
            }
            else
            {
                return default;
            }
        }

        public override void SetData<T>(Enum typeEnum, T v, int subId = 0)
        {
            if (MainIdMap.TryGetValue(typeEnum, out var map1))
            {
                if (map1.GetType() != typeof(Dictionary<int, T>))
                {
                    Debug.LogError($"dataType:{typeEnum} 类型错误:{typeof(T)} != {map1.GetType()}");
                    return;
                }

                var subMap = ((Dictionary<int, T>)map1);
                if (subMap.ContainsKey(subId))
                {
                    var old = subMap[subId];
                    subMap[subId] = v;
                    if (!old.Equals(v))
                    {
                        DispatchChangeEvent(typeEnum, subId);
                    }
                }
                else
                {
                    subMap.Add(subId, v);
                    DispatchChangeEvent(typeEnum, subId);
                }
            }
            else
            {
                var newMap = new Dictionary<int, T>();
                MainIdMap.Add(typeEnum, newMap);
                newMap.Add(subId, v);
                DispatchChangeEvent(typeEnum, subId);
            }
        }

        public override void RemoveData(Enum typeEnum, int subId = 0)
        {
            if (!MainIdMap.ContainsKey(typeEnum)) return;
            if (!ContainsKey(MainIdMap[typeEnum], subId)) return;
            RemoveKey(MainIdMap[typeEnum], subId);
        }

        public override void RemoveAllTypeData(Enum typeEnum)
        {
            if (!MainIdMap.ContainsKey(typeEnum)) return;
            ClearMap(MainIdMap[typeEnum]);
        }

        public override void RemoveAllDataOnMgr()
        {
            MainIdMap.Clear();
        }

        public override void AddListener(Enum typeEnum, Action action, int subId = 0)
        {
            if (!ChangeActionMap.ContainsKey(typeEnum))
            {
                var map = new Dictionary<int, List<Action>>();
                map.Add(subId, new List<Action>() { action });
                ChangeActionMap.Add(typeEnum, map);
            }
            else
            {
                var map = ChangeActionMap[typeEnum];
                if (!map.ContainsKey(subId))
                {
                    map.Add(subId, new List<Action>() { action });
                }
                else
                {
                    map[subId].Add(action);
                }
            }
        }

        public override void AddListener(Enum typeEnum, Action<int> action, int subId = 0)
        {
            if (!ChangeActionWithSubIdMap.ContainsKey(typeEnum))
            {
                var map = new Dictionary<int, List<Action<int>>>();
                map.Add(subId, new List<Action<int>>() { action });
                ChangeActionWithSubIdMap.Add(typeEnum, map);
            }
            else
            {
                var map = ChangeActionWithSubIdMap[typeEnum];
                if (!map.ContainsKey(subId))
                {
                    map.Add(subId, new List<Action<int>>() { action });
                }
                else
                {
                    map[subId].Add(action);
                }
            }
        }

        public override void RemoveListener(Enum typeEnum, Action action, int subId = 0)
        {
            if (!ChangeActionMap.ContainsKey(typeEnum)) return;
            if (!ChangeActionMap[typeEnum].ContainsKey(subId)) return;
            var list = ChangeActionMap[typeEnum][subId];
            list.RemoveAll(x => x.Target == null || (action.Target.Equals(x.Target) && action.Method.Equals(x.Method)));
        }

        public override void RemoveListener(Enum typeEnum, Action<int> action, int subId = 0)
        {
            if (!ChangeActionWithSubIdMap.ContainsKey(typeEnum)) return;
            if (!ChangeActionWithSubIdMap[typeEnum].ContainsKey(subId)) return;
            var list = ChangeActionWithSubIdMap[typeEnum][subId];
            list.RemoveAll(x => x.Target == null || (action.Target.Equals(x.Target) && action.Method.Equals(x.Method)));
        }

        public override void RemoveAllListener(Enum typeEnum, int subId = 0)
        {
            if (ChangeActionMap.ContainsKey(typeEnum))
            {
                if (ChangeActionMap[typeEnum].ContainsKey(subId))
                {
                    var list = ChangeActionMap[typeEnum][subId];
                    list.Clear();
                }
            }

            if (ChangeActionWithSubIdMap.ContainsKey(typeEnum))
            {
                if (ChangeActionWithSubIdMap[typeEnum].ContainsKey(subId))
                {
                    var list = ChangeActionWithSubIdMap[typeEnum][subId];
                    list.Clear();
                }
            }
        }

        public override void RemoveAllTypeListener(Enum typeEnum)
        {
            if (ChangeActionMap.TryGetValue(typeEnum, out var value))
            {
                value.Clear();
            }

            if (ChangeActionWithSubIdMap.TryGetValue(typeEnum, out var value1))
            {
                value1.Clear();
            }
        }

        public override void RemoveAllListenerOnMgr()
        {
            ChangeActionMap.Clear();
            ChangeActionWithSubIdMap.Clear();
        }

        public override void DispatchChangeEvent(Enum typeEnum, int subId)
        {
            if (ChangeActionMap.ContainsKey(typeEnum))
            {
                if (ChangeActionMap[typeEnum].ContainsKey(subId))
                {
                    var list = ChangeActionMap[typeEnum][subId];
                    for (var i = list.Count - 1; i >= 0; i--)
                    {
                        if (list[i].Target != null)
                        {
                            list[i].Invoke();
                        }
                    }
                }
            }

            if (ChangeActionWithSubIdMap.ContainsKey(typeEnum))
            {
                if (ChangeActionWithSubIdMap[typeEnum].ContainsKey(subId))
                {
                    var list = ChangeActionWithSubIdMap[typeEnum][subId];
                    for (var i = list.Count - 1; i >= 0; i--)
                    {
                        if (list[i].Target != null)
                        {
                            list[i].Invoke(subId);
                        }
                    }
                }
            }
        }

        protected virtual bool ContainsKey(ICollection map, int subId)
        {
            var containMethod = map.GetType().GetMethod("ContainsKey", BindingFlags.Public | BindingFlags.Instance);
            if (containMethod != null)
            {
                var bo = containMethod.Invoke(map, new System.Object[] { subId });
                return (bool)bo;
            }

            return false;
        }

        protected virtual bool RemoveKey(ICollection map, int subId)
        {
            // Dictionary<int, int> ma = new Dictionary<int, int>();
            // ma.Remove()
            var containMethod = map.GetType().GetMethod("Remove", BindingFlags.Public | BindingFlags.Instance, null,
                CallingConventions.Any,
                new Type[] { typeof(int) },
                null);
            if (containMethod != null)
            {
                var bo = containMethod.Invoke(map, new System.Object[] { subId });
                return (bool)bo;
            }

            return false;
        }

        protected virtual void ClearMap(ICollection map)
        {
            var containMethod = map.GetType().GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
            if (containMethod != null) containMethod.Invoke(map, null);
        }
    }

    /** 全局数据管理器*/
    public abstract class GDataMgrBase
    {
        /// <summary>
        /// 判断是否有某个数据
        /// </summary>
        /// <param name="typeEnum">数据的类型枚举,任何枚举都可以,建议使用每个程序集自定义的枚举</param>
        /// <param name="subId">数据的子ID,相同结构的数据可以根据子ID保存多份</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract bool HasData(Enum typeEnum, int subId = 0);

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="typeEnum">数据的类型枚举,任何枚举都可以,建议使用每个程序集自定义的枚举</param>
        /// <param name="subId">数据的子ID,相同结构的数据可以根据子ID保存多份</param>
        /// <returns></returns>
        public abstract T GetData<T>(Enum typeEnum, int subId = 0);

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="typeEnum">数据的类型枚举,任何枚举都可以,建议使用每个程序集自定义的枚举</param>
        /// <param name="v">数据的值或者对象</param>
        /// <param name="subId">数据的子ID,相同结构的数据可以根据子ID保存多份</param>
        public abstract void SetData<T>(Enum typeEnum, T v, int subId = 0);

        /// <summary>
        /// 移除某个指定的数据
        /// </summary>
        /// <param name="typeEnum"></param>
        /// <param name="subId"></param>
        public abstract void RemoveData(Enum typeEnum, int subId = 0);

        /// <summary>
        /// 移除一个大类的全部数据
        /// </summary>
        /// <param name="typeEnum"></param>
        public abstract void RemoveAllTypeData(Enum typeEnum);

        /// <summary>
        /// 移除这个管理类中所有的数据
        /// </summary>
        public abstract void RemoveAllDataOnMgr();

        /// <summary>
        /// 添加数据变化事件的监听
        /// </summary>
        /// <param name="typeEnum"></param>
        /// <param name="action"></param>
        /// <param name="subId"></param>
        public abstract void AddListener(Enum typeEnum, Action action, int subId = 0);

        /// <summary>
        /// 添加数据变化事件的监听(附带subId)
        /// </summary>
        /// <param name="typeEnum"></param>
        /// <param name="action"></param>
        /// <param name="subId"></param>
        public abstract void AddListener(Enum typeEnum, Action<int> action, int subId = 0);

        /// <summary>
        /// 移除对数据变化的监听
        /// </summary>
        /// <param name="typeEnum"></param>
        /// <param name="action"></param>
        /// <param name="subId"></param>
        public abstract void RemoveListener(Enum typeEnum, Action action, int subId = 0);

        /// <summary>
        /// 移除对数据变化的监听
        /// </summary>
        /// <param name="typeEnum"></param>
        /// <param name="action"></param>
        /// <param name="subId"></param>
        public abstract void RemoveListener(Enum typeEnum, Action<int> action, int subId = 0);

        /// <summary>
        /// 移除某个数据的所有监听事件
        /// </summary>
        /// <param name="typeEnum"></param>
        /// <param name="subId"></param>
        public abstract void RemoveAllListener(Enum typeEnum, int subId = 0);

        /// <summary>
        /// 移除某个大类的所有监听事件
        /// </summary>
        /// <param name="typeEnum"></param>
        public abstract void RemoveAllTypeListener(Enum typeEnum);

        /// <summary>
        /// 移除这个管理类上注册的所有的监听
        /// </summary>
        public abstract void RemoveAllListenerOnMgr();

        /// <summary>
        /// 手动派发监听的事件,主要是用来触发 类(class) 数据的监听事件;
        /// 而其他类型的数据(int,string等),当数据发生变化的时候会自动触发监听的事件
        /// </summary>
        /// <param name="typeEnum"></param>
        /// <param name="subId"></param>
        public abstract void DispatchChangeEvent(Enum typeEnum, int subId);
    }
}