using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**各种状态基类 */
public abstract class StateBase
{
    /**状态基类 传入名字和所属的fsm*/
    public StateBase()
    {
        
    }
   
    
    /**状态的管理机 */
    public FsmBase fsm;

    /**进入状态 */
    public abstract void EnterState(params object[] args);
    /**离开状态 */
    public abstract void LeaveState(params object[] args);

    public abstract void OnUpdata();
    
}

/**游戏状态基类,一个状态管理一个场景 */
public abstract class GameStateBase
{
    /// <summary>
    /// 获得所管理的场景名字
    /// </summary>
    /// <returns></returns>
    public abstract string SceneName { get; }
   
    public abstract void EnterGameState(params object[] args);
    public abstract void OnUpdata();
    
    public abstract void LeaveState(params object[] args);



}

/**状态机基类*/
public abstract class FsmBase
{
    /**状态机基类 */
   public FsmBase(Object ower)
    {

        this.ower = ower;
        // FsmManager.Instance.regFsm(this);

    }
    /**状态机拥有者 */
    Object ower;
    /**当前状态 */
    public StateBase currentState;
    /**上一个状态 */
    public StateBase lastState;
    /**状态字典 */
    Dictionary<string, StateBase> stateMap = new Dictionary<string, StateBase>();
    /**向状态管理机注册状态 */
    public void RegState<T>()where T:StateBase,new()
    {
     
        

        // console.log("注册状态:"+state.stateName);
        if (!this.stateMap.ContainsKey(typeof(T).GetType().Name))
        {
            var state = new T();
            this.stateMap.Add(state.GetType().Name, state);
            state.fsm = this;
        }

    }


    /**改变游戏状态 注意:如果再次改变到当前状态会重走一次当前状态的流程*/
    public void ChangeState<T>( params object[] args) where T : StateBase
    {
        //&& this.GetState(stateName) != this.currentState
        var stateTemp = this.GetState<T>();

        if (stateTemp != null)
        {
            var lastStateChangen = false;
            if (this.currentState != stateTemp)
            {
                this.lastState = this.currentState;
                lastStateChangen = true;
            }

            this.currentState = stateTemp;
            if (this.lastState != null && lastStateChangen)
            {
                Debug.Log("离开游戏状态:" + this.lastState.GetType().Name);
                this.lastState.LeaveState(args);
            }
            Debug.Log("进入游戏状态:" + this.currentState.GetType().Name);
            this.currentState.EnterState(args);

        }
        else
        {

            Debug.Log("状态切换失败:" + typeof(T).Name);
        }

    }
    /**获取游戏状态 */
    public T GetState<T>() where T : StateBase
    {

        if (!this.stateMap.ContainsKey(typeof(T).Name) )
        {
            Debug.LogError("没有找到Game状态:" + typeof(T).Name);
            return null;
        }
        return this.stateMap[typeof(T).Name] as T;

    }

    /**检查当前游戏状态是不是参数传入的状态 */
    public bool CheckCurrentState<T>() where T : StateBase
    {
        if (this.currentState != null && (this.currentState).GetType().Name == typeof(T).Name)
        {
            return true;
        }
        return false;
    }

    public void OnUpdate()
    {
        if (currentState != null)
        {
            currentState.OnUpdata();
        }
    }
}




