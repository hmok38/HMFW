using HMFW;
using UnityEngine;

public class FW
{
    private FW() //禁止外部创建
    {
    }

    public static FW API { get; } = new FW();

    private GameFsmMgrBase _gameFsmMgr;

    /// <summary>
    /// 框架模块:游戏状态机-负责游戏整体状态管理的有限状态机基类
    /// </summary>
    public GameFsmMgrBase GameFsmMgr
    {
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

    /// <summary>
    /// 全局框架数据类
    /// </summary>
    public FWData FwData { get; set; } = new FWData();

    /// <summary>
    /// 资源管理器
    /// </summary>
    public AssetsMgr AssetsMgr { get; set; } = new AssetsMgr();

    /// <summary>
    /// UI管理器
    /// </summary>
    public UIMgrBase UIMgr { get; set; } = new UIMgr();

    /// <summary>
    /// 全局事件管理器
    /// </summary>
    public GEventMgrBase GEventMgr { get; set; } = new GEventMgr();
    
    /// <summary>
    /// 全局数据管理器
    /// </summary>
    public GDataMgrBase GDataMgr { get; set; } = new GDataMgr();
}