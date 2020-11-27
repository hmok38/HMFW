using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace HMFW
{

    /// <summary>
    /// UI管理器,请手动挂载,并手动注册UI
    /// </summary>
    public class UIManager : MonoSingleton<UIManager>
    {
        /// <summary>
        /// 是否全局-UIManager分场景挂载,不全局
        /// </summary>
        /// <returns></returns>
        public override bool IsGolbal()
        {
            return false;
        }
        [SerializeField]
        public UIBase[] RegistUIs;

        private Dictionary<Type, UIBase> UIMap;
        private void Awake()
        {
            UIMap = new Dictionary<Type, UIBase>();
            for (int i = 0; i < RegistUIs.Length; i++)
            {
                var ins = RegistUIs[i];
                UIMap.Add(ins.GetType(), RegistUIs[i]);
            }
        }
        /// <summary>
        /// 获取UI类--返回Null的话代表此场景不存在此UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetUI<T>() where T : UIBase
        {
            if (UIMap.ContainsKey(typeof(T)))
            {
                return UIMap[typeof(T)] as T;
            }

            return null;
        }

        /// <summary>
        /// UI是否显示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool BeUIShow<T>()
        {
            if (UIMap.ContainsKey(typeof(T)))
            {
                return UIMap[typeof(T)].gameObject.activeSelf;
            }
            return false;
        }

        /// <summary>
        /// 打开UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T OpenUI<T>(params object[] args) where T : UIBase
        {
            var ui = GetUI<T>();
            if (ui != null)
            {
                ui.Open(args);
                return ui;
            }
            else
            {
                Debug.LogError("需要打开的UI不存在,type=" + typeof(T));
                return null;
            }

        }

        /// <summary>
        /// 打开UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T CloseUI<T>(params object[] args) where T : UIBase
        {
            var ui = GetUI<T>();
            if (ui != null)
            {
                if (ui.gameObject.activeSelf == true)
                {
                    ui.Close(args);
                }

                return ui;
            }
            else
            {
                Debug.LogError("需要关闭的UI不存在,type=" + typeof(T));
                return null;
            }

        }
    }
}