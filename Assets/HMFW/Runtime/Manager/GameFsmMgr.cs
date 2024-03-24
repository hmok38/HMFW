using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HMFW.Core;
using UnityEngine;

namespace HMFW
{
    /// <summary>
    /// 游戏主逻辑状态机管理器
    /// </summary>
    public class GameFsmMgr : GameFsmMgrBase
    {
        /**状态字典 */
        private readonly Dictionary<string, GameStateBase> _stateMap = new Dictionary<string, GameStateBase>();

        public override bool BeStateChanging { get; protected set; }
        public override GameStateBase CurrentState { get; protected set; }
        public override GameStateBase LastState { get; protected set; }

        public override GameStateBase RegState(string type)
        {
            if (type == null)
            {
                Debug.Log($"GameFsmManager RegState 不允许空类型");
                return null;
            }

            return RegState(GetStateType(type));
        }

        public override GameStateBase RegState(Type type)
        {
            if (type == null)
            {
                Debug.Log($"GameFsmManager RegState 不允许空类型");
                return null;
            }


            if (this._stateMap.ContainsKey(type.Name)) return this._stateMap[type.Name];

            var obj = Activator.CreateInstance(type) as GameStateBase;
            if (obj == null)
            {
                Debug.Log($"GameFsmManager RegState {type.Name} 类型未继承于GameStateBase");
                return null;
            }


            this._stateMap.Add(type.Name, obj);
            return obj;
        }


        public override GameStateBase RegState<T>()
        {
            return this.RegState(typeof(T));
        }

        public override async UniTask ChangeState(string type, params object[] args)
        {
            await ChangeState(GetStateType(type), args);
        }


        public override async UniTask ChangeState<T>(params object[] args)
        {
            await ChangeState(typeof(T), args);
        }


        public override async UniTask ChangeState(Type type, params object[] args)
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
                if (this.CurrentState != null && this.CurrentState != stateTemp)
                {
                    this.LastState = this.CurrentState;
                }

                this.CurrentState = stateTemp;
                if (this.LastState != null)
                {
                    await this.LastState.LeaveState();
                }

                await this.CurrentState.EnterState(args);
            }
            else
            {
                Debug.LogError("状态切换失败:" + type.Name);
            }

            this.BeStateChanging = false;
        }

        public override GameStateBase GetStateInstance(string type)
        {
            return GetStateInstance(GetStateType(type));
        }

        public override T GetStateInstance<T>()
        {
            return GetStateInstance(typeof(T)) as T;
        }


        public override GameStateBase GetStateInstance(Type tp)
        {
            if (this._stateMap.ContainsKey(tp.Name)) return this._stateMap[tp.Name];
            var clasT = Activator.CreateInstance(tp);
            this._stateMap.Add(tp.Name, clasT as GameStateBase);

            return this._stateMap[tp.Name];
        }

        public override bool CheckCurrentState(string type)
        {
            return this.CurrentState != null && (this.CurrentState).GetType().Name.Equals(type);
        }

        public override bool CheckCurrentState<T>()
        {
            return CheckCurrentState(typeof(T).FullName);
        }

        private readonly Dictionary<string, Type> _allGameStateTypes = new Dictionary<string, Type>();

        private Type GetStateType(string typeName)
        {
            if (_allGameStateTypes.Count <= 0)
            {
                this.UITypeDataInit();
            }

            if (_allGameStateTypes.ContainsKey(typeName)) return _allGameStateTypes[typeName];
            Debug.LogError($"未找到名为{typeName} 的GameState类");
            return null;
        }

        private void UITypeDataInit()
        {
            _allGameStateTypes.Clear();
            var subTypes = Tools.Util.GetAllSubClass(typeof(GameStateBase));
            for (var i = 0; i < subTypes.Count; i++)
            {
                var tempType = subTypes[i];
                _allGameStateTypes.Add(tempType.FullName, tempType);
            }
        }

        protected override void Update()
        {
            if (CurrentState != null && !this.BeStateChanging)
            {
                CurrentState.OnUpdate();
            }
        }
    }


    /// <summary>
    /// 游戏状态机-负责游戏整体状态管理的有限状态机基类
    /// </summary>
    public abstract class GameFsmMgrBase : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// 是否正在切换状态中
        /// </summary>
        public abstract bool BeStateChanging { get; protected set; }

        /**当前状态 */
        public abstract GameStateBase CurrentState { get; protected set; }

        /**上一个状态 */
        public abstract GameStateBase LastState { get; protected set; }

        /**向状态管理机注册状态 */
        public abstract GameStateBase RegState(string type);

        /**向状态管理机注册状态 */
        public abstract GameStateBase RegState(Type type);

        /**向状态管理机注册状态 */
        public abstract GameStateBase RegState<T>() where T : GameStateBase, new();

        /// <summary>
        /// 改变游戏状态 注意:如果再次改变到当前状态会重走一次当前状态的流程
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract UniTask ChangeState(string type, params object[] args);

        /// <summary>
        /// 改变游戏状态 注意:如果再次改变到当前状态会重走一次当前状态的流程
        /// </summary>
        /// <param name="args">要传入的参数</param>
        /// <typeparam name="T"></typeparam>
        public abstract UniTask ChangeState<T>(params object[] args) where T : GameStateBase, new();

        /// <summary>
        /// 改变游戏状态 注意:如果再次改变到当前状态会重走一次当前状态的流程
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract UniTask ChangeState(Type type, params object[] args);

        /// <summary>
        /// 获取游戏状态实例(没有话会自动创建)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract GameStateBase GetStateInstance(string type);

        /// <summary>
        /// 获取游戏状态实例(没有话会自动创建)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract T GetStateInstance<T>() where T : GameStateBase, new();

        /// <summary>
        /// 获取游戏状态
        /// </summary>
        /// <param name="tp"></param>
        /// <returns></returns>
        public abstract GameStateBase GetStateInstance(Type tp);

        /**检查当前游戏状态是不是参数传入的状态 */
        public abstract bool CheckCurrentState(string type);

        /**检查当前游戏状态是不是参数传入的状态 */
        public abstract bool CheckCurrentState<T>() where T : GameStateBase;

        protected abstract void Update();
    }
}