using UnityEngine;

namespace HMFW.Core
{
    /// <summary>
    /// UGUI的基类,子类需要添加UGUIResUrl特性
    /// </summary>
    public abstract class UGUIBase : MonoBehaviour
    {
        public Object[] Args { get; set; }
    }
}