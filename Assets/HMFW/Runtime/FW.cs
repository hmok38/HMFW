namespace HMFW
{
    public class FW
    {
        private FW()
        {
        }

        public static FW API { get; } = new FW();

        /// <summary>
        /// 框架模块:游戏状态机-负责游戏整体状态管理的有限状态机基类
        /// </summary>
        public GameFsmMgrBase GameFsmMgr { get; set; } = new GameFsmMgr();

        /// <summary>
        /// 框架模块:示例的模块
        /// </summary>
        public SampleMgrBase SampleMgr { get; set; } = new SampleMgr();
    }
}