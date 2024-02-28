using UnityEngine;

namespace HMFW
{
    public class FW
    {
        private FW()
        {
        }

        public static FW API { get; } = new FW();

        private GameFsmMgrBase _gameFsmMgr;
        /// <summary>
        /// 框架模块:游戏状态机-负责游戏整体状态管理的有限状态机基类
        /// </summary>
        public GameFsmMgrBase GameFsmMgr {
            get
            {
                if (_gameFsmMgr == null)
                {
                    _gameFsmMgr = new GameObject("GameFsmMgr").AddComponent<GameFsmMgr>();
                }

                return _gameFsmMgr;
            }
            set => _gameFsmMgr = value;
        } 

        /// <summary>
        /// 框架模块:示例的模块
        /// </summary>
        public SampleMgrBase SampleMgr { get; set; } = new SampleMgr();
    }
}