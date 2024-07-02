using System.Collections.Generic;
using HMFW.Core;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HMFW
{
    /// <summary>
    /// 使用扩展的fairyGUI的管理器,它基于HMFW框架中的UGUI的UIMgr扩展了FairyGUI的使用
    /// 注意,FairyGUI使用OpenTopUI无置顶效果
    /// </summary>
    public class UIMgrFairyGUI : HMFW.UIMgr
    {
        public override void Init(Transform uguiRoot = null)
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
            SortGroup();
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
            SortGroup();
            return setting;
        }

        protected Transform CreatFairyGuiGroupRoot(uint groupId, out GComponent gComponent)
        {
            gComponent = new GComponent();
            gComponent.name = $"{UIGroupRootName}{groupId}";
            gComponent.gameObjectName = $"{UIGroupRootName}{groupId}";
            GRoot.inst.AddChild(gComponent);
            return gComponent.displayObject.gameObject.transform;
        }

        protected void SortGroup()
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
                    var u = LoadPackage(preLoadUrlStrings[i]);
                    all[i] = (u);
                }

                await UniTask.WhenAll(all);
            }

            var packageFileURL = ReplaceUrl(fguiResUrlAttribute.PackageFileUrl);
            await LoadPackage(packageFileURL);


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

            bool beLoad = false;
            GComponent ui = null;
            FairyGuiGroupSetting fairyGuiGroupSetting = groupSetting as FairyGuiGroupSetting;
            UIPackage.CreateObjectAsync(fguiResUrlAttribute.PackageName, fguiResUrlAttribute.UIName, homeUI =>
            {
                fairyGuiGroupSetting.FguiGroupRootGComponent.AddChild(homeUI);
                ui = homeUI.asCom;
                ui.fairyBatching = fguiResUrlAttribute.BeFairyBatching;
                beLoad = true;
            });
            await UniTask.WaitUntil(() => beLoad);

            var transform = ui.displayObject.gameObject.transform;

            var uiCom = transform.GetComponent(uiInfo.UIType) as FairyGUIBase;
            if (uiCom == null) uiCom = transform.gameObject.AddComponent(uiInfo.UIType) as FairyGUIBase;
            uiCom.MyGObject = ui as GObject;

            if (uiInfo.UIState == UIState.Loading)
            {
                uiInfo.UIState = UIState.Show;
            }
            else if (uiInfo.UIState == UIState.Hide) //在加载的过程总,被其他ui强制hide了
            {
                uiCom.gameObject.SetActive(false); //关闭显示
            }

            if (uiCom != null) await uiCom.OnUIOpen(uiInfo.Arg);

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

            return uiInfo;
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


        //
        // /// <summary>
        // /// 关闭ui,会等待OnUIClose函数执行完毕后再销毁ui对象,如果有关闭动画等可以重写ui的OnUIClose函数.
        // /// </summary>
        // /// <param name="uiFullNameOrAliasName"></param>
        // /// <param name="args"></param>
        // public override async UniTask CloseUI(string uiFullNameOrAliasName, params System.Object[] args)
        // {
        //     Type uiType = GetUIDataType(uiFullNameOrAliasName);
        //     UIBase uiBase = null;
        //     if (uiType.FullName != null && _uiMap.ContainsKey(uiType.FullName))
        //     {
        //         uiBase = _uiMap[uiType.FullName];
        //         _uiMap.Remove(uiType.FullName);
        //     }
        //
        //     if (uiType.FullName != null && uiBase == null && _uiTopMap.ContainsKey(uiType.FullName))
        //     {
        //         uiBase = _uiTopMap[uiType.FullName];
        //         _uiTopMap.Remove(uiType.FullName);
        //     }
        //
        //     if (uiBase == null) return;
        //     await uiBase.OnUIClose(args);
        //     if (uiBase != null)
        //     {
        //         switch (uiBase.MyUISystem)
        //         {
        //             case UISystem.UGUI:
        //                 UnityEngine.Object.Destroy(uiBase.gameObject); //销毁
        //                 break;
        //             case UISystem.FairyGui:
        //
        //
        //                 var info = uiBase.GetComponent<DisplayObjectInfo>();
        //                 GObject obj = GRoot.inst.DisplayObjectToGObject(info.displayObject);
        //                 obj.Dispose();
        //                 break;
        //         }
        //     }
        // }
        //
        // protected virtual async UniTask<UIBase> OpenHandleFairyGUI(string uiName, string packageName,
        //     string packageFileURL,
        //     string[] dependencyPackagesFileUrl,
        //     bool beFairyGUIBatching,
        //     Type uiType,
        //     Dictionary<string, UIBase> map,
        //     params System.Object[] args)
        // {
        //     if (dependencyPackagesFileUrl != null && dependencyPackagesFileUrl.Length > 0)
        //     {
        //         UniTask[] all = new UniTask[dependencyPackagesFileUrl.Length];
        //         for (int i = 0; i < dependencyPackagesFileUrl.Length; i++)
        //         {
        //             var u = LoadPackage(dependencyPackagesFileUrl[i]);
        //             all[i] = (u);
        //         }
        //
        //         await UniTask.WhenAll(all);
        //     }
        //
        //     await LoadPackage(packageFileURL);
        //     bool beLoad = false;
        //     GComponent ui = null;
        //     UIPackage.CreateObjectAsync(packageName, uiName, homeUI =>
        //     {
        //         GRoot.inst.AddChild(homeUI);
        //         ui = homeUI.asCom;
        //         ui.fairyBatching = beFairyGUIBatching;
        //         beLoad = true;
        //     });
        //     await UniTask.WaitUntil(() => beLoad);
        //
        //     if (uiType.FullName != null && map.TryGetValue(uiType.FullName, out var uiComTemp1))
        //     {
        //         uiComTemp1.gameObject.SetActive(false);
        //         uiComTemp1.gameObject.SetActive(true);
        //         await uiComTemp1.OnUIOpen(args);
        //         return uiComTemp1;
        //     }
        //
        //
        //     var transform = ui.displayObject.gameObject.transform;
        //
        //     var uiCom = transform.GetComponent(uiType) as UIBase;
        //
        //     if (uiCom == null) uiCom = transform.gameObject.AddComponent(uiType) as UIBase;
        //     map.Add(uiType.FullName, uiCom);
        //     if (uiCom != null) await uiCom.OnUIOpen(args);
        //     return map[uiType.FullName];
        // }
        //
        protected virtual async UniTask LoadPackage(string packagePath)
        {
            var descDataAssetUIHome =
                await FW.AssetsMgr.LoadAsync<TextAsset>($"{packagePath}_fui.bytes");
            var descData = descDataAssetUIHome.bytes;

            UIPackage.AddPackage(descData, packagePath, OnLoadResourceAsync);
        }

        protected virtual async void OnLoadResourceAsync(string name, string extension, Type type, PackageItem item)
        {
            Debug.Log($"Fgui异步加载资源 {name},扩展名 {extension},类型 {type.FullName},文件url {item.file}");
            var obj = await FW.AssetsMgr.LoadAsync<UnityEngine.Object>(item.file);
            item.owner.SetItemAsset(item, obj, DestroyMethod.None);
        }
        //
        // /// <summary>获得某个UI预制体的资源加载地址</summary>
        // protected override bool UGUILoadResUrl(Type uiType, out UISystem uiSystem, out string resUrl,
        //     out string[] preLoadUrlStrings)
        // {
        //     var attribute =
        //         (UGUIAttribute)Attribute.GetCustomAttribute(uiType, typeof(UGUIAttribute));
        //     if (attribute == null)
        //     {
        //         var fguiAttr = (FGUIResUrlAttribute)Attribute.GetCustomAttribute(uiType, typeof(FGUIResUrlAttribute));
        //         if (fguiAttr == null)
        //         {
        //             Debug.LogErrorFormat("{0}类型未定义UGUIResUrl特性,请定义后再试", uiType.FullName);
        //             uiSystem = UISystem.Error;
        //             resUrl = null;
        //             preLoadUrlStrings = null;
        //             return false;
        //         }
        //
        //         uiSystem = UISystem.FairyGui;
        //         resUrl = ReplaceResUrl(fguiAttr.PackageFileUrl);
        //         preLoadUrlStrings = fguiAttr.DependencyPackagesFileUrl;
        //         return true;
        //     }
        //
        //     uiSystem = UISystem.UGUI;
        //
        //     resUrl = ReplaceResUrl(attribute.UILoadUrl);
        //
        //     preLoadUrlStrings = attribute.PreloadResUrl;
        //
        //     return true;
        // }
    }
}