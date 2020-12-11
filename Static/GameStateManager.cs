using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HMFW
{
    /**游戏主逻辑状态机 */
    public class GameStateManager : MonoSingleton<GameStateManager>
    {
        public GameStateManager()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += LoadSceneOver;
            GameFsmDefine.RegGameFsm(this);
        }
        /**当前状态 */
        public GameStateBase currentState;
        /**上一个状态 */
        public GameStateBase lastState;
        /**状态字典 */
        Dictionary<string, GameStateBase> stateMap = new Dictionary<string, GameStateBase>();

       
        /**向状态管理机注册状态 */
        public void RegState<T>() where T : GameStateBase, new()
        {

            // console.log("注册状态:"+state.stateName);
            if (!this.stateMap.ContainsKey(typeof(T).GetType().Name))
            {
                var state = new T();
                this.stateMap.Add(state.GetType().Name, state);

            }

        }

        private object[] args = null;
        private bool lastStateChangen = false;
        /**改变游戏状态 注意:如果再次改变到当前状态会重走一次当前状态的流程*/
        public void ChangeState<T>(params object[] args) where T : GameStateBase
        {

            var stateTemp = this.GetState<T>();

            if (stateTemp != null)
            {

                this.args = args;

                lastStateChangen = false;
                if (this.currentState != stateTemp)
                {
                    this.lastState = this.currentState;
                    lastStateChangen = true;
                }

                this.currentState = stateTemp;
                if (this.lastState != null)
                {
                    StartLoadScene(this.currentState.SceneName);
                }
                else
                {
                    LoadSceneOver(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), UnityEngine.SceneManagement.LoadSceneMode.Additive);
                }





            }
            else
            {

                Debug.Log("状态切换失败:" + typeof(T).Name);
            }

        }
        private void StartLoadScene(string sceneName)
        {

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        }
        private void LoadSceneOver(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (this.lastState != null && lastStateChangen)
            {
                Debug.Log("离开游戏状态:" + this.lastState.GetType().Name);
                this.lastState.LeaveState(args);
            }
            Debug.Log("进入游戏状态:" + this.currentState.GetType().Name);
            this.currentState.EnterState(args);
        }
        /**获取游戏状态 */
        public T GetState<T>() where T : GameStateBase
        {

            if (!this.stateMap.ContainsKey(typeof(T).Name))
            {
                Debug.LogError("没有找到Game状态:" + typeof(T).Name);
                return null;
            }
            return this.stateMap[typeof(T).Name] as T;

        }

        /**检查当前游戏状态是不是参数传入的状态 */
        public bool CheckCurrentState<T>() where T : GameStateBase
        {
            if (this.currentState != null && (this.currentState).GetType().Name == typeof(T).Name)
            {
                return true;
            }
            return false;
        }

        public void Update()
        {
            if (currentState != null)
            {
                currentState.OnUpdata();
            }
        }

    }
}

