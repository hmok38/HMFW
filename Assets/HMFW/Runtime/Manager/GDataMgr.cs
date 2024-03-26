using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HMFW
{
    public class GDataMgr : GDataMgrBase
    {
        private readonly Dictionary<int, ICollection> _mainIdMap = new Dictionary<int, ICollection>();
        private readonly Dictionary<Type, ICollection> _typeMap = new Dictionary<Type, ICollection>();

        public override T GetData<T>(int id, int subId = 0)
        {
            if (_mainIdMap.ContainsKey(id))
            {
                var map = _mainIdMap[id];
                if (map.GetType() != typeof(Dictionary<int, Dictionary<int, T>>))
                {
                    Debug.LogError($"dataType:{id} 类型错误:{typeof(T)} != {map.GetType()}");
                    return default;
                }

                var subMap = ((Dictionary<int, Dictionary<int, T>>) map)[id];
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

        public override void SetData<T>(int id, T v, int subId = 0)
        {
            if (_mainIdMap.ContainsKey(id))
            {
                var map = _mainIdMap[id];

                if (map.GetType() != typeof(Dictionary<int, Dictionary<int, T>>))
                {
                    Debug.LogError($"dataType:{id} 类型错误:{typeof(T)} != {map.GetType()}");
                    return;
                }

                var subMap = ((Dictionary<int, Dictionary<int, T>>) map)[id];
                if (subMap.ContainsKey(subId))
                {
                    subMap[subId] = v;
                }
                else
                {
                    subMap.Add(subId, v);
                }
            }
            else
            {
                if (_typeMap.ContainsKey(typeof(T)))
                {
                    var map = ((Dictionary<int, Dictionary<int, T>>) _typeMap[typeof(T)]);
                    var subMap = map[id];
                    if (subMap.ContainsKey(subId))
                    {
                        subMap[subId] = v;
                    }
                    else
                    {
                        subMap.Add(subId, v);
                    }

                    _mainIdMap.Add(id, map);
                }
                else
                {
                    var newMap = new Dictionary<int, Dictionary<int, T>>(1);
                    _typeMap.Add(typeof(T), newMap);
                    var subMap = new Dictionary<int, T>(10);
                    newMap.Add(id, subMap);
                    _mainIdMap.Add(id, newMap);
                    subMap.Add(subId, v);
                }
            }
        }
    }

    /** 全局数据管理器*/
    public abstract class GDataMgrBase
    {
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="id">数据的ID</param>
        /// <param name="subId">数据的子ID,相同结构的数据可以根据子ID保存多份</param>
        /// <returns></returns>
        public abstract T GetData<T>(int id, int subId = 0);

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="id">数据的ID</param>
        /// <param name="v">数据的值或者对象</param>
        /// <param name="subId">数据的子ID,相同结构的数据可以根据子ID保存多份</param>
        public abstract void SetData<T>(int id, T v, int subId = 0);
    }
}