using System.Reflection;
using Cysharp.Threading.Tasks;

namespace HMFW.Core
{
    public struct EventInfo
    {
        public MethodInfo info;
        public object Target;
        public int ParamCount;

        public void Trigger(params object[] objList)
        {
            info.Invoke(Target, objList);
        }
    }
}