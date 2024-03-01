using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HMFW.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HMFW
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public class UIMgr
    {
        private readonly Dictionary<string, UGUIBase> _uguiMap = new Dictionary<string, UGUIBase>();
        private readonly Dictionary<string, UGUIBase> _uguiTopMap = new Dictionary<string, UGUIBase>();
        private readonly List<string> _uguiSortList = new List<string>();
        private readonly List<string> _uguiTopSortList = new List<string>();
        private readonly Dictionary<string, Type> _allUIBaseTypes = new Dictionary<string, Type>();
        private bool _inited;
        private Transform _uguiRoot;

        public Transform UguiRoot
        {
            get
            {
                if (_uguiRoot == null)
                {
                    Init();
                }

                return _uguiRoot;
            }
        }

        private Transform _uguiTopRoot;

        public Transform UguiTopRoot
        {
            get
            {
                if (_uguiTopRoot == null)
                {
                    Init();
                }

                return _uguiTopRoot;
            }
        }

        /// <summary>
        /// 初始化UI,主要是自定义UIRoot对象用的,否则可以不单独调用,
        /// 如果要自定义UIRoot,那么请在开启任何UI之前调用此Init
        /// </summary>
        /// <param name="uguiRoot"></param>
        /// <param name="uguiTopRoot"></param>
        /// <returns></returns>
        public void Init(Transform uguiRoot = null, Transform uguiTopRoot = null)
        {
            GameObject rootTeam = null;
            if (_uguiRoot == null && uguiRoot == null)
            {
                var prefab = UnityEngine.Resources.Load<GameObject>("FWPrefabs/UIRootTeam");
                rootTeam = UnityEngine.Object.Instantiate(prefab);
                UnityEngine.Object.DontDestroyOnLoad(rootTeam);
                var eventObj = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                if (eventObj == null)
                {
                    new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }

                _uguiRoot = rootTeam.transform.Find("UIRoot");
            }
            else
            {
                _uguiRoot = uguiRoot;
            }

            if (_uguiTopRoot == null && uguiTopRoot == null)
            {
                if (rootTeam == null)
                {
                    var prefab = UnityEngine.Resources.Load<GameObject>("FWPrefabs/UIRootTeam");
                    rootTeam = UnityEngine.Object.Instantiate(prefab);
                    var eventObj = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                    if (eventObj == null)
                    {
                        new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                        //eventObj.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    }
                }


                _uguiTopRoot = rootTeam.transform.Find("UITopRoot");
            }
            else
            {
                _uguiTopRoot = uguiTopRoot;
            }

            _inited = true;
        }

        /// <summary> 打开UI </summary>
        public async UniTask<UGUIBase> OpenUI(string uiFullName, params Object[] args)
        {
            return await GetUI(this.GetUIDataType(uiFullName), _uguiMap, UguiRoot,
                _uguiSortList, args);
        }

        /// <summary> 打开UI </summary>
        public async UniTask<T> OpenUI<T>(params Object[] args) where T : UGUIBase
        {
            return await GetUI(typeof(T), _uguiMap, UguiRoot, _uguiSortList, args) as T;
        }

        /// <summary> 打开TopUI 保证在普通UI上部 </summary>
        public async UniTask<UGUIBase> OpenTopUI(string uiFullName, params Object[] args)
        {
            return await GetUI(this.GetUIDataType(uiFullName), _uguiTopMap, UguiTopRoot,
                _uguiTopSortList,
                args);
        }

        /// <summary> 打开TopUI 保证在普通UI上部 </summary>
        public async UniTask<T> OpenTopUI<T>(params Object[] args) where T : UGUIBase
        {
            var uiFullName = typeof(T).FullName;
            return await GetUI(typeof(T), _uguiTopMap, UguiTopRoot, _uguiTopSortList,
                args) as T;
        }

        /// <summary>获得某个UI预制体的资源加载地址</summary>
        public string UGUILoadResUrl<T>() where T : UGUIBase
        {
            return UGUILoadResUrl(typeof(T));
        }

        /// <summary>获得某个UI预制体的资源加载地址</summary>
        public string UGUILoadResUrl(Type uiType)
        {
            var attribute =
                (UGUIResUrlAttribute) Attribute.GetCustomAttribute(uiType, typeof(UGUIResUrlAttribute));
            if (attribute == null)
            {
                Debug.LogErrorFormat("{0}类型未定义UGUIResUrl特性,请定义后再试", uiType.FullName);
                return null;
            }


            var str = ReplaceLanguage(attribute.UILoadUrl);
            return str;
        }

        /// <summary>获得某个UI预制体的资源加载地址</summary>
        public string UGUILoadResUrl(string uiFullname)
        {
            return UGUILoadResUrl(this.GetUIDataType(uiFullname));
        }

        /// <summary>获得某个UI预制体及其依赖的资源加载地址</summary>
        public string[] UGUIPreLoadResUrl<T>() where T : UGUIBase
        {
            return UGUIPreLoadResUrl(typeof(T));
        }

        /// <summary>获得某个UI预制体及其依赖的资源加载地址 返回的是新的字符串数组</summary>
        public string[] UGUIPreLoadResUrl(Type uiType)
        {
            var attribute =
                (UGUIResUrlAttribute) Attribute.GetCustomAttribute(uiType, typeof(UGUIResUrlAttribute));
            if (attribute == null)
            {
                Debug.LogErrorFormat("{0}类型未定义UGUIResUrl特性,请定义后再试", uiType);
                return null;
            }

            var strs = new String[attribute.PreloadResUrl.Length];
            for (int i = 0; i < attribute.PreloadResUrl.Length; i++)
            {
                strs[i] = ReplaceLanguage(attribute.PreloadResUrl[i]);
            }

            return strs;
        }

        /// <summary>获得某个UI预制体及其依赖的资源加载地址</summary>
        public string[] UGUIPreLoadResUrl(string uiClassFullname)
        {
            var type = this.GetUIDataType(uiClassFullname);
            return UGUIPreLoadResUrl(type);
        }

        public void ResetRectTransform(RectTransform rectTransform)
        {
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;
            rectTransform.anchoredPosition = new Vector2(0, 0);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        /// <summary>重新初始化UI类型数据,主要是用来给热更程序集后使用的</summary>
        public void ResetUITypeData()
        {
            _allUIBaseTypes.Clear();
            this.UITypeDataInit();
        }


        private async UniTask<UGUIBase> GetUI(Type uiType, Dictionary<string, UGUIBase> map, Transform root,
            List<string> sortList,
            params Object[] args)
        {
            if (map.ContainsKey(uiType.FullName))
            {
                sortList.Remove(uiType.FullName);
                sortList.Add(uiType.FullName);
                var uiComTemp = map[uiType.FullName];
                uiComTemp.Args = args;
                uiComTemp.gameObject.SetActive(false);
                uiComTemp.gameObject.SetActive(true);
                SortUI(map, sortList);
                return uiComTemp;
            }

            sortList.Add(uiType.FullName);
            string url = UGUILoadResUrl(uiType);
            if (url == null) return null;
            var uiPrefab = await FW.API.AssetsMgr.LoadAsync<GameObject>(url);


            if (map.ContainsKey(uiType.FullName))
            {
                var uiComTemp = map[uiType.FullName];
                uiComTemp.Args = args;
                uiComTemp.gameObject.SetActive(false);
                uiComTemp.gameObject.SetActive(true);
                SortUI(map, sortList);
                return uiComTemp;
            }

            var ui = UnityEngine.Object.Instantiate(uiPrefab, root, true) as GameObject;
            var rectTransform = ui.transform as RectTransform;
            ResetRectTransform(rectTransform);
            var uiCom = ui.GetComponent(uiType) as UGUIBase;

            if (uiCom == null) uiCom = ui.AddComponent(uiType) as UGUIBase;
            uiCom.Args = args;
            map.Add(uiType.FullName, uiCom);
            SortUI(map, sortList);
            return map[uiType.FullName];
        }

        private void SortUI(Dictionary<string, UGUIBase> map, List<string> uguiSortListTmp)
        {
            var indexSubV = 0;
            for (int i = 0; i < uguiSortListTmp.Count; i++)
            {
                var typeName = uguiSortListTmp[i];
                if (map.ContainsKey(typeName))
                {
                    map[typeName].transform.SetSiblingIndex(i - indexSubV);
                    //Debug.Log($"{typeName} 放在 {i - indexSubV}");
                }
                else
                {
                    indexSubV++;
                }
            }
        }

        private Type GetUIDataType(string typeName)
        {
            if (_allUIBaseTypes.Count <= 0)
            {
                this.UITypeDataInit();
            }

            if (_allUIBaseTypes.ContainsKey(typeName)) return _allUIBaseTypes[typeName];

            Debug.LogError($"未找到名为{typeName} 的UI类");
            return null;
        }

        private void UITypeDataInit()
        {
            if (!_inited) Init();
            _allUIBaseTypes.Clear();
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < allAssemblies.Length; i++)
            {
                var assembly = allAssemblies[i];
                var types = assembly.GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    var typeTmp = types[j];
                    if (typeTmp.BaseType == typeof(UGUIBase))
                    {
                        _allUIBaseTypes.Add(typeTmp.FullName, typeTmp);
                    }
                }
            }
        }

        private string ReplaceLanguage(string str)
        {
            return str.Replace("[L]", FW.API.FwData.CurrentLanguageStr);
        }
    }
}