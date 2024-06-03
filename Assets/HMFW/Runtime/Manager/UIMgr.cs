using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HMFW.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HMFW
{
    public class UIMgr : UIMgrBase
    {
        private readonly Dictionary<string, UGUIBase> _uguiMap = new Dictionary<string, UGUIBase>();
        private readonly Dictionary<string, UGUIBase> _uguiTopMap = new Dictionary<string, UGUIBase>();
        private readonly List<string> _uguiSortList = new List<string>();
        private readonly List<string> _uguiTopSortList = new List<string>();
        private readonly Dictionary<string, Type> _allUIBaseTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> _allUIAliasTypes = new Dictionary<string, Type>();
        private bool _inited;
        private Transform _uguiRoot;

        public override Transform UguiRoot
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

        public override Transform UguiTopRoot
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
        public override void Init(Transform uguiRoot = null, Transform uguiTopRoot = null)
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
        public override async UniTask<bool> OpenUI(string uiFullNameOrAliasName, params System.Object[] args)
        {
            var ui = await OpenUIHandle(this.GetUIDataType(uiFullNameOrAliasName), _uguiMap, UguiRoot,
                _uguiSortList, args);
            return ui != null;
        }

        /// <summary> 打开UI </summary>
        public async UniTask<T> OpenUI<T>(params System.Object[] args) where T : UGUIBase
        {
            return await OpenUIHandle(typeof(T), _uguiMap, UguiRoot, _uguiSortList, args) as T;
        }

        /// <summary> 打开TopUI 保证在普通UI上部 </summary>
        public override async UniTask<bool> OpenTopUI(string uiFullNameOrAliasName, params System.Object[] args)
        {
            var ui = await OpenUIHandle(this.GetUIDataType(uiFullNameOrAliasName), _uguiTopMap, UguiTopRoot,
                _uguiTopSortList,
                args);

            return ui != null;
        }

        /// <summary> 打开TopUI 保证在普通UI上部 </summary>
        public async UniTask<T> OpenTopUI<T>(params System.Object[] args) where T : UGUIBase
        {
            var uiFullName = typeof(T).FullName;
            return await OpenUIHandle(typeof(T), _uguiTopMap, UguiTopRoot, _uguiTopSortList,
                args) as T;
        }


        /// <summary>
        /// 关闭ui,会等待OnUIClose函数执行完毕后再销毁ui对象,如果有关闭动画等可以重写ui的OnUIClose函数.
        /// </summary>
        /// <param name="uiFullNameOrAliasName"></param>
        /// <param name="args"></param>
        public override async UniTask CloseUI(string uiFullNameOrAliasName, params System.Object[] args)
        {
            Type uiType = GetUIDataType(uiFullNameOrAliasName);
            UGUIBase uguiBase = null;
            if (_uguiMap.ContainsKey(uiType.FullName))
            {
                uguiBase = _uguiMap[uiType.FullName];
                _uguiMap.Remove(uiType.FullName);
            }

            if (uguiBase == null && _uguiTopMap.ContainsKey(uiType.FullName))
            {
                uguiBase = _uguiTopMap[uiType.FullName];
                _uguiTopMap.Remove(uiType.FullName);
            }

            if (uguiBase == null) return;
            await uguiBase.OnUIClose(args);
            if (uguiBase != null)
                UnityEngine.Object.Destroy(uguiBase.gameObject); //销毁
        }

        /// <summary>获得某个UI预制体的资源加载地址</summary>
        public string UGUILoadResUrl<T>() where T : UGUIBase
        {
            return UGUILoadResUrl(typeof(T));
        }

        /// <summary>获得某个UI预制体的资源加载地址</summary>
        private string UGUILoadResUrl(Type uiType)
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
        public override string UGUILoadResUrl(string uiFullNameOrAliasName)
        {
            return UGUILoadResUrl(this.GetUIDataType(uiFullNameOrAliasName));
        }

        /// <summary>获得某个UI预制体及其依赖的资源加载地址</summary>
        public string[] UGUIPreLoadResUrl<T>() where T : UGUIBase
        {
            return UGUIPreLoadResUrl(typeof(T));
        }

        /// <summary>获得某个UI预制体及其依赖的资源加载地址 返回的是新的字符串数组</summary>
        private string[] UGUIPreLoadResUrl(Type uiType)
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
        public override string[] UGUIPreLoadResUrl(string uiFullNameOrAliasName)
        {
            var type = this.GetUIDataType(uiFullNameOrAliasName);
            return UGUIPreLoadResUrl(type);
        }

        private void ResetRectTransform(RectTransform rectTransform)
        {
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;
            rectTransform.anchoredPosition = new Vector2(0, 0);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private async UniTask<UGUIBase> OpenUIHandle(Type uiType, Dictionary<string, UGUIBase> map, Transform root,
            List<string> sortList,
            params System.Object[] args)
        {
            if (map.ContainsKey(uiType.FullName))
            {
                sortList.Remove(uiType.FullName);
                sortList.Add(uiType.FullName);
                var uiComTemp = map[uiType.FullName];
                SortUI(map, sortList);
                uiComTemp.gameObject.SetActive(false);
                uiComTemp.gameObject.SetActive(true);
                await uiComTemp.OnUIOpen(args);
                return uiComTemp;
            }

            sortList.Add(uiType.FullName);
            string url = UGUILoadResUrl(uiType);
            if (url == null) return null;
            var uiPrefab = await FW.AssetsMgr.LoadAsync<GameObject>(url);


            if (map.ContainsKey(uiType.FullName))
            {
                var uiComTemp = map[uiType.FullName];
                SortUI(map, sortList);
                uiComTemp.gameObject.SetActive(false);
                uiComTemp.gameObject.SetActive(true);
                await uiComTemp.OnUIOpen(args);
                return uiComTemp;
            }

            var ui = UnityEngine.Object.Instantiate(uiPrefab, root, true) as GameObject;
            var rectTransform = ui.transform as RectTransform;
            ResetRectTransform(rectTransform);
            var uiCom = ui.GetComponent(uiType) as UGUIBase;

            if (uiCom == null) uiCom = ui.AddComponent(uiType) as UGUIBase;
            map.Add(uiType.FullName, uiCom);
            SortUI(map, sortList);
            if (uiCom != null) await uiCom.OnUIOpen(args);
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

            if (_allUIAliasTypes.ContainsKey(typeName)) return _allUIAliasTypes[typeName];
            if (_allUIBaseTypes.ContainsKey(typeName)) return _allUIBaseTypes[typeName];
            Debug.LogError($"未找到名为{typeName} 的UI类");
            return null;
        }

        private void UITypeDataInit()
        {
            if (!_inited) Init();
            _allUIBaseTypes.Clear();
            _allUIAliasTypes.Clear();
            var subTypes = Tools.Util.GetAllSubClass(typeof(UGUIBase));
            for (int i = 0; i < subTypes.Count; i++)
            {
                var tempType = subTypes[i];

                var attribute =
                    (UGUIResUrlAttribute) Attribute.GetCustomAttribute(tempType, typeof(UGUIResUrlAttribute));
                if (attribute == null)
                {
                    Debug.LogError($"{tempType.FullName} 无指定特性,UI必须添加UGUIResUrlAttribute 特性");
                    continue;
                }

                _allUIBaseTypes.Add(tempType.FullName, tempType);
                _allUIAliasTypes.Add(attribute.UIAlias, tempType);
            }
        }

        private string ReplaceLanguage(string str)
        {
            return str.Replace("[L]", FW.FwData.CurrentLanguageStr);
        }
    }

    /// <summary>
    /// UI管理器基类
    /// </summary>
    public abstract class UIMgrBase
    {
        public abstract Transform UguiRoot { get; }
        public abstract Transform UguiTopRoot { get; }

        /// <summary>
        /// 初始化UI,主要是自定义UIRoot对象用的,否则可以不单独调用,
        /// 如果要自定义UIRoot,那么请在开启任何UI之前调用此Init
        /// </summary>
        /// <param name="uguiRoot"></param>
        /// <param name="uguiTopRoot"></param>
        /// <returns></returns>
        public abstract void Init(Transform uguiRoot = null, Transform uguiTopRoot = null);

        /// <summary> 打开UI </summary>
        public abstract UniTask<bool> OpenUI(string uiFullNameOrAliasName, params System.Object[] args);

        /// <summary> 打开TopUI 保证在普通UI上部 </summary>
        public abstract UniTask<bool> OpenTopUI(string uiFullNameOrAliasName, params System.Object[] args);

        /// <summary> 关闭ui,会等待OnUIClose函数执行完毕后再销毁ui对象,如果有关闭动画等可以重写ui的OnUIClose函数. </summary>
        public abstract UniTask CloseUI(string uiFullNameOrAliasName, params System.Object[] args);

        /// <summary>获得某个UI预制体的资源加载地址</summary>
        public abstract string UGUILoadResUrl(string uiFullNameOrAliasName);

        /// <summary>获得某个UI预制体及其依赖的资源加载地址</summary>
        public abstract string[] UGUIPreLoadResUrl(string uiFullNameOrAliasName);
    }
}