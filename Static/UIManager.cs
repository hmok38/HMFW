using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace HMFW
{

    /// <summary>
    /// UI管理器,请在场景中创建UIRoot空物体,并将本场景中需要用到的UI放入其下,不需要在UIRoot中手动添加本类
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager _Instance;
        
        public static UIManager Instance
        {
            get
            {
                if (_Instance && _Instance.gameObject && _Instance.gameObject.activeSelf)
                {
                    
                    return _Instance;
                }
                else
                {
                    var uiRoot = GameObject.Find("UIRoot");
                    if (!uiRoot)
                    {
                        uiRoot = new GameObject("UIRoot");
                    }
                    var uiManager = uiRoot.GetComponent<UIManager>();
                    if (!uiManager)
                    {
                        uiManager = uiRoot.AddComponent<UIManager>();
                    }
                    _Instance = uiManager;


                    return _Instance;
                }
            }

        }


        private UIBase[] RegistUIs ;

        private Dictionary<Type, UIBase> UIMap=new Dictionary<Type, UIBase>();
        private void Awake()
        {
            this.FindAllUI();
        }

        private void FindAllUI()
        {
            
             RegistUIs =  this.gameObject.GetComponentsInChildren<UIBase>(true);

            UIMap.Clear();
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
          return  GetUI(typeof(T)) as T;
        }
        private UIBase GetUI(Type type)
        {
            if (UIMap.ContainsKey(type))
            {
                return UIMap[type];
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
            return BeUIShow(typeof(T));
        }
        private bool BeUIShow(Type type)
        {
            if (UIMap.ContainsKey(type))
            {
                return UIMap[type].gameObject.activeSelf;
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

            return OpenUI(typeof(T), args) as T;
        }
        private UIBase OpenUI(Type type, params object[] args)
        {
            var ui = GetUI(type);
            if (ui != null)
            {
                ui.Open(args);
                return ui;
            }
            else
            {
                Debug.LogError("需要打开的UI不存在,type=" + type);
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

            return CloseUI(typeof(T), args) as T;
        }
        private UIBase CloseUI(Type type, params object[] args)
        {
            var ui = GetUI(type);
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
                Debug.LogError("需要关闭的UI不存在,type=" + type);
                return null;
            }
        }
        public void CloseAllUI()
        {
            foreach (var item in UIMap)
            {
               if(BeUIShow(item.Key))
                {
                    CloseUI(item.Key);
                }
            }
        }
    }
}