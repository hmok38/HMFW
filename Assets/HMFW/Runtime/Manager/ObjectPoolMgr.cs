using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace HMFW
{
    public class ObjectPoolMgr : ObjectPoolMgrBase
    {
        private readonly Dictionary<Type, Dictionary<string, IMyObjectPoolBase>> _maps =
            new Dictionary<Type, Dictionary<string, IMyObjectPoolBase>>();

        /// <summary>
        /// 清理某个对象池,所有对象会被销毁
        /// </summary>
        /// <param name="name"></param>
        public override void ClearPool<T>(string name = null) where T : class
        {
            ClearPool(typeof(T), name);
        }

        public override void ClearPool(Type type, string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = type.FullName;
            }

            var pool =
                GetPool(type, name);
            if (pool != null)
            {
                pool.Clear();
            }
        }

        /// <summary>
        /// 清理所有对象池
        /// </summary>
        public override void ClearAllPoolByType<T>() where T : class
        {
            ClearAllPoolByType(typeof(T));
        }

        public override void ClearAllPoolByType(Type type)
        {
            if (_maps.TryGetValue(type, out var dic))
            {
                if (dic != null)
                {
                    foreach (var variable in dic)
                    {
                        variable.Value?.Clear();
                    }
                }
            }
        }

        public override void ClearAllPool()
        {
            foreach (var dic in _maps)
            {
                if (dic.Value != null)
                {
                    MethodInfo methodInfo = null;
                    foreach (var poolKv in dic.Value)
                    {
                        var pool = poolKv.Value;
                        if (pool != null)
                        {
                            pool.Clear();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据名字获取对象池
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override IMyObjectPool<T> GetPool<T>(string name = null) where T : class
        {
            var pool = GetPool(typeof(T), name);
            if (pool != null)
            {
                return pool as IMyObjectPool<T>;
            }
            else
            {
                return null;
            }
        }

        public override IMyObjectPoolBase GetPool(Type type, string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = type.FullName;
            }

            if (_maps.TryGetValue(type, out var poolDic))
            {
                if (poolDic.TryGetValue(name, out var pool))
                {
                    return pool;
                }
            }

            Debug.LogError($"未能找到初始化后的对象池: {name} 类型:{type.FullName} ");
            return null;
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        /// <param name="name">这个对象的名字,传null就使用类型的全名作为对象池名字</param>
        /// <param name="createFunc">创建对象的函数</param>
        /// <param name="actionOnGet">当获取对象时候调用的函数</param>
        /// <param name="actionOnRelease">当释放这个对象时调用的函数</param>
        /// <param name="actionOnDestroy">当销毁对象时调用的函数</param>
        /// <param name="defaultCapacity">初始大小</param>
        /// <param name="maxSize">最大数量,大于这个数量的对象会被销毁掉</param>
        /// <returns></returns>
        public override IMyObjectPool<T> InitPool<T>(Func<T> createFunc, string name = null,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            int defaultCapacity = 10,
            int maxSize = 10000) where T : class
        {
            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).FullName;
            }

            if (!_maps.TryGetValue(typeof(T), out var dic))
            {
                _maps[typeof(T)] = new Dictionary<string, IMyObjectPoolBase>();
                dic = _maps[typeof(T)];
            }

            dic[name] = new MyObjectPool<T>(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, true,
                defaultCapacity, maxSize);
            return (MyObjectPool<T>)dic[name];
        }

        public override bool HasPool<T>(string name = null)
        {
            return HasPool(typeof(T), name);
        }

        public override bool HasPool(Type type, string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = type.FullName;
            }

            if (_maps.TryGetValue(type, out var poolDic))
            {
                if (poolDic.TryGetValue(name, out var pool))
                {
                    return pool != null;
                }
            }

            return false;
        }
    }


    public class MyObjectPool<T> :
        MyObjectPoolBase<T> where T : class

    {
        public MyObjectPool(Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            bool collectionCheck = true,
            int defaultCapacity = 10,
            int maxSize = 10000) : base(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, collectionCheck,
            defaultCapacity, maxSize)
        {
        }
    }


    public abstract class ObjectPoolMgrBase
    {
        /// <summary>
        /// 清理某个对象池,所有对象会被销毁
        /// </summary>
        /// <param name="name"></param>
        public abstract void ClearPool<T>(string name = null) where T : class;

        /// <summary>
        /// 清理某个对象池,所有对象会被销毁
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public abstract void ClearPool(Type type, string name = null);

        /// <summary>
        /// 清理某个类型的所有对象池
        /// </summary>
        public abstract void ClearAllPoolByType<T>() where T : class;

        /// <summary>
        /// 清理某个类型的所有对象池
        /// </summary>
        /// <param name="type"></param>
        public abstract void ClearAllPoolByType(Type type);

        /// <summary>
        /// 清理所有对象池
        /// </summary>
        public abstract void ClearAllPool();


        /// <summary>
        /// 根据名字获取对象池
        /// </summary>
        /// <param name="name">传入null的话会默认使用类型的全名作为name</param>
        /// <returns></returns>
        public abstract IMyObjectPool<T> GetPool<T>(string name = null) where T : class;

        /// <summary>
        /// 根据名字获取对象池
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name">传入null的话会默认使用类型的全名作为name</param>
        /// <returns></returns>
        public abstract IMyObjectPoolBase GetPool(Type type, string name = null);


        /// <summary>
        /// 初始化对象池
        /// </summary>
        /// <param name="name">对象池的名字 传入null的话会默认使用类型的全名作为name</param>
        /// <param name="createFunc">创建对象的函数</param>
        /// <param name="actionOnGet">当获取对象时候调用的函数</param>
        /// <param name="actionOnRelease">当释放这个对象时调用的函数</param>
        /// <param name="actionOnDestroy">当销毁对象时调用的函数</param>
        /// <param name="defaultCapacity">初始大小</param>
        /// <param name="maxSize">最大数量,大于这个数量的对象会被销毁掉</param>
        /// <returns></returns>
        public abstract IMyObjectPool<T> InitPool<T>(Func<T> createFunc, string name = null,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            int defaultCapacity = 10,
            int maxSize = 10000) where T : class;


        /// <summary>
        /// 判断某个池子是否拥有了(初始化过)
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract bool HasPool<T>(string name = null);

        /// <summary>
        /// 判断某个池子是否拥有了(初始化过)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract bool HasPool(Type type, string name = null);
    }

    public interface IMyObjectPoolBase : IDisposable
    {
        /// <summary>
        /// 一共多少个
        /// </summary>
        public abstract int CountAll { get; }

        /// <summary>
        /// 被取出使用的
        /// </summary>
        public int CountActive { get; }

        /// <summary>
        /// 当前未使用的
        /// </summary>
        public int CountInactive { get; }

        /// <summary>
        /// 清理所有的对象
        /// </summary>
        public void Clear();

        public void Dispose();
    }

    public interface IMyObjectPool<T> : IMyObjectPoolBase where T : class
    {
        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <returns></returns>
        public T Get();

        /// <summary>
        /// 释放一个对象
        /// </summary>
        /// <param name="element"></param>
        public void Release(T element);
    }

    public class MyObjectPoolBase<T> : IMyObjectPool<T> where T : class
    {
        internal readonly List<T> m_List;
        private readonly Func<T> m_CreateFunc;
        private readonly Action<T> m_ActionOnGet;
        private readonly Action<T> m_ActionOnRelease;
        private readonly Action<T> m_ActionOnDestroy;
        private readonly int m_MaxSize;
        internal bool m_CollectionCheck;

        public int CountAll { get; private set; }

        public int CountActive => this.CountAll - this.CountInactive;

        public int CountInactive => this.m_List.Count;

        public MyObjectPoolBase(
            Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            bool collectionCheck = true,
            int defaultCapacity = 10,
            int maxSize = 10000)
        {
            if (createFunc == null)
                throw new ArgumentNullException(nameof(createFunc));
            if (maxSize <= 0)
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
            this.m_List = new List<T>(defaultCapacity);
            this.m_CreateFunc = createFunc;
            this.m_MaxSize = maxSize;
            this.m_ActionOnGet = actionOnGet;
            this.m_ActionOnRelease = actionOnRelease;
            this.m_ActionOnDestroy = actionOnDestroy;
            this.m_CollectionCheck = collectionCheck;
        }

        public T Get()
        {
            T obj;
            if (this.m_List.Count == 0)
            {
                obj = this.m_CreateFunc();
                ++this.CountAll;
            }
            else
            {
                int index = this.m_List.Count - 1;
                obj = this.m_List[index];
                this.m_List.RemoveAt(index);
            }

            Action<T> actionOnGet = this.m_ActionOnGet;
            if (actionOnGet != null)
                actionOnGet(obj);
            return obj;
        }

        //public PooledObject<T> Get(out T v) => new PooledObject<T>(v = this.Get(), (IObjectPool<T>)this);

        public void Release(T element)
        {
            if (this.m_CollectionCheck && this.m_List.Count > 0)
            {
                for (int index = 0; index < this.m_List.Count; ++index)
                {
                    if ((object)element == (object)this.m_List[index])
                        throw new InvalidOperationException(
                            "Trying to release an object that has already been released to the pool.");
                }
            }

            Action<T> actionOnRelease = this.m_ActionOnRelease;
            if (actionOnRelease != null)
                actionOnRelease(element);
            if (this.CountInactive < this.m_MaxSize)
            {
                this.m_List.Add(element);
            }
            else
            {
                --this.CountAll;
                Action<T> actionOnDestroy = this.m_ActionOnDestroy;
                if (actionOnDestroy != null)
                    actionOnDestroy(element);
            }
        }

        public void Clear()
        {
            if (this.m_ActionOnDestroy != null)
            {
                foreach (T obj in this.m_List)
                    this.m_ActionOnDestroy(obj);
            }

            this.m_List.Clear();
            this.CountAll = 0;
        }

        public void Dispose() => this.Clear();
    }
}