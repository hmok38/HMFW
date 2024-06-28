using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HMFW.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HMFW
{
    public class UIMgrNew
    {
        protected const string UIGroupRootName = "UIGroup";
        protected bool Inited;
        protected readonly Dictionary<string, Type> AllUIBaseTypes = new Dictionary<string, Type>();
        protected readonly Dictionary<string, Type> AllUIAliasTypes = new Dictionary<string, Type>();

        /// <summary>
        /// 所有排队中,显示的,隐藏的UI,加载中的UI字典
        /// </summary>
        protected readonly Dictionary<string, List<UIInfo>> NameToUIMap = new Dictionary<string, List<UIInfo>>();

        /// <summary>
        /// 各组正在排队的UI
        /// </summary>
        protected readonly Dictionary<uint, List<UIInfo>> WaitingUIMap = new Dictionary<uint, List<UIInfo>>();

        /// <summary>
        /// 各组正在显示/隐藏/加载中  的ui
        /// </summary>
        protected readonly Dictionary<uint, List<UIInfo>> UIInGroupMap = new Dictionary<uint, List<UIInfo>>();

        protected Transform MyUGUIRoot;

        public Transform UGUIRoot
        {
            get
            {
                if (MyUGUIRoot == null)
                {
                    Init();
                }

                return MyUGUIRoot;
            }
        }

        protected Dictionary<uint, UIGroupSetting> UIGroupSettings;


        #region ----------Public Method----------------------------------

        /// <summary>
        /// 初始化UI,主要是自定义UIRoot对象用的,否则可以不单独调用,
        /// 如果要自定义UIRoot,那么请在开启任何UI之前调用此Init
        /// </summary>
        /// <param name="uguiRoot"></param>
        /// <returns></returns>
        public void Init(Transform uguiRoot = null)
        {
            GameObject rootTeam = null;
            if (MyUGUIRoot == null && uguiRoot == null)
            {
                var prefab = UnityEngine.Resources.Load<GameObject>("FWPrefabs/UGUIRoot");
                rootTeam = UnityEngine.Object.Instantiate(prefab);
                UnityEngine.Object.DontDestroyOnLoad(rootTeam);
                var eventObj = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                if (eventObj == null)
                {
                    new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }

                MyUGUIRoot = rootTeam.transform;
            }
            else
            {
                MyUGUIRoot = uguiRoot;
            }
            
            UIGroupSettings = new Dictionary<uint, UIGroupSetting>
            {
                { 0, new UIGroupSetting() { GroupId = 0, GroupRoot = MyUGUIRoot.Find($"{UIGroupRootName}0") } },
                { 100, new UIGroupSetting() { GroupId = 100, GroupRoot = MyUGUIRoot.Find($"{UIGroupRootName}100") } }
            };
            
            Inited = true;
        }

        public UIGroupSetting GetGroupSetting(uint priorityBase)
        {
            var groupId = GetGroupId(priorityBase);
            if (this.UIGroupSettings.TryGetValue(groupId, out var groupSetting))
            {
                return groupSetting;
            }

            var setting = new UIGroupSetting() { GroupId = groupId, GroupRoot = CreatGroupRoot(groupId) };
            this.UIGroupSettings.Add(groupId, setting);
            return setting;
        }

        public async UniTask<UIInfo> OpenUI(string uiNameOrAlias, uint priority = 100,
            UIOpenType uiOpenType = UIOpenType.Normal, params object[] args)
        {
            var uiType = this.GetUIDataType(uiNameOrAlias);
            if (uiType == null) return default;

            var attribute =
                (UIAttribute)Attribute.GetCustomAttribute(uiType, typeof(UIAttribute));

            if (uiType.FullName != null && !attribute.BeMultiple &&
                NameToUIMap.TryGetValue(uiType.FullName, out List<UIInfo> uiInfos))
            {
                //不能多实例的,去查找,如果发现有相同的页面就直接返回即可
                if (uiInfos != null && uiInfos.Count > 0 && !uiInfos[0].IsNull())
                {
                    return uiInfos[0];
                }
            }

            var uiInfo = new UIInfo()
            {
                UIName = uiType.FullName,
                Priority = priority,
                UIState = UIState.Wait,
                Arg = args,
                UIOpenType = uiOpenType,
                UIType = uiType
            };

            return await OpenUIByUIInfo(uiInfo);
        }

        protected async UniTask<UIInfo> OpenUIByUIInfo(UIInfo uiInfo, bool beCheckWait = false)
        {
            //创建和获取组设置
            var groupSetting = GetGroupSetting(uiInfo.Priority);

            //获取组显示/隐藏/加载中的ui列表
            List<UIInfo> showedList = null;
            if (!UIInGroupMap.TryGetValue(groupSetting.GroupId, out showedList))
            {
                showedList = new List<UIInfo>();
                UIInGroupMap.Add(groupSetting.GroupId, showedList);
            }

            //获取组等待中的ui列表
            List<UIInfo> waitingList = null;
            if (!WaitingUIMap.TryGetValue(groupSetting.GroupId, out waitingList))
            {
                waitingList = new List<UIInfo>();
                WaitingUIMap.Add(groupSetting.GroupId, waitingList);
            }

            //检查是否需要等待
            var needWait = CheckNeedWait(showedList, uiInfo.UIOpenType, groupSetting, uiInfo.UIName);

            if (needWait)
            {
                if (beCheckWait)
                {
                    //因为是检查等待的操作,所以直接插入第一个继续等待
                    waitingList.Insert(0, uiInfo); //first就插入最前面
                }
                else
                {
                    //需要等待就添加进来,然后等待有页面被关闭后再次打开
                    if (uiInfo.UIOpenType is UIOpenType.WaitFirst or UIOpenType.CoveredOrWait)
                    {
                        waitingList.Insert(0, uiInfo); //first就插入最前面
                    }
                    else
                    {
                        waitingList.Add(uiInfo);
                    }
                }


                return uiInfo;
            }


            if (!NameToUIMap.TryGetValue(uiInfo.UIName, out var uiTypeUiInfos))
            {
                NameToUIMap.Add(uiInfo.UIName, new List<UIInfo>());
            }

            NameToUIMap[uiInfo.UIName].Add(uiInfo);

            var attribute =
                (UIAttribute)Attribute.GetCustomAttribute(uiInfo.UIType, typeof(UIAttribute));

            switch (attribute.UISystem)
            {
                case UISystem.UGUI:
                    await OpenHandleUGUI(attribute, uiInfo.UIType, uiInfo, showedList, groupSetting);
                    break;
            }

            return uiInfo;
        }

        public async UniTask<UIInfo> CloseUI(UIBase uiBase, params object[] args)
        {
            var uiType = uiBase.GetType();

            if (NameToUIMap.TryGetValue(uiType.FullName, out var uiInfos))
            {
                if (uiInfos.Count <= 0) return default;
                var uiB = uiInfos.Find(x => x.UIBase == uiBase);

                if (uiB != null)
                {
                    return await CloseUIHandle(uiB, args);
                }
            }

            return default;
        }

        /// <summary>
        /// 关闭ui
        /// </summary>
        /// <param name="uiNameOrAlias">UI的类名或者别名</param>
        /// <param name="uiId">多实例的话请传入要关闭的id,传0会关闭第一个</param>
        /// <param name="args">要传入的参数</param>
        /// <returns></returns>
        public async UniTask<UIInfo> CloseUI(string uiNameOrAlias, uint uiId = 0, params object[] args)
        {
            var uiType = this.GetUIDataType(uiNameOrAlias);
            if (uiType == null) return default;
            if (NameToUIMap.TryGetValue(uiType.FullName, out var uiInfos))
            {
                if (uiInfos.Count <= 0) return default;
                if (uiId != 0)
                {
                    var uiB = uiInfos.Find(x => x.UIId == uiId);

                    if (uiB != null)
                    {
                        return await CloseUIHandle(uiB, args);
                    }
                }
                else
                {
                    return await CloseUIHandle(uiInfos[0], args);
                }
            }

            return default;
        }

        public UniTask CloseUIGroup(int priorityBase, UICloseType uiCloseType = UICloseType.All)
        {
            throw new NotImplementedException();
        }

        public UniTask CloseAllUI(UICloseType uiCloseType = UICloseType.All, string[] excludedUIs = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        protected async UniTask<UIInfo> CheckWaitUI(UIGroupSetting groupSetting)
        {
            if (groupSetting == null) return default;


            if (WaitingUIMap.TryGetValue(groupSetting.GroupId, out var waitUIInfos) &&
                UIInGroupMap.TryGetValue(groupSetting.GroupId, out var groupUI))
            {
                if (waitUIInfos.Count > 0 && GroupCanShowNewUI(groupUI, groupSetting))
                {
                    //打开这个ui
                    var info = waitUIInfos[0];
                    waitUIInfos.RemoveAt(0);
                    return await OpenUIByUIInfo(info, true);
                }
            }

            return default;
        }

        protected async UniTask<UIInfo> CloseUIHandle(UIInfo uiInfo, object[] args)
        {
            if (uiInfo == null) return default;
            if (uiInfo.UIState == UIState.Destroy) return uiInfo;

            uiInfo.UIState = UIState.Destroy;
            var groupId = GetGroupId(uiInfo.Priority);
            if (uiInfo.UIState == UIState.Wait)
            {
                if (WaitingUIMap.TryGetValue(groupId, out var waitingUI))
                {
                    waitingUI.Remove(uiInfo);
                }
            }
            else
            {
                if (UIInGroupMap.TryGetValue(groupId, out var groupUI))
                {
                    groupUI.Remove(uiInfo);
                }
            }

            if (NameToUIMap.TryGetValue(uiInfo.UIName, out var list))
            {
                list.Remove(uiInfo);
            }


            if (uiInfo.UIBase != null)
            {
                await uiInfo.UIBase.OnUIClose(args);
                Object.Destroy(uiInfo.UIBase.gameObject);
                uiInfo.UIBase = null;
            }

            var setting = GetGroupSetting(uiInfo.Priority);
            await CheckWaitUI(setting);
            return uiInfo;
        }

        protected Transform CreatGroupRoot(uint groupId)
        {
            int newIndex = MyUGUIRoot.childCount;
            for (int i = 0; i < MyUGUIRoot.childCount; i++)
            {
                var name = MyUGUIRoot.GetChild(i).name;
                if (uint.TryParse(name.Replace(UIGroupRootName, ""), out var childGroupId))
                {
                    if (groupId < childGroupId)
                    {
                        newIndex = i;
                        Debug.Log($"{groupId}组的index={newIndex}");
                        break;
                    }
                }
                else
                {
                    Debug.Log($"请移除 {MyUGUIRoot.name} Ui根节点下的 {name} 物体,ui根节点下只能自动创建ui组节点");
                }
            }

            var tr = MyUGUIRoot.Find($"{UIGroupRootName}{groupId}");
            if (tr == null)
            {
                var group0 = MyUGUIRoot.Find($"{UIGroupRootName}0");
                tr = Object.Instantiate(group0, MyUGUIRoot);
                tr.name = $"{UIGroupRootName}{groupId}";
                tr.SetSiblingIndex(newIndex);
                tr.GetComponent<Canvas>().sortingOrder = (int)groupId;
            }

            return tr;
        }
        
        protected virtual async UniTask<UIInfo> OpenHandleUGUI(UIAttribute attribute, Type uiType, UIInfo uiInfo,
            List<UIInfo> showedList, UIGroupSetting groupSetting)
        {
            UGUIAttribute uguiAttribute = attribute as UGUIAttribute;

            var preLoadUrlStrings = uguiAttribute.PreloadResUrl;
            uiInfo.UIState = UIState.Loading;
            showedList.Add(uiInfo);
            //开始加载就添加到显示列表中了
            if (preLoadUrlStrings != null && preLoadUrlStrings.Length > 0)
                await FW.AssetsMgr.LoadAssetsAsync<UnityEngine.Object>(preLoadUrlStrings.ToList());

            var uiPrefab = await FW.AssetsMgr.LoadAsync<GameObject>(uguiAttribute.UILoadUrl);

            //await结束后,检查一下uiInfo是否还存在,如果在这个过程总被关闭了,就不要实例化了.

            if (!showedList.Contains(uiInfo))
            {
                //没包含了,代表在加载的过程总被关闭了,那就不显示了吧
                if (uiInfo.UIState == UIState.Loading)
                {
                    uiInfo.UIState = UIState.Destroy;
                }

                return uiInfo;
            }

            var ui = UnityEngine.Object.Instantiate(uiPrefab, groupSetting.GroupRoot, true) as GameObject;
            var rectTransform = ui.transform as RectTransform;
            ResetRectTransform(rectTransform);

            var uiCom = ui.GetComponent(uiType) as UIBase;
            if (uiCom == null) uiCom = ui.AddComponent(uiType) as UIBase;
            uiInfo.UIBase = uiCom;
            uiCom.UiInfo = uiInfo;
            SortUI(showedList);
            uiInfo.UIState = UIState.Show;
            if (uiCom != null) await uiCom.OnUIOpen(uiInfo.Arg);
            return uiInfo;
        }

        protected virtual Type GetUIDataType(string typeName)
        {
            if (AllUIBaseTypes.Count <= 0)
            {
                this.UITypeDataInit();
            }

            if (AllUIAliasTypes.TryGetValue(typeName, out var type)) return type;
            if (AllUIBaseTypes.TryGetValue(typeName, out var dataType)) return dataType;
            Debug.LogError($"未找到名为{typeName} 的UI类");
            return null;
        }

        protected virtual void UITypeDataInit()
        {
            if (!Inited) Init();
            AllUIBaseTypes.Clear();
            AllUIAliasTypes.Clear();
            var subTypes = Tools.Util.GetAllSubClass(typeof(UIBase));
            for (var i = 0; i < subTypes.Count; i++)
            {
                var tempType = subTypes[i];
                if (tempType == null) continue;
                var attribute =
                    (UIAttribute)Attribute.GetCustomAttribute(tempType, typeof(UIAttribute));
                if (attribute == null)
                {
                    Debug.LogError($"{tempType.FullName} 无指定特性,UI必须添加UGUIResUrlAttribute或者FGUIResUrlAttribute 特性");
                    continue;
                }

                AllUIBaseTypes.Add(tempType.FullName, tempType);
                AllUIAliasTypes.Add(attribute.UIAlias, tempType);
            }
        }

        protected virtual string ReplaceResUrl(string str)
        {
            return str.Replace("[L]", FW.FwData.CurrentLanguageStr);
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

        protected virtual void SortUI(List<UIInfo> showUIInfos)
        {
            var showUIInfosCopy = showUIInfos.ToArray();

            Array.Sort(showUIInfosCopy, (x, y) =>
            {
                if (x.Priority - y.Priority == 0) return 0;
                return x.Priority - y.Priority > 0 ? -1 : 1;
            });

            for (var i = 0; i < showUIInfosCopy.Length; i++)
            {
                var uiInfo = showUIInfos[i];
                if (uiInfo.UIBase != null)
                {
                    uiInfo.UIBase.transform.SetSiblingIndex(i);
                }
            }
        }

        /// <summary>
        /// 获取优先级分组,即ui组的数值
        /// </summary>
        /// <param name="priority">优先级</param>
        /// <returns></returns>
        protected virtual uint GetGroupId(uint priority)
        {
            return priority / 100 * 100;
        }

        protected virtual bool CheckNeedWait(List<UIInfo> showedList, UIOpenType openType, UIGroupSetting setting,
            string uiName)
        {
            if (GroupCanShowNewUI(showedList, setting))
            {
                return false;
            }

            if (openType is UIOpenType.Now or UIOpenType.CoveredNow) return false;

            if (openType == UIOpenType.CoveredOrWait)
            {
                //这个是马上打开,但是如果有相同的页面被打开了,那么就需要等待
                var index = showedList.FindIndex(x => x.UIName == uiName);
                if (index < 0) return false;
            }

            return true;
        }

        protected virtual bool GroupCanShowNewUI(List<UIInfo> showedList, UIGroupSetting setting)
        {
            if (setting.BusyLimit <= 0) return true;
            if (showedList.Count < setting.BusyLimit) return true;
            return false;
        }
    }
}