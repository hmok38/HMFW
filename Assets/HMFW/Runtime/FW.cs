using HMFW;
using UnityEngine;

public class FW
{
    private FW() //禁止外部创建
    {
    }

    /// <summary>
    /// 自定义模块访问接口
    /// </summary>
    public static FW CustomAPI { get; } = new FW();

    private static GameFsmMgrBase _gameFsmMgr;

    /// <summary>
    /// 框架模块:游戏状态机-负责游戏整体状态管理的有限状态机基类
    /// </summary>
    public static GameFsmMgrBase GameFsmMgr
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
    public static SampleMgrBase SampleMgr { get; set; } = new SampleMgr();

    /// <summary>
    /// 全局框架数据类
    /// </summary>
    public static FWData FwData { get; set; } = new FWData();

    /// <summary>
    /// 资源管理器
    /// </summary>
    public static AssetsMgrBase AssetsMgr { get; set; } = new AssetsMgr();

    /// <summary>
    /// UI管理器
    /// </summary>
    public static UIMgrBase UIMgr { get; set; } = new UIMgr();

    /// <summary>
    /// 全局事件管理器
    /// </summary>
    public static GEventMgrBase GEventMgr { get; set; } = new GEventMgr();

    /// <summary>
    /// 全局数据管理器
    /// </summary>
    public static GDataMgrBase GDataMgr { get; set; } = new GDataMgr();

    /// <summary>
    /// 音频管理器
    /// </summary>
    public static AudioMgrBase AudioMgr { get; set; } = new AudioMgr();

    /// <summary>
    /// 对象池管理器
    /// </summary>
    public static ObjectPoolMgrBase ObjectPoolMgr { get; set; } = new ObjectPoolMgr();


    private static BackBtnQueueMgrBase _backBtnQueueMgr;

    /// <summary>
    /// 返回键队列管理器,添加如管理器中时,会在返回键被触发时,自动关闭最后添加的UI或者调用最后添加的函数
    /// ui的关闭不需要手动添加,重写UI类的beBackBtnQueueUI为true即可自动添加和移除.此字段默认为false
    /// </summary>
    public static BackBtnQueueMgrBase BackBtnQueueMgr
    {
        get
        {
            if (_backBtnQueueMgr == null)
            {
                _backBtnQueueMgr = new GameObject("BackBtnQueueMgr").AddComponent<BackBtnQueueMgr>();
                UnityEngine.Object.DontDestroyOnLoad(_backBtnQueueMgr.gameObject);
            }

            return _backBtnQueueMgr;
        }
        set => _backBtnQueueMgr = value;
    }
}