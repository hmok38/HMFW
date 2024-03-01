using System;
using UnityEngine;

namespace HMFW
{
    /// <summary>
    /// 框架的Runtime数据类
    /// </summary>
    public class FWData
    {
        public UnityEngine.SystemLanguage CurrentLanguage => Application.systemLanguage;

        public string CurrentLanguageStr => Enum.GetName(typeof(SystemLanguage), CurrentLanguage);

    }
}