using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using HMFW.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace HMFW
{
    public class UIMgr : UIMgrBase
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

        /// <summary>
        /// UI根节点
        /// </summary>
        protected Transform MyUGUIRoot;

        public override Transform UGUIRoot
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

        protected GameObject _uIGroupDemo;

        #region ----------Public Method----------------------------------

        protected void OnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            for (int i = 0; i < Camera.allCameras.Length; i++)
            {
                var camera = Camera.allCameras[i];
                if (camera == _uguiMyUICamera) continue;
                var ad = camera.GetUniversalAdditionalCameraData();
                if (ad != null && ad.renderType == CameraRenderType.Base)
                {
                    ad.cameraStack.Add(_uguiMyUICamera);
                }
            }
        }

        public override void Init()
        {
            GameObject rootTeam = null;
            if (MyUGUIRoot == null)
            {
                var prefab = UnityEngine.Resources.Load<GameObject>(UGUIRootResourcesPath);
                rootTeam = UnityEngine.Object.Instantiate(prefab);
                UnityEngine.Object.DontDestroyOnLoad(rootTeam);

                _uguiMyUICamera = rootTeam.transform.Find("UICamera").GetComponent<Camera>();
                MyUGuiRootCanvasTr = rootTeam.transform.Find("RootCanvas");
                MyUGuiRootCanvas = MyUGuiRootCanvasTr.GetComponent<Canvas>();
                MyUGuiRootCanvas.renderMode = this.UguiRenderMode;
                if (MyUGuiRootCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    SceneManager.activeSceneChanged += OnActiveSceneChanged;
                    _uguiMyUICamera.gameObject.SetActive(true);
                    MyUGuiRootCanvas.worldCamera = _uguiMyUICamera;
                    MyUGuiRootCanvas.sortingLayerID = this.UguiSortingLayer;
                    MyUGuiRootCanvas.sortingOrder = this.UguiOrderInLayer;
                }
                else
                {
                    _uguiMyUICamera.gameObject.SetActive(false);
                }

                var eventObj = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                if (eventObj == null)
                {
                    var input = new GameObject("EventSystem")
                        .AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    eventObj = input.gameObject.GetComponent<UnityEngine.EventSystems.EventSystem>();
                }

                UnityEngine.Object.DontDestroyOnLoad(eventObj.gameObject);
                MyUGUIRoot = rootTeam.transform;
                _uIGroupDemo = MyUGuiRootCanvasTr.Find("UIGroupDemo").gameObject;
                _uIGroupDemo.gameObject.SetActive(false); //关闭
            }

            UIGroupSettings = new Dictionary<uint, UIGroupSetting>
            {
                { 0, new UIGroupSetting() { GroupId = 0, GroupRoot = MyUGuiRootCanvasTr.Find($"{UIGroupRootName}0") } },
                {
                    100,
                    new UIGroupSetting() { GroupId = 100, GroupRoot = MyUGuiRootCanvasTr.Find($"{UIGroupRootName}100") }
                }
            };

            Inited = true;
        }


        public override UIGroupSetting GetGroupSetting(uint priorityBase)
        {
            if (!Inited)
            {
                this.UITypeDataInit();
            }

            var groupId = GetGroupId(priorityBase);
            if (this.UIGroupSettings.TryGetValue(groupId, out var groupSetting))
            {
                return groupSetting;
            }

            var setting = new UIGroupSetting() { GroupId = groupId, GroupRoot = CreatGroupRoot(groupId) };
            this.UIGroupSettings.Add(groupId, setting);
            return setting;
        }

        public override async UniTask<UIInfo> OpenUI(string uiNameOrAlias, uint priority = 100,
            UIOpenType uiOpenType = UIOpenType.Wait, params object[] args)
        {
            if (NeedDebugInfo)
            {
                Debug.Log($"OpenUI {uiNameOrAlias} @ {priority} with {uiOpenType} ");
            }

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

            var rv = await OpenUIByUIInfo(uiInfo);
            ShowUIMapDebugInfo();
            return rv;
        }

        public override async UniTask<UIInfo> CloseUI(UIBase uiBase, params object[] args)
        {
            if (NeedDebugInfo)
            {
                Debug.Log($"CloseUI {uiBase.GetType().FullName} ");
            }

            if (!Inited)
            {
                this.UITypeDataInit();
            }

            var uiType = uiBase.GetType();

            if (NameToUIMap.TryGetValue(uiType.FullName, out var uiInfos))
            {
                if (uiInfos.Count <= 0) return default;
                var uiB = uiInfos.Find(x => x.UIBase == uiBase);

                if (uiB != null)
                {
                    var rv = await CloseUI(uiB.UIName, uiB.UIId, args);

                    ShowUIMapDebugInfo();
                    return rv;
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
        public override async UniTask<UIInfo> CloseUI(string uiNameOrAlias, uint uiId = 0, params object[] args)
        {
            if (NeedDebugInfo)
            {
                Debug.Log($"CloseUI {uiNameOrAlias} uiId:{uiId}");
            }

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
                        var rv = await CloseUIByUISystem(uiB, args);
                        ShowUIMapDebugInfo();
                        return rv;
                    }
                }
                else
                {
                    //没有指定uiid的话,优先关闭正在显示的,
                    var uib = uiInfos.Find(x =>
                        x.UIState == UIState.Loading || x.UIState == UIState.Show || x.UIState == UIState.Hide);

                    if (uib == null)
                    {
                        uib = uiInfos[0];
                    }

                    uiInfos.Remove(uib);
                    var rv = await CloseUIByUISystem(uib, args);
                    ShowUIMapDebugInfo();
                    return rv;
                }
            }

            return default;
        }

        public override async UniTask CloseUIGroup(uint priorityBase, UICloseType uiCloseType = UICloseType.All)
        {
            if (NeedDebugInfo)
            {
                Debug.Log($"CloseUIGroup group:{priorityBase} with {uiCloseType}");
            }

            if (!Inited)
            {
                this.UITypeDataInit();
            }

            List<UIInfo> needDelUIInfos = new List<UIInfo>();
            var groupIn = GetGroupId(priorityBase);
            if (UIInGroupMap.TryGetValue(groupIn, out var showedUI))
            {
                if (uiCloseType is UICloseType.All or UICloseType.Showed)
                {
                    needDelUIInfos.AddRange(showedUI);
                }
            }

            if (WaitingUIMap.TryGetValue(groupIn, out var waitUI))
            {
                if (uiCloseType is UICloseType.All or UICloseType.Waiting)
                {
                    needDelUIInfos.AddRange(waitUI);
                }
            }

            var list = new List<UniTask>(needDelUIInfos.Count);
            for (int i = 0; i < needDelUIInfos.Count; i++)
            {
                var task = CloseUIByUISystem(needDelUIInfos[i], null);
                list.Add(task);
            }

            await UniTask.WhenAll(list);
            ShowUIMapDebugInfo();
        }

        public override async UniTask CloseAllUI(UICloseType uiCloseType = UICloseType.All, string[] excludedUIs = null)
        {
            if (NeedDebugInfo)
            {
                Debug.Log($"CloseAllUI excludedUIs:{excludedUIs.Length} with {uiCloseType}");
            }

            if (!Inited)
            {
                this.UITypeDataInit();
            }

            List<UIInfo> needDelUIInfos = new List<UIInfo>();
            string[] excludedUIType = new String[excludedUIs != null ? excludedUIs.Length : 0];
            if (excludedUIs != null && excludedUIs.Length > 0)
            {
                for (int i = 0; i < excludedUIs.Length; i++)
                {
                    Type uiType = GetUIDataType(excludedUIs[i]);
                    if (uiType != null)
                    {
                        excludedUIType[i] = uiType.FullName;
                    }
                }
            }


            foreach (var kv in NameToUIMap)
            {
                if (kv.Value == null || kv.Value.Count <= 0) continue;
                if (!excludedUIType.Contains(kv.Key))
                {
                    if (uiCloseType == UICloseType.All)
                    {
                        needDelUIInfos.AddRange(kv.Value);
                    }
                    else
                    {
                        for (int i = 0; i < kv.Value.Count; i++)
                        {
                            var info = kv.Value[i];
                            if (uiCloseType == UICloseType.Showed)
                            {
                                if (info.UIState is UIState.Show or UIState.Hide or UIState.Loading)
                                {
                                    needDelUIInfos.Add(info);
                                }
                            }
                            else if (uiCloseType == UICloseType.Waiting)
                            {
                                if (info.UIState is UIState.Wait)
                                {
                                    needDelUIInfos.Add(info);
                                }
                            }
                        }
                    }
                }
            }

            var list = new List<UniTask>(needDelUIInfos.Count);
            for (int i = 0; i < needDelUIInfos.Count; i++)
            {
                var task = CloseUIByUISystem(needDelUIInfos[i], null);
                list.Add(task);
            }

            await UniTask.WhenAll(list);
            ShowUIMapDebugInfo();
        }

        /// <summary>
        /// 获取UIInfo,如果不存在则代表未打开
        /// 注意:如果uiInfo存在也请检查其UIState状态
        /// 严重注意:禁止直接调用uiBase接口,需要调用请传事件
        /// </summary>
        /// <param name="uiNameOrAlias">要查找的ui的名字</param>
        /// <param name="uiId">可多实例的ui的Id,传默认0则代表这个ui不是多实例ui</param>
        /// <returns></returns>
        public override UIInfo GetUIInfo(string uiNameOrAlias, uint uiId = 0)
        {
            var uiType = this.GetUIDataType(uiNameOrAlias);
            if (uiType == null || string.IsNullOrEmpty(uiType.FullName)) return null;
            if (NameToUIMap.TryGetValue(uiType.FullName, out var uiInfos))
            {
                if (uiInfos.Count <= 0) return default; //没找到
                if (uiId == 0) //UIID永远不会是0,如果传0代表不是多实例的ui,那么就返回第一个就行
                {
                    return uiInfos[0];
                }

                //如果传了uiId那么就必须匹配uiId
                var uiB = uiInfos.Find(x => x.UIId == uiId);
                return uiB;
            }

            return null;
        }

        #endregion

        protected virtual async UniTask<UIInfo> OpenUIByUIInfo(UIInfo uiInfo, bool beCheckWait = false)
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
                    if (uiInfo.UIOpenType == UIOpenType.Normal)
                    {
                        uiInfo.UIState = UIState.Error;
                        return uiInfo;
                    }

                    //需要等待就添加进来,然后等待有页面被关闭后再次打开
                    if (uiInfo.UIOpenType is UIOpenType.WaitFirst or UIOpenType.CoveredOrWait)
                    {
                        waitingList.Insert(0, uiInfo); //first就插入最前面
                    }
                    else
                    {
                        waitingList.Add(uiInfo);
                    }

                    if (!NameToUIMap.ContainsKey(uiInfo.UIName))
                    {
                        NameToUIMap.Add(uiInfo.UIName, new List<UIInfo>());
                    }

                    NameToUIMap[uiInfo.UIName].Add(uiInfo);
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

            return await OpenHandleByUISystem(attribute, uiInfo, showedList, groupSetting);
        }

        protected virtual async UniTask<UIInfo> OpenHandleByUISystem(UIAttribute attribute, UIInfo uiInfo,
            List<UIInfo> showedList, UIGroupSetting groupSetting)
        {
            switch (attribute.UISystem)
            {
                case UISystem.UGUI:
                    await OpenHandleUGUI(attribute, uiInfo, showedList, groupSetting);
                    break;
            }

            return uiInfo;
        }

        protected virtual async UniTask<UIInfo> CheckWaitUI(UIGroupSetting groupSetting)
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

        protected virtual async UniTask<UIInfo> CloseUIByUISystem(UIInfo uiInfo, object[] args)
        {
            if (uiInfo == null)
            {
                Debug.LogError($"CloseUIHandle 中 uiInfo不能为空");
                return default;
            }

            UIAttribute uiAttribute = Attribute.GetCustomAttribute(uiInfo.UIType, typeof(UIAttribute)) as UIAttribute;

            if (uiAttribute == null)
            {
                Debug.LogError($"CloseUIHandle 中 uiInfo类型: {uiInfo.UIType} 无UIAttribute特性");
                return default;
            }

            switch (uiAttribute.UISystem)
            {
                case UISystem.UGUI: return await CloseUIHandleUGUI(uiInfo, args);
            }

            return default;
        }

        protected virtual async UniTask<UIInfo> CloseUIHandleUGUI(UIInfo uiInfo, object[] args)
        {
            if (uiInfo == null) return default;
            if (uiInfo.UIState == UIState.Destroy) return uiInfo;
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

            uiInfo.UIState = UIState.Destroy;
            if (uiInfo.UIBase != null)
            {
                await uiInfo.UIBase.OnUIClose(args);
                if (uiInfo.UIBase.beBackBtnQueueUI) FW.BackBtnQueueMgr.RemoveQueue(uiInfo.UIBase);
                Object.Destroy(uiInfo.UIBase.gameObject);
                uiInfo.UIBase = null;
                FW.GEventMgr.Trigger(UIMgr.UiCloseEventInternal, uiInfo.UIName, uiInfo.Priority);
            }

            if (uiInfo.UIOpenType is UIOpenType.CoveredNow or UIOpenType.CoveredOrWait)
            {
                //打开同组的其他ui显示
                if (UIInGroupMap.TryGetValue(groupId, out var groupUI))
                {
                    for (int i = 0; i < groupUI.Count; i++)
                    {
                        var uiTemp = groupUI[i];
                        if (uiTemp.UIState == UIState.Hide && uiTemp.UIBase != null)
                        {
                            uiTemp.UIState = UIState.Show;
                            uiTemp.UIBase.gameObject.SetActive(true);
                        }
                    }
                }
            }

            var setting = GetGroupSetting(uiInfo.Priority);
            await CheckWaitUI(setting);
            return uiInfo;
        }

        protected virtual Transform CreatGroupRoot(uint groupId)
        {
            int newIndex = MyUGuiRootCanvasTr.childCount;
            for (int i = 0; i < MyUGuiRootCanvasTr.childCount; i++)
            {
                var name = MyUGuiRootCanvasTr.GetChild(i).name;
                if (name.Equals("UIGroupDemo")) continue;
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
                    Debug.Log($"请移除 {MyUGuiRootCanvasTr.name} Ui根节点下的 {name} 物体,ui根节点下只能自动创建ui组节点");
                }
            }

            var tr = MyUGuiRootCanvasTr.Find($"{UIGroupRootName}{groupId}");
            if (tr == null)
            {
                tr = Object.Instantiate(_uIGroupDemo, MyUGuiRootCanvasTr).transform;
                tr.name = $"{UIGroupRootName}{groupId}";
                tr.SetSiblingIndex(newIndex);
                tr.gameObject.SetActive(true);
                tr.GetComponent<Canvas>().sortingOrder = (int)groupId;
            }

            return tr;
        }

        protected virtual async UniTask<UIInfo> OpenHandleUGUI(UIAttribute attribute, UIInfo uiInfo,
            List<UIInfo> showedList, UIGroupSetting groupSetting)
        {
            UGUIResAttribute uguiResAttribute = attribute as UGUIResAttribute;

            var preLoadUrlStrings = ReplaceUrl(uguiResAttribute.PreloadResUrl);
            uiInfo.UIState = UIState.Loading;
            showedList.Add(uiInfo);
            //开始加载就添加到显示列表中了
            if (preLoadUrlStrings != null && preLoadUrlStrings.Length > 0)
                await FW.AssetsMgr.LoadAssetsAsync<UnityEngine.Object>(preLoadUrlStrings.ToList());

            var uiPrefab = await FW.AssetsMgr.LoadAsync<GameObject>(ReplaceUrl(uguiResAttribute.UILoadUrl));

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


            var ui = UnityEngine.Object.Instantiate(uiPrefab, groupSetting.GroupRoot) as GameObject;
            var rectTransform = ui.transform as RectTransform;
            ResetRectTransform(rectTransform);

            var uiCom = ui.GetComponent(uiInfo.UIType) as UIBase;
            if (uiCom == null) uiCom = ui.AddComponent(uiInfo.UIType) as UIBase;
            uiInfo.UIBase = uiCom;
            uiCom.UiInfo = uiInfo;
            SortUI(showedList);

            if (uiInfo.UIState == UIState.Loading)
            {
                uiInfo.UIState = UIState.Show;
            }
            else if (uiInfo.UIState == UIState.Hide) //在加载的过程总,被其他ui强制hide了
            {
                uiCom.gameObject.SetActive(false); //关闭显示
            }

            //触发ui被打开前的事件
            FW.GEventMgr.Trigger(UIMgr.UiPreOpenEventInternal, uiInfo.UIName, uiInfo.Priority);
            if (uiCom != null) await uiCom.OnUIOpen(uiInfo.Arg);

            if (uiCom.beBackBtnQueueUI) FW.BackBtnQueueMgr.AddToQueue(uiCom);

            if (uiInfo.UIOpenType is UIOpenType.CoveredNow or UIOpenType.CoveredOrWait)
            {
                for (int i = 0; i < showedList.Count; i++)
                {
                    var tempUI = showedList[i];
                    if (tempUI != uiInfo &&
                        (tempUI.UIState is UIState.Show) &&
                        tempUI.UIBase != null)
                    {
                        tempUI.UIState = UIState.Hide;
                        tempUI.UIBase.gameObject.SetActive(false);
                    }
                    else if (tempUI != uiInfo && (tempUI.UIState is UIState.Loading))
                    {
                        tempUI.UIState = UIState.Hide;
                        if (tempUI.UIBase != null)
                        {
                            tempUI.UIBase.gameObject.SetActive(false);
                        }
                    }
                }
            }

            FW.GEventMgr.Trigger(UIMgr.UiOpenedEventInternal, uiInfo.UIName, uiInfo.Priority);
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
                return x.Priority - y.Priority > 0 ? 1 : -1;
            });

            for (var i = 0; i < showUIInfosCopy.Length; i++)
            {
                var uiInfo = showUIInfosCopy[i];
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

        protected virtual void ShowUIMapDebugInfo()
        {
            if (NeedDebugInfo)
            {
                StringBuilder allMapinfoB = new StringBuilder();
                allMapinfoB.Append("UI系统数据信息: NameToUIMap中数据:");
                foreach (var kv in NameToUIMap)
                {
                    if (kv.Value.Count > 0)
                    {
                        allMapinfoB.Append($"\n{kv.Value.Count} 个 {kv.Key}");
                    }
                }

                allMapinfoB.Append("\nUIInGroupMap 中数据:");
                foreach (var kv in UIInGroupMap)
                {
                    if (kv.Value.Count > 0)
                    {
                        allMapinfoB.Append($"\n {kv.Key} 组中显示的ui数量:{kv.Value.Count}");
                    }
                }

                allMapinfoB.Append("\nWaitingUIMap 中数据:");
                foreach (var kv in WaitingUIMap)
                {
                    if (kv.Value.Count > 0)
                    {
                        allMapinfoB.Append($"\n {kv.Key} 组中等待的ui数量:{kv.Value.Count}");
                    }
                }

                Debug.Log(allMapinfoB.ToString());
            }
        }
    }

    /// <summary>
    /// UI管理器基类
    /// </summary>
    public abstract class UIMgrBase
    {
        /// <summary>
        /// ui即将被打开,在ui的onAwake之后onOpen之前被调用,监听此事件可获得ui被打开的回调,参数为 uiName(字符串) uiPriority(uint)
        /// </summary>
        public readonly string UiPreOpenEvent = "UiPreOpenEvent";

        /// <summary>
        /// ui已经被打开,在ui的onOpen之后被调用,监听此事件可获得ui被打开的回调,参数为 uiName(字符串) uiPriority(uint)
        /// </summary>
        public readonly string UiOpenedEvent = "UiOpenedEvent";

        /// <summary>
        /// ui被关闭,监听此事件可获得ui被打开的回调,参数为 uiName(字符串) uiPriority(uint)
        /// </summary>
        public readonly string UiCloseEvent = "UiCloseEvent";


        /// <summary>
        /// ui即将被打开,在ui的onAwake之后onOpen之前被调用,监听此事件可获得ui被打开的回调,参数为 uiName(字符串) uiPriority(uint)
        /// </summary>
        protected const string UiPreOpenEventInternal = "UiPreOpenEvent";

        /// <summary>
        /// ui已经被打开,在ui的onOpen之后被调用,监听此事件可获得ui被打开的回调,参数为 uiName(字符串) uiPriority(uint)
        /// </summary>
        protected const string UiOpenedEventInternal = "UiOpenedEvent";

        /// <summary>
        /// ui被关闭,监听此事件可获得ui被打开的回调,参数为 uiName(字符串) uiPriority(uint)
        /// </summary>
        protected const string UiCloseEventInternal = "UiCloseEvent";

        /// <summary>
        /// UGUIRoot的资源路径，必须是Resources目录下的资源，请参考框架Resources/FWPrefabs/UGUIRoot预制体
        /// </summary>
        public string UGUIRootResourcesPath = "FWPrefabs/UGUIRoot";

        /// <summary>
        /// 画布的渲染模式===仅Ugui有效,在第一次使用ui前设置才有效
        /// </summary>
        public RenderMode UguiRenderMode = RenderMode.ScreenSpaceCamera;

        /// <summary>
        /// 画布根节点的默认sortLayer--仅Ugui有效,在第一次使用ui前设置才有效
        /// </summary>
        public int UguiSortingLayer = 128;

        /// <summary>
        /// 画布的排序--仅Ugui有效,在第一次使用ui前设置才有效
        /// </summary>
        public int UguiOrderInLayer = 1000;

        /// <summary>
        /// 结构带的ui相机--仅Ugui有效
        /// </summary>
        protected Camera _uguiMyUICamera;

        /// <summary>
        /// ui摄像机--非ScreenSpaceCamera模式时请不要每帧频繁调用--仅Ugui有效
        /// </summary>
        public Camera UguiUICamera
        {
            get
            {
                if (this.UguiRenderMode == RenderMode.ScreenSpaceCamera) return this._uguiMyUICamera;
                return Camera.main;
            }
        }

        protected readonly Dictionary<string, string> UrlReplaceMap = new Dictionary<string, string>();

        /// <summary>
        /// 设置是否需要输出ui系统的信息
        /// </summary>
        public bool NeedDebugInfo = true;

        /// <summary>
        /// Ugui的根节点
        /// </summary>
        public abstract Transform UGUIRoot { get; }

        /// <summary>
        /// 初始化UI,主要是自定义UIRoot对象用的,否则可以不单独调用,
        /// 如果要自定义UIRoot,那么请在开启任何UI之前调用此Init
        /// </summary>
        /// <returns></returns>
        public abstract void Init();


        /// <summary>
        /// 获取某个ui组的设置文件，如果不存在则创建并保存
        /// </summary>
        /// <param name="priorityBase"></param>
        /// <returns></returns>
        public abstract UIGroupSetting GetGroupSetting(uint priorityBase);

        /// <summary>
        /// 开启ui
        /// </summary>
        /// <param name="uiNameOrAlias">ui的类fullName或者别名</param>
        /// <param name="priority">优先级,每100优先级为一个ui组,组内按照优先级确定显示顺序,组序号是0,100,200,序号越大越上层</param>
        /// <param name="uiOpenType">打开ui的方式,可以普通打开,等待打开,立刻打开,还有覆盖打开等</param>
        /// <param name="args">需要传入ui脚本中 onUiOpen函数的参数</param>
        /// <returns>返回ui的信息,但不要通过uibase进行任何操作</returns>
        public abstract UniTask<UIInfo> OpenUI(string uiNameOrAlias, uint priority = 100,
            UIOpenType uiOpenType = UIOpenType.Wait, params object[] args);

        /// <summary>
        /// 关闭ui,通过脚本关闭ui,一般在脚本内CloseUi(this)
        /// </summary>
        /// <param name="uiBase"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract UniTask<UIInfo> CloseUI(UIBase uiBase, params object[] args);

        /// <summary>
        /// 关闭ui
        /// </summary>
        /// <param name="uiNameOrAlias">UI的类名或者别名</param>
        /// <param name="uiId">多实例的话请传入要关闭的id,传0会关闭第一个</param>
        /// <param name="args">要传入的参数</param>
        /// <returns></returns>
        public abstract UniTask<UIInfo> CloseUI(string uiNameOrAlias, uint uiId = 0, params object[] args);

        /// <summary>
        /// 根据传入的uiCloseType,关闭某一个ui组的ui
        /// </summary>
        /// <param name="priorityBase"></param>
        /// <param name="uiCloseType"></param>
        public abstract UniTask CloseUIGroup(uint priorityBase, UICloseType uiCloseType = UICloseType.All);

        /// <summary>
        /// 根据传入的uiCloseType 关闭所有的ui,除了excludedUIs包含的ui
        /// </summary>
        /// <param name="uiCloseType">关闭方法</param>
        /// <param name="excludedUIs">不关闭的ui,也可以传null</param>
        /// <returns></returns>
        public abstract UniTask CloseAllUI(UICloseType uiCloseType = UICloseType.All, string[] excludedUIs = null);

        /// <summary>
        /// 设置在加载预制体体或者资源时,需要自动替换的标签和标签内容(如:UGUIAttribute的UILoadUrl),如[L]代表语言目录
        /// 可以根据设定自动加载相应语言目录下的预制体,
        /// </summary>
        /// <param name="stringFlag">需要替换的标签,即ui脚本上设置的UIAttribute中的资源加载目录,尽量使用不会使用的标志如[L]</param>
        /// <param name="replaceStr">需要替换的内容</param>
        public virtual void SetUrlReplace(string stringFlag, string replaceStr)
        {
            if (!UrlReplaceMap.ContainsKey(stringFlag))
            {
                UrlReplaceMap.Add(stringFlag, replaceStr);
            }
            else
            {
                UrlReplaceMap[stringFlag] = replaceStr;
            }
        }

        protected virtual string ReplaceUrl(string inStr)
        {
            var rv = inStr;
            foreach (var kv in UrlReplaceMap)
            {
                rv = rv.Replace(kv.Key, kv.Value);
            }

            return rv;
        }

        protected virtual string[] ReplaceUrl(string[] inStr)
        {
            if (inStr == null || inStr.Length <= 0) return inStr;
            var rv = new string[inStr.Length];
            for (var i = 0; i < rv.Length; i++)
            {
                rv[i] = ReplaceUrl(inStr[i]);
            }

            return rv;
        }

        /// <summary>
        /// 获取UIInfo,如果不存在则代表未打开
        /// 注意:如果uiInfo存在也请检查其UIState状态
        /// 严重注意:禁止直接调用uiBase接口,需要调用请传事件
        /// </summary>
        /// <param name="uiNameOrAlias">要查找的ui的名字</param>
        /// <param name="uiId">可多实例的ui的Id,传默认0则代表这个ui不是多实例ui</param>
        /// <returns></returns>
        public abstract UIInfo GetUIInfo(string uiNameOrAlias, uint uiId = 0);

        /// <summary>
        /// UGUI的根画布Transform(fgui下为空)
        /// </summary>
        public virtual Transform MyUGuiRootCanvasTr { get; protected set; }

        /// <summary>
        /// UGUI的根画布(fgui下为空)
        /// </summary>
        public virtual Canvas MyUGuiRootCanvas { get; protected set; }
    }
}