using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HMFW
{
    /// <summary>
    /// 狀態機,T為宿主類型,狀態類必須繼承 FsmState&lt;T&gt; 將狀態定義在宿主類之内,可以直接訪問宿主類的所有內容
    /// </summary>
    /// <typeparam name="T">宿主類</typeparam>
    public class FSM<T> where T : class
    {
        public FSM(T owner)
        {
            this.Owner = owner;
        }

        public T Owner { get; private set; }

        /**状态字典 */
        protected readonly Dictionary<string, FsmState<T>> StateMap =
            new Dictionary<string, FsmState<T>>();

        public virtual bool BeStateChanging { get; protected set; }
        public virtual FsmState<T> CurrentState { get; protected set; }
        public virtual FsmState<T> LastState { get; protected set; }

        public virtual FsmState<T> RegState(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                Debug.Log($"FSM({typeof(T).Namespace}) RegState 不允许空类型");
                return null;
            }

            return RegState(GetStateType(type));
        }

        public virtual FsmState<T> RegState(Type type)
        {
            if (type == null)
            {
                Debug.Log($"FSM({typeof(T).Namespace}) RegState 不允许空类型");
                return null;
            }


            if (this.StateMap.TryGetValue(type.Name, out var state)) return state;

            var obj = Activator.CreateInstance(type) as FsmState<T>;
            if (obj == null)
            {
                Debug.Log($"FSM({typeof(T).Namespace}) RegState {type.Name} 类型未继承于FsmState");
                return null;
            }


            this.StateMap.Add(type.Name, obj);
            return obj;
        }


        public virtual FsmState<T> RegState()
        {
            return this.RegState(typeof(FsmState<T>));
        }

        public virtual void ChangeState(string type, params object[] args)
        {
            ChangeStateHandle(GetStateType(type), args).Forget();
        }


        public virtual void ChangeState<T2>(params object[] args) where T2 : FsmState<T>
        {
            ChangeStateHandle(typeof(T2), args).Forget();
        }

        public virtual void ChangeState(Type type, params object[] args)
        {
            ChangeStateHandle(type, args).Forget();
        }

        private async UniTaskVoid ChangeStateHandle(Type type, params object[] args)
        {
            CheckBeChanging:
            if (this.BeStateChanging)
            {
                await UniTask.WaitUntil(() => !this.BeStateChanging); //等待上一次切换结束后再切换
                goto CheckBeChanging;
            }

            this.BeStateChanging = true;
            var stateTemp = this.GetStateInstance(type);

            if (stateTemp != null)
            {
                this.LastState = this.CurrentState;

                this.CurrentState = stateTemp;
                if (this.LastState != null)
                {
                    await this.LastState.LeaveState();
                }

                await this.CurrentState.EnterState(args);
                this.BeStateChanging = false;
            }
            else
            {
                Debug.LogError($"FSM({typeof(T).Namespace}) 状态切换失败:" + type.Name);
            }
        }

        public virtual FsmState<T> GetStateInstance(string type)
        {
            return GetStateInstance(GetStateType(type));
        }

        public virtual T GetStateInstance<T2>() where T2 : T
        {
            return GetStateInstance(typeof(T)) as T;
        }


        public virtual FsmState<T> GetStateInstance(Type tp)
        {
            if (this.StateMap.TryGetValue(tp.Name, out var instance)) return instance;
            var clasT = Activator.CreateInstance(tp);
            var clasR = clasT as FsmState<T>;
            clasR.myFsm = this as FSM<T>;
            this.StateMap.Add(tp.Name, clasR);

            return this.StateMap[tp.Name];
        }

        public virtual bool CheckCurrentState(string type)
        {
            return this.CurrentState != null && (this.CurrentState).GetType().Name.Equals(type);
        }

        public virtual bool CheckCurrentState<T2>() where T2 : FsmState<T>
        {
            return CheckCurrentState(typeof(T2).Name);
        }

        private static readonly Dictionary<string, Type> _allGameStateTypes = new Dictionary<string, Type>();

        protected virtual Type GetStateType(string typeName)
        {
            if (_allGameStateTypes.Count <= 0)
            {
                this.UITypeDataInit();
            }

            if (_allGameStateTypes.TryGetValue(typeName, out var type)) return type;
            Debug.LogError($"FSM({typeof(T).Namespace}) 未找到名为{typeName} 的State类");
            return null;
        }

        protected virtual void UITypeDataInit()
        {
            _allGameStateTypes.Clear();
            var subTypes = Tools.Util.GetAllSubClass(typeof(FsmState<T>));
            for (var i = 0; i < subTypes.Count; i++)
            {
                var tempType = subTypes[i];
                if (!string.IsNullOrEmpty(tempType.Name)) _allGameStateTypes.Add(tempType.Name, tempType);
            }
        }

        public virtual void FsmUpdate()
        {
            if (CurrentState != null && !this.BeStateChanging)
            {
                CurrentState.OnUpdate();
            }
        }

        public virtual void FsmLateUpdate()
        {
            if (CurrentState != null && !this.BeStateChanging)
            {
                CurrentState.OnLateUpdate();
            }
        }

        public virtual void FsmFixedUpdate()
        {
            if (CurrentState != null && !this.BeStateChanging)
            {
                CurrentState.OnFixedUpdate();
            }
        }

        /// <summary>
        /// 状态机销毁时调用,用來將当前状态退出操作
        /// </summary>
        public virtual void FsmOnDestroy()
        {
            if (CurrentState != null)
            {
                CurrentState.LeaveState().Forget();
            }
        }

        /// <summary>
        /// 获取当前状态的某个接口(如受击),如果返回null代表这个状态不支持这个接口(如无敌状态不支持受击)
        /// </summary>
        /// <typeparam name="InterfaceT"></typeparam>
        /// <returns></returns>
        public virtual InterfaceT GetCurrentStateInterface<InterfaceT>() where InterfaceT : class, IFsmInterfaceBase
        {
            if (this.CurrentState is InterfaceT state)
            {
                return state;
            }

            return null;
        }
    }

    /// <summary>
    /// 状态类,T为宿主类类型,将状态类定义在宿主类之類,可以直接訪問宿主類的所有內容
    /// </summary>
    /// <typeparam name="T">宿主类</typeparam>
    public class FsmState<T> where T : class
    {
        public FSM<T> myFsm { get; set; }

        public virtual UniTask EnterState(params object[] args)
        {
            return default;
        }

        public virtual UniTask LeaveState()
        {
            return default;
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnLateUpdate()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }
    }

    /// <summary>
    /// fsm的接口类型,用來約束fsmState上要实现的功能接口的定义
    /// </summary>
    public interface IFsmInterfaceBase
    {
    }
}