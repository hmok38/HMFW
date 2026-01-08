using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace HMFW
{
    /*
     *  反内存修改作弊管理器
     * 使用方法:
     * 1:每次加载存档后调用init方法进行初始化-清理之前的数据和初始化
     * 2:将需要保护的货币调用InitData进行初始化
     * 3:业务层每次设置货币数值时调用SetValue方法进行设置-保持业务数据和反作弊数据一致
     * 4:业务层每次使用货币或者获取货币时检查数值是否合法，调用CheckValue方法,如果不合法则认为被修改作弊
     * 5:如果发现作弊,请上报事件并将这个货币设置为0,防止继续作弊
     */
    public class AntiCheatDataMgr : AntiCheatDataMgrBase
    {
        private Dictionary<string, AntiCheatEncryptKey> _antiCheatKeyDatas;
        private Dictionary<string, AntiCheatDataWrapper> _antiCheatDataWrapper;


        /// <summary>
        /// 每次加载存档后必须进行一次初始化，不然可能会用到之前存档的数据
        /// </summary>
        public override void Init()
        {
            //还需要随机设置下加密算法的key
            if (_antiCheatKeyDatas == null)
                _antiCheatKeyDatas = new Dictionary<string, AntiCheatEncryptKey>();
            else
            {
                foreach (var vKey in _antiCheatKeyDatas)
                {
                    vKey.Value.Release();
                }

                _antiCheatKeyDatas.Clear();
            }

            if (_antiCheatDataWrapper != null)
            {
                foreach (var vk in _antiCheatDataWrapper)
                {
                    vk.Value.Release();
                }

                _antiCheatDataWrapper.Clear();
            }
            else
            {
                _antiCheatDataWrapper = new Dictionary<string, AntiCheatDataWrapper>();
            }
        }

        public override void InitData(string currencyId, long value)
        {
            if (!_antiCheatKeyDatas.ContainsKey(currencyId))
            {
                //添加Encryp的类
                AntiCheatEncryptKey antiCheatKeyData = AntiCheatEncryptKey.CreatFromPool();
                _antiCheatKeyDatas.TryAdd(currencyId, antiCheatKeyData);
            }

            //设置值
            SetValue(currencyId, value);
        }

        public override void InitDatas(Dictionary<string, long> currencyIdAndValue)
        {
            //初始化各个货币的数值
            foreach (var keyValuePair in currencyIdAndValue)
            {
                InitData(keyValuePair.Key, keyValuePair.Value);
            }
        }

        /// <summary>
        /// 检查数值是否合法
        /// </summary>
        /// <param name="currencyId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool CheckValue(string currencyId, long value)
        {
            long? actualValue = GetValue(currencyId);

            if (actualValue.HasValue)
            {
                return actualValue.Value == value;
            }
            else
            {
                //没有数据，直接返回true，认为不需要验证
                return true;
            }
        }

        /// <summary>
        /// 获取对应货币的数据
        /// </summary>
        /// <param name="currencyId"></param>
        /// <returns></returns>
        public override long? GetValue(string currencyId)
        {
            //认为不需要验证
            if (!_antiCheatKeyDatas.TryGetValue(currencyId, out var _antiCheatKeyData))
            {
                return null;
            }

            if (!_antiCheatDataWrapper.TryGetValue(currencyId, out var _antiWrapper))
            {
                Debug.LogError($"{currencyId} 没有对应的_antiCheatDataWrapper数据");
                return null;
            }

            var saveV = _antiWrapper.GetValue();
            var v = _antiCheatKeyData.GetDecryptValue(Math.Abs(saveV));
            return v * _antiWrapper.SignV;
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="currencyId"></param>
        /// <param name="value"></param>
        public override void SetValue(string currencyId, long value)
        {
            //认为不需要验证
            if (!_antiCheatKeyDatas.TryGetValue(currencyId, out var _antiCheatKeyData))
            {
                return;
            }

            if (_antiCheatKeyData != null) _antiCheatKeyData.Release();
            //每次设置值都重新创建一个加密key，防止被破解
            _antiCheatKeyDatas[currencyId] = AntiCheatEncryptKey.CreatFromPool();
            _antiCheatKeyData = _antiCheatKeyDatas[currencyId];
            long encryptValue = _antiCheatKeyData.GetEncryptValue(Math.Abs(value));


            if (_antiCheatDataWrapper.TryGetValue(currencyId, out var antiCheatDataWrapper))
            {
                antiCheatDataWrapper.Release();
            }

            antiCheatDataWrapper = AntiCheatDataWrapper.CreatFomPool();
            antiCheatDataWrapper.SaveValues(encryptValue * (value >= 0 ? 1 : -1));
            _antiCheatDataWrapper[currencyId] = antiCheatDataWrapper;
        }


        private class AntiCheatData
        {
            /// <summary>
            /// 从对象池创建
            /// </summary>
            /// <returns></returns>
            public static AntiCheatData CreatFromPool() => _objectPool.Get();

            private static UnityEngine.Pool.ObjectPool<AntiCheatData> _objectPool =
                new ObjectPool<AntiCheatData>(() => new AntiCheatData(), actionOnRelease: item => item.Reset());

            public int Value = 1; //做验证的值,这个1是为了作为首位占位符,最后的结果也要除去这个1


            public void Reset()
            {
                Value = 1;
            }

            /// <summary>
            /// 释放回pool
            /// </summary>
            public void Release()
            {
                Reset();
                _objectPool.Release(this);
            }
        }

        private class AntiCheatEncryptKey
        {
            public static AntiCheatEncryptKey CreatFromPool() => Pool.Get();

            private static UnityEngine.Pool.ObjectPool<AntiCheatEncryptKey> Pool =
                new ObjectPool<AntiCheatEncryptKey>(() => new AntiCheatEncryptKey(),
                    actionOnRelease: item => item.Reset());

            public string CurrencyId;

            //需要的随机key放在这里
            private long _addKey = Random.Range(50, 200);
            private long _mulKey = Random.Range(3, 10);

            public void Reset()
            {
                _addKey = Random.Range(50, 200);
                _mulKey = Random.Range(3, 10);
            }

            /// <summary>
            /// 释放回pool
            /// </summary>
            public void Release()
            {
                Reset();
                Pool.Release(this);
            }

            /// <summary>
            /// 根据加密后的值获取实际的值
            /// </summary>
            /// <param name="entryValue"></param>
            /// <returns></returns>
            public long GetDecryptValue(long entryValue)
            {
                long value = (entryValue - _addKey) / _mulKey;
                return value;
            }

            /// <summary>
            /// 根据真实的值获取加密后的值
            /// </summary>
            /// <param name="actualValue"></param>
            /// <returns></returns>
            public long GetEncryptValue(long actualValue)
            {
                long value = (long)actualValue * _mulKey + _addKey;
                return value;
            }
        }

        private class AntiCheatDataWrapper
        {
            private List<AntiCheatData> _antiCheatDatas = new List<AntiCheatData>();

            private int _randomCount;

            /// <summary>
            /// 正负乘数
            /// </summary>
            public int SignV { get; private set; }

            /// <summary>
            /// 从对象池创建
            /// </summary>
            /// <returns></returns>
            public static AntiCheatDataWrapper CreatFomPool() => Pool.Get();


            private static UnityEngine.Pool.ObjectPool<AntiCheatDataWrapper> Pool =
                new ObjectPool<AntiCheatDataWrapper>(() => new AntiCheatDataWrapper(),
                    actionOnRelease: item => item.Reset());

            public AntiCheatDataWrapper()
            {
                Reset();
            }

            public void SaveValues(long value)
            {
                var lis = UnityEngine.Pool.ListPool<int>.Get();
                SignV = value >= 0 ? 1 : -1;

                PickValue(value, ref lis);
                ClearAntiCheatDatas();
                for (var i = 0; i < lis.Count; i++)
                {
                    AntiCheatData antiCheatData = _antiCheatDatas[i % _randomCount];
                    if (antiCheatData == null)
                    {
                        antiCheatData = AntiCheatData.CreatFromPool();
                        _antiCheatDatas[i % _randomCount] = antiCheatData;
                    }

                    antiCheatData.Value = antiCheatData.Value * 10 + lis[i];
                }

                UnityEngine.Pool.ListPool<int>.Release(lis);
            }

            public long GetValue()
            {
                if (_antiCheatDatas == null || _antiCheatDatas.Count <= 0) return 0L;

                var lis = UnityEngine.Pool.ListPool<List<int>>.Get();
                int totalWei = 0;
                for (int i = 0; i < _antiCheatDatas.Count; i++)
                {
                    var lisAnti = UnityEngine.Pool.ListPool<int>.Get();
                    if (_antiCheatDatas[i] != null)
                    {
                        PickValue(_antiCheatDatas[i].Value, ref lisAnti);
                        lisAnti.RemoveAt(0);
                    }


                    lis.Add(lisAnti);
                    totalWei += Mathf.Max(1, lisAnti.Count);
                }

                var finaList = UnityEngine.Pool.ListPool<int>.Get();
                for (int i = 0; i < totalWei; i++)
                {
                    finaList.Add(-1); //-1代表没有这个位
                }

                for (int j = 0; j < lis.Count; j++)
                {
                    var data = lis[j];
                    if (lis.Count > 0)
                    {
                        for (int i = 0; i < data.Count; i++)
                        {
                            finaList[i * _randomCount + j] = data[i];
                        }
                    }
                }

                long finalValue = 0L;
                for (int i = 0; i < finaList.Count; i++)
                {
                    if (finaList[i] >= 0)
                        finalValue = finalValue * 10 + finaList[i];
                }

                for (int i = 0; i < lis.Count; i++)
                {
                    if (lis[i] != null)
                        UnityEngine.Pool.ListPool<int>.Release(lis[i]);
                }

                UnityEngine.Pool.ListPool<int>.Release(finaList);
                UnityEngine.Pool.ListPool<List<int>>.Release(lis);
                return finalValue * SignV;
            }

            private void ClearAntiCheatDatas()
            {
                if (_antiCheatDatas != null && _antiCheatDatas.Count > 0)
                {
                    for (int i = 0; i < _antiCheatDatas.Count; i++)
                    {
                        if (_antiCheatDatas[i] != null)
                            _antiCheatDatas[i].Release();
                        _antiCheatDatas[i] = null;
                    }
                }
            }


            public void Reset()
            {
                ClearAntiCheatDatas();
                _antiCheatDatas.Clear();
                _randomCount = Random.Range(2, 10);
                for (int i = 0; i < _randomCount; i++)
                {
                    _antiCheatDatas.Add(null);
                }

                SignV = 1;
            }

            /// <summary>
            /// 释放到pool
            /// </summary>
            public void Release()
            {
                this.Reset();
                Pool.Release(this);
            }

            /// <summary>
            /// 按照位数提取数字
            /// </summary>
            /// <param name="origenV"></param>
            /// <param name="values"></param>
            /// <returns></returns>
            public bool PickValue(long origenV, ref List<int> values)
            {
                values.Clear();
                // 处理负数：取绝对值（数字位数无正负）
                long absValue = Math.Abs(origenV);

                // 循环提取每一位（先提取个位，再十位，依此类推）
                while (absValue > 0)
                {
                    // 取当前最低位（个位）：%10 得到0-9的数字
                    byte digit = (byte)(absValue % 10);
                    values.Add(digit);
                    // 去掉已提取的最低位：/10 缩小10倍
                    absValue /= 10;
                }

                values.Reverse(); // 反转列表，使其按从高位到低位的顺序排列
                return true;
            }

            /// <summary>
            /// 按照位数提取数字
            /// </summary>
            /// <param name="origenV"></param>
            /// <param name="values"></param>
            /// <returns></returns>
            public bool PickValue(int origenV, ref List<int> values)
            {
                values.Clear();
                // 处理负数：取绝对值（数字位数无正负）
                long absValue = Math.Abs(origenV);

                // 循环提取每一位（先提取个位，再十位，依此类推）
                while (absValue > 0)
                {
                    // 取当前最低位（个位）：%10 得到0-9的数字
                    byte digit = (byte)(absValue % 10);
                    values.Add(digit);
                    // 去掉已提取的最低位：/10 缩小10倍
                    absValue /= 10;
                }

                values.Reverse(); // 反转列表，使其按从高位到低位的顺序排列
                return true;
            }
        }
    }

    public abstract class AntiCheatDataMgrBase
    {
        /// <summary>
        /// 每次确定存档后初始化都需要执行的逻辑,一定要执行，不然可能会用到之前存档的数据
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// 初始化数据-如果之前没有数据的话必须初始化
        /// </summary>
        /// <param name="currencyId"></param>
        /// <param name="value"></param>
        public abstract void InitData(string currencyId, long value);

        /// <summary>
        /// 初始化多个数据
        /// </summary>
        /// <param name="currencyIdAndValue"></param>
        public abstract void InitDatas(Dictionary<string, long> currencyIdAndValue);

        /// <summary>
        /// 检查数值是否合法--如果没有数据则认为合法
        /// </summary>
        /// <param name="currencyId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract bool CheckValue(string currencyId, long value);


        /// <summary>
        /// 获取对应货币的数据
        /// </summary>
        /// <param name="currencyId"></param>
        /// <returns></returns>
        public abstract long? GetValue(string currencyId);

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="currencyId"></param>
        /// <param name="value"></param>
        public abstract void SetValue(string currencyId, long value);
    }
}