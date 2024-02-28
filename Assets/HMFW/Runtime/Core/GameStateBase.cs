using Cysharp.Threading.Tasks;

namespace HMFW.Core
{
    /// <summary>
    /// 游戏主逻辑状态基类,一个状态管理一个场景
    /// </summary>
    public abstract class GameStateBase
    {
        /// <summary>
        /// 进入状态的回调,当新的场景被加载完成后调用,会在场景物品的awake,OnEnable后调用,但先于Start
        /// </summary>
        /// <param name="args"></param>
        public abstract UniTask EnterState(params object[] args);
        

        /// <summary>
        /// 离开状态的回调
        /// </summary>
        /// <param name="args"></param>
        public abstract UniTask LeaveState(params object[] args);

        /// <summary>
        /// 由状态机调用的update,只有本状态是当前激活状态的时候才会被调用
        /// </summary>
        public abstract void OnUpdate();
    }
}