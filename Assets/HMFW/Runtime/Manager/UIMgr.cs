using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HMFW.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HMFW
{
    public class UIMgr : UIMgrBase
    {
        protected readonly Dictionary<string, UIBase> _uiMap = new Dictionary<string, UIBase>();
        protected readonly Dictionary<string, UIBase> _uiTopMap = new Dictionary<string, UIBase>();
        protected readonly List<string> _uguiSortList = new List<string>();
        protected readonly List<string> _uguiTopSortList = new List<string>();
        protected readonly Dictionary<string, Type> _allUIBaseTypes = new Dictionary<string, Type>();
        protected readonly Dictionary<string, Type> _allUIAliasTypes = new Dictionary<string, Type>();
        protected bool _inited;
        protected Transform _uguiRoot;

        public override Transform UGUIRoot
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

        protected Transform _uguiTopRoot;

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
        public override void Init(Transform uguiRoot = null)
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

            if (_uguiTopRoot == null) //&& uguiTopRoot == null
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
                //_uguiTopRoot = uguiTopRoot;
            }

            _inited = true;
        }

        /// <summary> 打开UI </summary>
        public override async UniTask<bool> OpenUI(string uiFullNameOrAliasName, params System.Object[] args)
        {
            var ui = await OpenUIHandle(this.GetUIDataType(uiFullNameOrAliasName), _uiMap, UGUIRoot,
                _uguiSortList, args);
            return ui != null;
        }

        /// <summary> 打开UI </summary>
        public virtual async UniTask<T> OpenUI<T>(params System.Object[] args) where T : UIBase
        {
            return await OpenUIHandle(typeof(T), _uiMap, UGUIRoot, _uguiSortList, args) as T;
        }

        /// <summary> 打开TopUI 保证在普通UI上部 </summary>
        public override async UniTask<bool> OpenTopUI(string uiFullNameOrAliasName, params System.Object[] args)
        {
            var ui = await OpenUIHandle(this.GetUIDataType(uiFullNameOrAliasName), _uiTopMap, UguiTopRoot,
                _uguiTopSortList,
                args);

            return ui != null;
        }

        /// <summary> 打开TopUI 保证在普通UI上部 </summary>
        public virtual async UniTask<T> OpenTopUI<T>(params System.Object[] args) where T : UIBase
        {
            var uiFullName = typeof(T).FullName;
            return await OpenUIHandle(typeof(T), _uiTopMap, UguiTopRoot, _uguiTopSortList,
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
            UIBase uiBase = null;
            if (uiType.FullName != null && _uiMap.ContainsKey(uiType.FullName))
            {
                uiBase = _uiMap[uiType.FullName];
                _uiMap.Remove(uiType.FullName);
            }

            if (uiType.FullName != null && uiBase == null && _uiTopMap.ContainsKey(uiType.FullName))
            {
                uiBase = _uiTopMap[uiType.FullName];
                _uiTopMap.Remove(uiType.FullName);
            }

            if (uiBase == null) return;
            await uiBase.OnUIClose(args);
            if (uiBase != null)
                UnityEngine.Object.Destroy(uiBase.gameObject); //销毁
        }

        public override async UniTask<bool> CloseAllUI(string[] excludedUIs = null)
        {
            List<string> needCloseMapKey = new List<string>();
            string[] excludedUIType = null;
            if (excludedUIs != null && excludedUIs.Length > 0)
            {
                excludedUIType = new string[excludedUIs.Length];
                for (var i = 0; i < excludedUIs.Length; i++)
                {
                    var t = GetUIDataType(excludedUIs[i]);
                    if (t != null)
                    {
                        excludedUIType[i] = t.FullName;
                    }
                    else
                    {
                        excludedUIType[i] = null;
                    }
                }
            }


            foreach (var kv in _uiMap)
            {
                if (excludedUIType == null || !excludedUIType.Contains(kv.Key))
                {
                    needCloseMapKey.Add(kv.Key);
                }
            }

            foreach (var kv in _uiTopMap)
            {
                if (excludedUIType == null || !excludedUIType.Contains(kv.Key))
                {
                    needCloseMapKey.Add(kv.Key);
                }
            }

            UniTask[] allUniTask = new UniTask[needCloseMapKey.Count];
            for (var i = 0; i < needCloseMapKey.Count; i++)
            {
                var u = CloseUI(needCloseMapKey[i]);
                allUniTask[i] = u;
            }

            await UniTask.WhenAll(allUniTask);
            return true;
        }

        /// <summary>获得某个UI预制体的资源加载地址</summary>
        public virtual bool UGUILoadResUrl<T>(out UISystem uiSystem, out string resUrl, out string[] preLoadUrlStrings)
            where T : UIBase
        {
            return UGUILoadResUrl(typeof(T), out uiSystem, out resUrl, out preLoadUrlStrings);
        }

        /// <summary>获得某个UI预制体的资源加载地址</summary>
        protected virtual bool UGUILoadResUrl(Type uiType, out UISystem uiSystem, out string resUrl,
            out string[] preLoadUrlStrings)
        {
            var attribute =
                (UGUIAttribute)Attribute.GetCustomAttribute(uiType, typeof(UGUIAttribute));
            if (attribute == null)
            {
                Debug.LogErrorFormat("{0}类型未定义UGUIResUrl特性,请定义后再试", uiType.FullName);
                uiSystem = UISystem.Error;
                resUrl = null;
                preLoadUrlStrings = null;
                return false;
            }

            uiSystem = UISystem.UGUI;

            resUrl = ReplaceResUrl(attribute.UILoadUrl);

            preLoadUrlStrings = attribute.PreloadResUrl;

            return true;
        }

        /// <summary>获得某个UI预制体的资源加载地址</summary>
        public override bool UGUILoadResUrl(string uiFullNameOrAliasName, out UISystem uiSystem, out string resUrl,
            out string[] preLoadUrlStrings)
        {
            return UGUILoadResUrl(this.GetUIDataType(uiFullNameOrAliasName), out uiSystem, out resUrl,
                out preLoadUrlStrings);
        }


        /// <summary>获得某个UI预制体及其依赖的资源加载地址 返回的是新的字符串数组</summary>
        protected virtual string[] UGUIPreLoadResUrl(Type uiType)
        {
            var attribute =
                (UGUIAttribute)Attribute.GetCustomAttribute(uiType, typeof(UGUIAttribute));
            if (attribute == null)
            {
                Debug.LogErrorFormat("{0}类型未定义UGUIResUrl特性,请定义后再试", uiType);
                return null;
            }

            var stirs = new String[attribute.PreloadResUrl.Length];
            for (int i = 0; i < attribute.PreloadResUrl.Length; i++)
            {
                stirs[i] = ReplaceResUrl(attribute.PreloadResUrl[i]);
            }

            return stirs;
        }

        protected virtual void ResetRectTransform(RectTransform rectTransform)
        {
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;
            rectTransform.anchoredPosition = new Vector2(0, 0);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        protected virtual async UniTask<UIBase> OpenUIHandle(Type uiType, Dictionary<string, UIBase> map,
            Transform root,
            List<string> sortList,
            params System.Object[] args)
        {
            if (uiType == null) return default;
            if (uiType.FullName != null && map.TryGetValue(uiType.FullName, out var uiComTemp))
            {
                sortList.Remove(uiType.FullName);
                sortList.Add(uiType.FullName);
                SortUI(map, sortList);
                uiComTemp.gameObject.SetActive(false);
                uiComTemp.gameObject.SetActive(true);
                await uiComTemp.OnUIOpen(args);
                return uiComTemp;
            }

            sortList.Add(uiType.FullName);
            if (!UGUILoadResUrl(uiType, out var uiSystem, out var url, out var preLoadUrlStrings))
            {
                return null;
            }

            return uiSystem switch
            {
                UISystem.UGUI => await OpenHandleUGUI(url, preLoadUrlStrings, uiType, map, root, sortList, args),
                _ => default
            };
        }

        protected virtual async UniTask<UIBase> OpenHandleUGUI(string url, string[] preLoadUrlStrings, Type uiType,
            Dictionary<string, UIBase> map,
            Transform root,
            List<string> sortList,
            params System.Object[] args)
        {
            if (preLoadUrlStrings != null && preLoadUrlStrings.Length > 0)
                await FW.AssetsMgr.LoadAssetsAsync<UnityEngine.Object>(preLoadUrlStrings.ToList());

            var uiPrefab = await FW.AssetsMgr.LoadAsync<GameObject>(url);


            if (uiType.FullName != null && map.TryGetValue(uiType.FullName, out var uiComTemp1))
            {
                SortUI(map, sortList);
                uiComTemp1.gameObject.SetActive(false);
                uiComTemp1.gameObject.SetActive(true);
                await uiComTemp1.OnUIOpen(args);
                return uiComTemp1;
            }

            var ui = UnityEngine.Object.Instantiate(uiPrefab, root, true) as GameObject;
            var rectTransform = ui.transform as RectTransform;
            ResetRectTransform(rectTransform);
            var uiCom = ui.GetComponent(uiType) as UIBase;

            if (uiCom == null) uiCom = ui.AddComponent(uiType) as UIBase;
            map.Add(uiType.FullName, uiCom);
            SortUI(map, sortList);
            if (uiCom != null) await uiCom.OnUIOpen(args);
            return map[uiType.FullName];
        }

        protected virtual void SortUI(Dictionary<string, UIBase> map, List<string> uguiSortListTmp)
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

        protected virtual Type GetUIDataType(string typeName)
        {
            if (_allUIBaseTypes.Count <= 0)
            {
                this.UITypeDataInit();
            }

            if (_allUIAliasTypes.TryGetValue(typeName, out var type)) return type;
            if (_allUIBaseTypes.TryGetValue(typeName, out var dataType)) return dataType;
            Debug.LogError($"未找到名为{typeName} 的UI类");
            return null;
        }

        protected virtual void UITypeDataInit()
        {
            if (!_inited) Init();
            _allUIBaseTypes.Clear();
            _allUIAliasTypes.Clear();
            var subTypes = Tools.Util.GetAllSubClass(typeof(UIBase));
            for (var i = 0; i < subTypes.Count; i++)
            {
                var tempType = subTypes[i];
                if (tempType == null) continue;
                var attribute =
                    (UGUIAttribute)Attribute.GetCustomAttribute(tempType, typeof(UGUIAttribute));
                if (attribute == null)
                {
                    Debug.LogError($"{tempType.FullName} 无指定特性,UI必须添加UGUIResUrlAttribute或者FGUIResUrlAttribute 特性");
                    continue;
                }

                _allUIBaseTypes.Add(tempType.FullName, tempType);
                _allUIAliasTypes.Add(attribute.UIAlias, tempType);
            }
        }

        protected virtual string ReplaceResUrl(string str)
        {
            return str.Replace("[L]", FW.FwData.CurrentLanguageStr);
        }
    }

    /// <summary>
    /// UI管理器基类
    /// </summary>
    public abstract class UIMgrBase
    {
        public abstract Transform UGUIRoot { get; }

        /// <summary>
        /// 初始化UI,主要是自定义UIRoot对象用的,否则可以不单独调用,
        /// 如果要自定义UIRoot,那么请在开启任何UI之前调用此Init
        /// </summary>
        /// <param name="uguiRoot"></param>
        /// <param name="uguiTopRoot"></param>
        /// <returns></returns>
        public abstract void Init(Transform uguiRoot = null);

        /// <summary> 打开UI </summary>
        public abstract UniTask<bool> OpenUI(string uiFullNameOrAliasName, params System.Object[] args);

        /// <summary> 打开TopUI 保证在普通UI上部 </summary>
        public abstract UniTask<bool> OpenTopUI(string uiFullNameOrAliasName, params System.Object[] args);

        /// <summary> 关闭ui,会等待OnUIClose函数执行完毕后再销毁ui对象,如果有关闭动画等可以重写ui的OnUIClose函数. </summary>
        public abstract UniTask CloseUI(string uiFullNameOrAliasName, params System.Object[] args);

        /// <summary>获得某个UI预制体的资源加载地址</summary>
        public abstract bool UGUILoadResUrl(string uiFullNameOrAliasName, out UISystem uiSystem, out string resUrl,
            out string[] preLoadUrlStrings);

        /// <summary>
        /// 关闭所有ui,除了被排除的Ui以外
        /// </summary>
        /// <param name="excludedUIs"></param>
        /// <returns></returns>
        public abstract UniTask<bool> CloseAllUI(string[] excludedUIs = null);


        // /// <summary>
        // /// 获取UI组信息,如果没有则创建,默认为无限制,
        // /// </summary>
        // /// <param name="priorityBase">传入优先级数值,按照每100一组的方式进行分组,传入102获取100-199group的设置</param>
        // /// <returns></returns>
        // public abstract UIGroupSetting GetGroupSetting(int priorityBase);
        //
        // /// <summary>
        // /// 打开UI
        // /// </summary>
        // /// <param name="uiNameOrAlias">UI的类全名或者别名</param>
        // /// <param name="priority">权限,每100为一组,相同的值按顺序排列</param>
        // /// <param name="openType"></param>
        // /// <param name="args"></param>
        // /// <returns></returns>
        // public abstract UniTask<bool> OpenUI(string uiNameOrAlias, int priority = 100,
        //     OpenType openType = OpenType.Normal, params System.Object[] args);
        //
        // /// <summary>
        // /// 关闭某个UI组
        // /// </summary>
        // /// <param name="priorityBase">Ui组,会自动进行组转换,即÷100</param>
        // /// <param name="closeType">关闭全部/显示的/排队的</param>
        // /// <returns></returns>
        // public abstract UniTask CloseUIGroup(int priorityBase, CloseType closeType = CloseType.All);
        //
        // /// <summary>
        // /// 关闭所有的ui,可选全部/显示了的/排队中的.还可以排除某些ui不要关闭
        // /// </summary>
        // /// <param name="closeType">关闭全部/显示的/排队的</param>
        // /// <param name="excludedUIs">不用被关闭的ui</param>
        // /// <returns></returns>
        // public abstract UniTask CloseAllUI(CloseType closeType = CloseType.All, string[] excludedUIs = null);
    }

   
}