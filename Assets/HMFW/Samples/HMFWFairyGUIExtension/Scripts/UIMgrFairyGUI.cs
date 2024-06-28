using System.Collections.Generic;
using HMFW.Core;
using System;
using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;

namespace HMFW
{
    /// <summary>
    /// 使用扩展的fairyGUI的管理器,它基于HMFW框架中的UGUI的UIMgr扩展了FairyGUI的使用
    /// 注意,FairyGUI使用OpenTopUI无置顶效果
    /// </summary>
    public class UIMgrFairyGUI : HMFW.UIMgr
    {
        protected override void UITypeDataInit()
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
                    var fguiAttribute =
                        (FGUIResUrlAttribute)Attribute.GetCustomAttribute(tempType, typeof(FGUIResUrlAttribute));
                    if (fguiAttribute == null)
                    {
                        Debug.LogError($"{tempType.FullName} 无指定特性,UI必须添加UGUIResUrlAttribute或者FGUIResUrlAttribute 特性");
                        continue;
                    }

                    _allUIBaseTypes.Add(tempType.FullName, tempType);
                    _allUIAliasTypes.Add(fguiAttribute.UIAlias, tempType);
                    continue;
                }

                _allUIBaseTypes.Add(tempType.FullName, tempType);
                _allUIAliasTypes.Add(attribute.UIAlias, tempType);
            }
        }

        protected override async UniTask<UIBase> OpenUIHandle(Type uiType, Dictionary<string, UIBase> map,
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


            if (!UGUILoadResUrl(uiType, out var uiSystem, out var url, out var preLoadUrlStrings))
            {
                return null;
            }

            if (uiSystem == UISystem.UGUI)
            {
                sortList.Add(uiType.FullName);
            }

            switch (uiSystem)
            {
                case UISystem.UGUI:
                    return await OpenHandleUGUI(url, preLoadUrlStrings, uiType, map, root, sortList, args);
                case UISystem.FairyGui:
                    var fguiAttr =
                        (FGUIResUrlAttribute)Attribute.GetCustomAttribute(uiType, typeof(FGUIResUrlAttribute));
                    return await OpenHandleFairyGUI(fguiAttr.UIName, fguiAttr.PackageName, url, preLoadUrlStrings,
                        fguiAttr.BeFairyBatching, uiType, map, args);
            }

            return default;
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
            {
                switch (uiBase.MyUISystem)
                {
                    case UISystem.UGUI:
                        UnityEngine.Object.Destroy(uiBase.gameObject); //销毁
                        break;
                    case UISystem.FairyGui:


                        var info = uiBase.GetComponent<DisplayObjectInfo>();
                        GObject obj = GRoot.inst.DisplayObjectToGObject(info.displayObject);
                        obj.Dispose();
                        break;
                }
            }
        }

        protected virtual async UniTask<UIBase> OpenHandleFairyGUI(string uiName, string packageName,
            string packageFileURL,
            string[] dependencyPackagesFileUrl,
            bool beFairyGUIBatching,
            Type uiType,
            Dictionary<string, UIBase> map,
            params System.Object[] args)
        {
            if (dependencyPackagesFileUrl != null && dependencyPackagesFileUrl.Length > 0)
            {
                UniTask[] all = new UniTask[dependencyPackagesFileUrl.Length];
                for (int i = 0; i < dependencyPackagesFileUrl.Length; i++)
                {
                    var u = LoadPackage(dependencyPackagesFileUrl[i]);
                    all[i] = (u);
                }

                await UniTask.WhenAll(all);
            }

            await LoadPackage(packageFileURL);
            bool beLoad = false;
            GComponent ui = null;
            UIPackage.CreateObjectAsync(packageName, uiName, homeUI =>
            {
                GRoot.inst.AddChild(homeUI);
                ui = homeUI.asCom;
                ui.fairyBatching = beFairyGUIBatching;
                beLoad = true;
            });
            await UniTask.WaitUntil(() => beLoad);

            if (uiType.FullName != null && map.TryGetValue(uiType.FullName, out var uiComTemp1))
            {
                uiComTemp1.gameObject.SetActive(false);
                uiComTemp1.gameObject.SetActive(true);
                await uiComTemp1.OnUIOpen(args);
                return uiComTemp1;
            }


            var transform = ui.displayObject.gameObject.transform;

            var uiCom = transform.GetComponent(uiType) as UIBase;

            if (uiCom == null) uiCom = transform.gameObject.AddComponent(uiType) as UIBase;
            map.Add(uiType.FullName, uiCom);
            if (uiCom != null) await uiCom.OnUIOpen(args);
            return map[uiType.FullName];
        }

        public async UniTask LoadPackage(string packagePath)
        {
            var descDataAssetUIHome =
                await FW.AssetsMgr.LoadAsync<TextAsset>($"{packagePath}_fui.bytes");
            var descData = descDataAssetUIHome.bytes;

            UIPackage.AddPackage(descData, packagePath, OnLoadResourceAsync);
        }

        public async void OnLoadResourceAsync(string name, string extension, Type type, PackageItem item)
        {
            Debug.Log($"Fgui异步加载资源 {name},扩展名 {extension},类型 {type.FullName},文件url {item.file}");
            var obj = await FW.AssetsMgr.LoadAsync<UnityEngine.Object>(item.file);
            item.owner.SetItemAsset(item, obj, DestroyMethod.None);
        }

        /// <summary>获得某个UI预制体的资源加载地址</summary>
        protected override bool UGUILoadResUrl(Type uiType, out UISystem uiSystem, out string resUrl,
            out string[] preLoadUrlStrings)
        {
            var attribute =
                (UGUIAttribute)Attribute.GetCustomAttribute(uiType, typeof(UGUIAttribute));
            if (attribute == null)
            {
                var fguiAttr = (FGUIResUrlAttribute)Attribute.GetCustomAttribute(uiType, typeof(FGUIResUrlAttribute));
                if (fguiAttr == null)
                {
                    Debug.LogErrorFormat("{0}类型未定义UGUIResUrl特性,请定义后再试", uiType.FullName);
                    uiSystem = UISystem.Error;
                    resUrl = null;
                    preLoadUrlStrings = null;
                    return false;
                }

                uiSystem = UISystem.FairyGui;
                resUrl = ReplaceResUrl(fguiAttr.PackageFileUrl);
                preLoadUrlStrings = fguiAttr.DependencyPackagesFileUrl;
                return true;
            }

            uiSystem = UISystem.UGUI;

            resUrl = ReplaceResUrl(attribute.UILoadUrl);

            preLoadUrlStrings = attribute.PreloadResUrl;

            return true;
        }
    }
}