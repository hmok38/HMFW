using System;
using System.Collections.Generic;
using System.Reflection;
using EventInfo = HMFW.Core.EventInfo;

namespace HMFW
{
    public class GEventMgr : GEventMgrBase
    {
        public override void Add(string key, Action func)
        {
            Add(key, func.Target, func.Method, 0);
        }

        public override void Add<T1>(string key, Action<T1> func)
        {
            Add(key, func.Target, func.Method, 1);
        }

        public override void Add<T1, T2>(string key, Action<T1, T2> func)
        {
            Add(key, func.Target, func.Method, 2);
        }

        public override void Add<T1, T2, T3>(string key, Action<T1, T2, T3> func)
        {
            Add(key, func.Target, func.Method, 3);
        }

        public override void Add<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> func)
        {
            Add(key, func.Target, func.Method, 4);
        }

        private void Add(string key, object target, MethodInfo metod, int paramNum)
        {
            List<EventInfo> list = null;
            if (!FuncMap.TryGetValue(key, out list))
            {
                list = new List<EventInfo>();
                FuncMap[key] = list;
            }

            EventInfo ev;
            ev.info = metod;
            ev.Target = target;
            ev.ParamCount = paramNum;
            FuncMap[key].Add(ev);
        }


        public override void Remove(string key, Action func)
        {
            Remove(key, func.Target, func.Method, 0);
        }

        public override void Remove<T1>(string key, Action<T1> func)
        {
            Remove(key, func.Target, func.Method, 1);
        }

        public override void Remove<T1, T2>(string key, Action<T1, T2> func)
        {
            Remove(key, func.Target, func.Method, 2);
        }

        public override void Remove<T1, T2, T3>(string key, Action<T1, T2, T3> func)
        {
            Remove(key, func.Target, func.Method, 3);
        }

        public override void Remove<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> func)
        {
            Remove(key, func.Target, func.Method, 4);
        }

        private void Remove(string key, object target, MethodInfo method, int paramNum)
        {
            List<EventInfo> list = null;
            List<EventInfo> RemoveList = new List<EventInfo>();
            if (FuncMap.TryGetValue(key, out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Target == target && list[i].info == method && paramNum == list[i].ParamCount)
                    {
                        RemoveList.Add(list[i]);
                    }
                }

                if (RemoveList.Count > 0)
                {
                    foreach (var item in RemoveList)
                    {
                        list.Remove(item);
                    }

                    RemoveList.Clear();
                }
            }
        }

        public override void Trigger(string key)
        {
            TriggerFinal(key);
        }

        public override void Trigger<T1>(string key, T1 t1)
        {
            TriggerFinal(key, t1);
        }

        public override void Trigger<T1, T2>(string key, T1 t1, T2 t2)
        {
            TriggerFinal(key, t1, t2);
        }

        public override void Trigger<T1, T2, T3>(string key, T1 t1, T2 t2, T3 t3)
        {
            TriggerFinal(key, t1, t2, t3);
        }

        public override void Trigger<T1, T2, T3, T4>(string key, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            TriggerFinal(key, t1, t2, t3, t4);
        }

        private void TriggerFinal(string key, params object[] objList)
        {
            if (FuncMap.TryGetValue(key, out var list))
            {
                int paramCount = objList.Length;
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].ParamCount == paramCount)
                    {
                        list[i].Trigger(objList);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 全局事件管理器
    /// </summary>
    public abstract class GEventMgrBase
    {
        protected readonly Dictionary<string, List<EventInfo>> FuncMap = new Dictionary<string, List<EventInfo>>();
        public abstract void Add(string key, Action func);
        public abstract void Add<T1>(string key, Action<T1> func);
        public abstract void Add<T1, T2>(string key, Action<T1, T2> func);
        public abstract void Add<T1, T2, T3>(string key, Action<T1, T2, T3> func);
        public abstract void Add<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> func);
        public abstract void Remove(string key, Action func);
        public abstract void Remove<T1>(string key, Action<T1> func);
        public abstract void Remove<T1, T2>(string key, Action<T1, T2> func);
        public abstract void Remove<T1, T2, T3>(string key, Action<T1, T2, T3> func);
        public abstract void Remove<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> func);
        public abstract void Trigger(string key);
        public abstract void Trigger<T1>(string key, T1 t1);
        public abstract void Trigger<T1, T2>(string key, T1 t1, T2 t2);
        public abstract void Trigger<T1, T2, T3>(string key, T1 t1, T2 t2, T3 t3);
        public abstract void Trigger<T1, T2, T3, T4>(string key, T1 t1, T2 t2, T3 t3, T4 t4);
    }
}