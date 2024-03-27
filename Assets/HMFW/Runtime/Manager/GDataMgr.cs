using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace HMFW
{
    public class GDataMgr : GDataMgrBase
    {
        private readonly Dictionary<Enum, ICollection> _mainIdMap = new Dictionary<Enum, ICollection>();
        private readonly Dictionary<Type, ICollection> _typeMap = new Dictionary<Type, ICollection>();

        private readonly Dictionary<Enum, Dictionary<int, List<Action>>> _changeActionMap =
            new Dictionary<Enum, Dictionary<int, List<Action>>>();

        private readonly Dictionary<Enum, List<int>> _changeMap = new Dictionary<Enum, List<int>>();

        public override bool HasData(Enum typeEnum, int subId = 0)
        {
            if (!_mainIdMap.ContainsKey(typeEnum)) return false;

            return ContainsKey(_mainIdMap[typeEnum], subId);
        }

        public override T GetData<T>(Enum typeEnum, int subId = 0)
        {
            if (_mainIdMap.ContainsKey(typeEnum))
            {
                var map = _mainIdMap[typeEnum];
                if (map.GetType() != typeof(Dictionary<int, T>))
                {
                    Debug.LogError($"dataType:{typeEnum} 类型错误:{typeof(T)} != {map.GetType()}");
                    return default;
                }

                var subMap = (Dictionary<int, T>) map;
                if (subMap.ContainsKey(subId))
                {
                    return subMap[subId];
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
            if (_mainIdMap.ContainsKey(typeEnum))
            {
                var map = _mainIdMap[typeEnum];

                if (map.GetType() != typeof(Dictionary<int, T>))
                {
                    Debug.LogError($"dataType:{typeEnum} 类型错误:{typeof(T)} != {map.GetType()}");
                    return;
                }

                var subMap = ((Dictionary<int, T>) map);
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
                if (_typeMap.ContainsKey(typeof(T)))
                {
                    var map = ((Dictionary<int, T>) _typeMap[typeof(T)]);

                    if (map.ContainsKey(subId))
                    {
                        var old = map[subId];
                        map[subId] = v;
                        if (!old.Equals(v))
                        {
                            DispatchChangeEvent(typeEnum, subId);
                        }
                    }
                    else
                    {
                        map.Add(subId, v);
                        DispatchChangeEvent(typeEnum, subId);
                    }

                    _mainIdMap.Add(typeEnum, map);
                }
                else
                {
                    var newMap = new Dictionary<int, T>();
                    _typeMap.Add(typeof(T), newMap);

                    _mainIdMap.Add(typeEnum, newMap);
                    newMap.Add(subId, v);
                    DispatchChangeEvent(typeEnum, subId);
                }
            }
        }

        public override void RemoveData(Enum typeEnum, int subId = 0)
        {
            if (!_mainIdMap.ContainsKey(typeEnum)) return;
            if (!ContainsKey(_mainIdMap[typeEnum], subId)) return;
            RemoveKey(_mainIdMap[typeEnum], subId);
        }

        public override void RemoveAllTypeData(Enum typeEnum)
        {
            if (!_mainIdMap.ContainsKey(typeEnum)) return;
            ClearMap(_mainIdMap[typeEnum]);
        }

        public override void RemoveAllDataOnMgr()
        {
            _mainIdMap.Clear();
            _typeMap.Clear();
        }

        public override void AddListener(Enum typeEnum, Action action, int subId = 0)
        {
            if (!_changeActionMap.ContainsKey(typeEnum))
            {
                var map = new Dictionary<int, List<Action>>();
                map.Add(subId, new List<Action>() {action});
                _changeActionMap.Add(typeEnum, map);
            }
            else
            {
                var map = _changeActionMap[typeEnum];
                if (!map.ContainsKey(subId))
                {
                    map.Add(subId, new List<Action>() {action});
                }
                else
                {
                    map[subId].Add(action);
                }
            }
        }

        public override void RemoveListener(Enum typeEnum, Action action, int subId = 0)
        {
            if (!_changeActionMap.ContainsKey(typeEnum)) return;
            if (!_changeActionMap[typeEnum].ContainsKey(subId)) return;
            var list = _changeActionMap[typeEnum][subId];
            list.RemoveAll(x => x.Target == null || (action.Target.Equals(x.Target) && action.Method.Equals(x.Method)));
        }

        public override void RemoveAllListener(Enum typeEnum, int subId = 0)
        {
            if (!_changeActionMap.ContainsKey(typeEnum)) return;
            if (!_changeActionMap[typeEnum].ContainsKey(subId)) return;
            var list = _changeActionMap[typeEnum][subId];
            list.Clear();
        }

        public override void RemoveAllTypeListener(Enum typeEnum)
        {
            if (!_changeActionMap.ContainsKey(typeEnum)) return;
            _changeActionMap[typeEnum].Clear();
        }

        public override void RemoveAllListenerOnMgr()
        {
            throw new NotImplementedException();
        }

        public override void DispatchChangeEvent(Enum typeEnum, int subId)
        {
            if (!_changeActionMap.ContainsKey(typeEnum)) return;
            if (!_changeActionMap[typeEnum].ContainsKey(subId)) return;

            var list = _changeActionMap[typeEnum][subId];
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Target != null)
                {
                    list[i].Invoke();
                }
            }
        }

        private bool ContainsKey(ICollection map, int subId)
        {
            var containMethod = map.GetType().GetMethod("ContainsKey", BindingFlags.Public | BindingFlags.Instance);
            var bo = containMethod.Invoke(map, new System.Object[] {subId});
            return (bool) bo;
        }

        private bool RemoveKey(ICollection map, int subId)
        {
            // Dictionary<int, int> ma = new Dictionary<int, int>();
            // ma.Remove()
            var containMethod = map.GetType().GetMethod("Remove", BindingFlags.Public | BindingFlags.Instance, null,
                CallingConventions.Any,
                new Type[] {typeof(int)},
                null);
            var bo = containMethod.Invoke(map, new System.Object[] {subId});
            return (bool) bo;
        }

        private void ClearMap(ICollection map)
        {
            var containMethod = map.GetType().GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
            containMethod.Invoke(map, null);
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
        /// 移除对数据变化的监听
        /// </summary>
        /// <param name="typeEnum"></param>
        /// <param name="action"></param>
        /// <param name="subId"></param>
        public abstract void RemoveListener(Enum typeEnum, Action action, int subId = 0);

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