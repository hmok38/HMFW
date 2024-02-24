using Cysharp.Threading.Tasks;

namespace HMFW
{
    /// <summary>
    /// 模块接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IModule
    {
        public UniTask<bool> Init();
    }
}