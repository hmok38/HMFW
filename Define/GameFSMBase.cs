using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**各种状态基类 */
public abstract class StateBase
{
   
    /**状态的管理机 */
    public object Fsm;

    /**进入状态 */
    public abstract void EnterState(params object[] args);
    /**离开状态 */
    public abstract void LeaveState(params object[] args);

    public abstract void OnUpdata();
    
}

/**游戏状态基类,一个状态管理一个场景 */
public abstract class GameStateBase: StateBase
{
    /// <summary>
    /// 获得所管理的场景名字
    /// </summary>
    /// <returns></returns>
    public abstract string SceneName { get; }
   
   

}

/**状态机基类*/
public abstract class FsmBase<T> :MonoBehaviour where T: StateBase
{
    /**状态机基类 */
   public void InitFsm(Object owner)
    {

        this.owner = owner;
        

    }
    /**状态机拥有者 */
    public Object owner;
    /**当前状态 */
    public T currentState;
    /**上一个状态 */
    public T lastState;
    /**状态字典 */
    Dictionary<string, T> stateMap = new Dictionary<string, T>();
    /**向状态管理机注册状态 */
    public void RegState<C>()where C: T, new()
    {
        // console.log("注册状态:"+state.stateName);
        if (!this.stateMap.ContainsKey(typeof(C).GetType().Name))
        {
            var state = new C();
            this.stateMap.Add(state.GetType().Name, state);
            state.Fsm = this ;
        }

    }


    /**改变游戏状态 注意:如果再次改变到当前状态会重走一次当前状态的流程*/
    public void ChangeState<C>( params object[] args) where C : T
    {
        //&& this.GetState(stateName) != this.currentState
        var stateTemp = this.GetState<C>();

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
                Debug.Log("离开状态:" + this.lastState.GetType().Name);
                this.lastState.LeaveState(args);
            }
            Debug.Log("进入状态:" + this.currentState.GetType().Name);
            this.currentState.EnterState(args);

        }
        else
        {

            Debug.Log("状态切换失败:" + typeof(C).Name);
        }

    }
    /**获取游戏状态 */
    public C GetState<C>() where C : T
    {

        if (!this.stateMap.ContainsKey(typeof(C).Name) )
        {
            Debug.LogError("没有找到Game状态:" + typeof(C).Name);
            return null;
        }
        return this.stateMap[typeof(C).Name] as C;

    }

    /**检查当前游戏状态是不是参数传入的状态 */
    public bool CheckCurrentState<C>() where C : T
    {
        if (this.currentState != null && (this.currentState).GetType().Name == typeof(C).Name)
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




