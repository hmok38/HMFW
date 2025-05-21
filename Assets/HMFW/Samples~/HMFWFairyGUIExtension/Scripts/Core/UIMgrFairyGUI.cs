using System.Collections.Generic;
using HMFW.Core;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace HMFW
{
    /// <summary>
    /// 使用扩展的fairyGUI的管理器,它基于HMFW框架中的UGUI的UIMgr扩展了FairyGUI的使用
    /// 注意,FairyGUI使用OpenTopUI无置顶效果
    /// </summary>
    public class UIMgrFairyGUI : HMFW.UIMgr
    {
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

            //创建groupRoot
            var group100Tr = CreatFairyGuiGroupRoot(100, out var group100);
            var group0Tr = CreatFairyGuiGroupRoot(0, out var group0Gc);

            UIGroupSettings = new Dictionary<uint, UIGroupSetting>
            {
                {
                    0, new FairyGuiGroupSetting()
                    {
                        GroupId = 0, GroupRoot = MyUGUIRoot.Find($"{UIGroupRootName}0"),
                        FguiGroupRootGComponent = group0Gc,
                        FguiGroupRoot = group0Tr
                    }
                },
                {
                    100,
                    new FairyGuiGroupSetting()
                    {
                        GroupId = 100, GroupRoot = MyUGUIRoot.Find($"{UIGroupRootName}100"),
                        FguiGroupRootGComponent = group100,
                        FguiGroupRoot = group100Tr
                    }
                }
            };
            SortFairyGuiGroup();
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

            var group100Tr = CreatFairyGuiGroupRoot(groupId, out var group100);
            var setting = new FairyGuiGroupSetting()
            {
                GroupId = groupId, GroupRoot = CreatGroupRoot(groupId),
                FguiGroupRootGComponent = group100,
                FguiGroupRoot = group100Tr
            };
            this.UIGroupSettings.Add(groupId, setting);
            SortFairyGuiGroup();
            return setting;
        }

        protected virtual Transform CreatFairyGuiGroupRoot(uint groupId, out GComponent gComponent)
        {
            gComponent = new GComponent();
            gComponent.name = $"{UIGroupRootName}{groupId}";
            gComponent.gameObjectName = $"{UIGroupRootName}{groupId}";
            GRoot.inst.AddChild(gComponent);
            return gComponent.displayObject.gameObject.transform;
        }

        protected virtual void SortFairyGuiGroup()
        {
            var uints = this.UIGroupSettings.Keys.ToList();
            uints.Sort();
            for (int i = 0; i < uints.Count; i++)
            {
                var key = uints[i];
                if (!this.UIGroupSettings.ContainsKey(key) || this.UIGroupSettings[key] == null)
                {
                    continue;
                }

                var fGuiSetting = this.UIGroupSettings[uints[i]] as FairyGuiGroupSetting;
                fGuiSetting.FguiGroupRoot.SetSiblingIndex(i);
                fGuiSetting.FguiGroupRootGComponent.parent.SetChildIndex(fGuiSetting.FguiGroupRootGComponent, i);
            }
        }

        protected override async UniTask<UIInfo> OpenHandleByUISystem(UIAttribute attribute, UIInfo uiInfo,
            List<UIInfo> showedList, UIGroupSetting groupSetting)
        {
            switch (attribute.UISystem)
            {
                case UISystem.UGUI:
                    await OpenHandleUGUI(attribute, uiInfo, showedList, groupSetting);
                    break;
                case UISystem.FairyGui:
                    await OpenHandleFairyGUI(attribute, uiInfo, showedList, groupSetting);
                    break;
            }

            return uiInfo;
        }

        protected virtual async UniTask<UIInfo> OpenHandleFairyGUI(UIAttribute attribute, UIInfo uiInfo,
            List<UIInfo> showedList, UIGroupSetting groupSetting)
        {
            FGUIResUrlAttribute fguiResUrlAttribute = attribute as FGUIResUrlAttribute;
            var preLoadUrlStrings = ReplaceUrl(fguiResUrlAttribute.DependencyPackagesFileUrl);
            uiInfo.UIState = UIState.Loading;
            showedList.Add(uiInfo);
            //开始加载就添加到显示列表中了
            if (preLoadUrlStrings != null && preLoadUrlStrings.Length > 0)
            {
                UniTask[] all = new UniTask[preLoadUrlStrings.Length];
                for (int i = 0; i < preLoadUrlStrings.Length; i++)
                {
                    var u = FW.CustomAPI.FguiHelper().LoadPackage(preLoadUrlStrings[i]);
                    all[i] = (u);
                }

                await UniTask.WhenAll(all);
            }

            var packageFileURL = ReplaceUrl(fguiResUrlAttribute.PackageFileUrl);
            await FW.CustomAPI.FguiHelper().LoadPackage(packageFileURL);


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


            GComponent ui = null;
            FairyGuiGroupSetting fairyGuiGroupSetting = groupSetting as FairyGuiGroupSetting;

            var homeUI = UIPackage.CreateObject(fguiResUrlAttribute.PackageName, fguiResUrlAttribute.UIName);
            ui = homeUI.asCom;
            ui.SetSize(GRoot.inst.width, GRoot.inst.height);
            fairyGuiGroupSetting.FguiGroupRootGComponent.AddChild(homeUI);
            ui.fairyBatching = fguiResUrlAttribute.BeFairyBatching;
            ui.displayObject.gameObject.SetActive(false);
            //await结束后,检查一下uiInfo是否还存在,如果在这个过程总被关闭了,就不要实例化了.
            if (!showedList.Contains(uiInfo))
            {
                //没包含了,代表在加载的过程总被关闭了,那就不显示了吧
                if (uiInfo.UIState == UIState.Loading)
                {
                    uiInfo.UIState = UIState.Destroy;
                }

                ui.Dispose();
                //  Object.Destroy(ui.displayObject.gameObject);
                return uiInfo;
            }


            ui.displayObject.gameObject.SetActive(true);
            var transform = ui.displayObject.gameObject.transform;

            var uiCom = transform.GetComponent(uiInfo.UIType) as FairyGUIBase;
            if (uiCom == null) uiCom = transform.gameObject.AddComponent(uiInfo.UIType) as FairyGUIBase;
            uiCom.MyGObject = ui as GObject;
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

            FW.GEventMgr.Trigger(UIMgrBase.UiOpenedEventInternal, uiInfo.UIName, uiInfo.Priority);
            return uiInfo;
        }

        protected override void SortUI(List<UIInfo> showUIInfos)
        {
            var showUIInfosCopy = showUIInfos.ToArray();

            Array.Sort(showUIInfosCopy, (x, y) =>
            {
                if (x.Priority - y.Priority == 0) return 0;
                return x.Priority - y.Priority > 0 ? 1 : -1;
            });
            int uGuiIndex = 0;
            int fguiIndex = 0;
            for (var i = 0; i < showUIInfosCopy.Length; i++)
            {
                var uiInfo = showUIInfosCopy[i];
                var attrbute = Attribute.GetCustomAttribute(uiInfo.UIType, typeof(UIAttribute)) as UIAttribute;
                if (attrbute.UISystem == UISystem.UGUI)
                {
                    if (uiInfo.UIBase != null)
                    {
                        uiInfo.UIBase.transform.SetSiblingIndex(uGuiIndex);
                        uGuiIndex++;
                    }
                }
                else if (attrbute.UISystem == UISystem.FairyGui)
                {
                    if (uiInfo.UIBase != null)
                    {
                        var fairyGUIBase = uiInfo.UIBase as FairyGUIBase;

                        fairyGUIBase.MyGObject.parent.SetChildIndex(fairyGUIBase.MyGObject, fguiIndex);
                        fairyGUIBase.transform.SetSiblingIndex(fguiIndex);
                        fguiIndex++;
                    }
                }
            }
        }

        protected override async UniTask<UIInfo> CloseUIByUISystem(UIInfo uiInfo, object[] args)
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
                case UISystem.FairyGui: return await CloseUIHandleFairyUGUI(uiInfo, args);
            }

            return default;
        }

        protected virtual async UniTask<UIInfo> CloseUIHandleFairyUGUI(UIInfo uiInfo, object[] args)
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
                var fgui = uiInfo.UIBase as FairyGUIBase;
                if (fgui != null)
                {
                    fgui.MyGObject?.Dispose();
                }
                else
                {
                    Object.Destroy(uiInfo.UIBase.gameObject);
                }


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
    }
}