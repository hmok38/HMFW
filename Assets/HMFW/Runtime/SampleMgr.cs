using Cysharp.Threading.Tasks;

namespace HMFW
{
    /// <summary>
    /// 框架模块-示例的模块的基类
    /// </summary>
    public abstract class SampleMgrBase
    {
        public abstract int SampleAMethod();
    }
    
    /// <summary>
    /// 框架模块-示例的模块
    /// </summary>
    public class SampleMgr : SampleMgrBase
    {
        public override int SampleAMethod()
        {
            return 1;
        }
    }
}