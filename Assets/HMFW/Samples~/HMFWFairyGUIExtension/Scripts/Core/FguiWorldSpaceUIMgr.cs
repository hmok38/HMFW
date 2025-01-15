using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace HMFW
{
    /// <summary>
    /// 基于世界坐标空间的ui元素管理器
    /// </summary>
    public class FguiWorldSpaceUIMgr : MonoBehaviour
    {
        private readonly Dictionary<Transform, List<WorldSpaceUIInfo>> _worldSpaceUIMap =
            new Dictionary<Transform, List<WorldSpaceUIInfo>>();

        private readonly Dictionary<Enum, WorldSpaceUIFguiInfo> _itemTypeFguiInfoMap =
            new Dictionary<Enum, WorldSpaceUIFguiInfo>();

        private readonly Dictionary<Enum, Type> _scriptMap =
            new Dictionary<Enum, Type>();


        private readonly HashSet<Enum> _stackableUI = new HashSet<Enum>();
        private readonly HashSet<Enum> _noManagerUI = new HashSet<Enum>();

        /// <summary>
        /// 初始化世界坐标ui的对象,
        /// </summary>
        /// <param name="uIItemType"></param>
        /// <param name="packageName"></param>
        /// <param name="viewName"></param>
        /// <param name="type"></param>
        public void InitUIItem(Enum uIItemType, string packageName, string viewName, Type type)
        {
            if (_itemTypeFguiInfoMap.ContainsKey(uIItemType)) return;
            _itemTypeFguiInfoMap.Add(uIItemType,
                new WorldSpaceUIFguiInfo() { PackageName = packageName, ViewName = viewName });
            _scriptMap.Add(uIItemType, type);
        }

        /// <summary>
        /// 判断是否可堆叠的ui,可堆叠的ui的参数args第一个必须是字符串类型的id
        /// </summary>
        /// <returns></returns>
        public bool BeStackableUI(Enum uiItemType)
        {
            return _stackableUI.Contains(uiItemType);
        }

        /// <summary>
        /// 设置这个ui类型为可堆叠的ui
        /// </summary>
        /// <param name="uiItemType"></param>
        public void SetUIBeStackable(Enum uiItemType)
        {
            _stackableUI.Add(uiItemType);
        }

        /// <summary>
        /// 判断是否不需要管理的ui,即传来的transform可能为空
        /// </summary>
        /// <param name="uiItemType"></param>
        /// <returns></returns>
        public bool BeNoManagerUI(Enum uiItemType)
        {
            return _noManagerUI.Contains(uiItemType);
        }

        /// <summary>
        /// 设置这个ui类型为不需要管理的ui,即传来的transform可能为空
        /// </summary>
        /// <param name="uiItemType"></param>
        /// <returns></returns>
        public void SetNoManagerUI(Enum uiItemType)
        {
            _noManagerUI.Add(uiItemType);
        }


        /// <summary>
        /// 创建某个世界空间的ui
        /// </summary>
        /// <param name="followTr">跟随的物体,设置为SetNoManagerUI()的ui可以不传</param>
        /// <param name="uiType"></param>
        /// <param name="args">需要传入ui控制类的参数,如果是可堆叠的ui,args得第一位必须是堆叠id(int)</param>
        public void CreatWorldSpaceUI(Transform followTr, Enum uiType, object[] args)
        {
            if (!BeNoManagerUI(uiType))
            {
                if (_worldSpaceUIMap.TryGetValue(followTr, out var uiInfos))
                {
                    if (!BeStackableUI(uiType))
                    {
                        //非堆叠的ui就检查之前有没有
                        var index = uiInfos.FindIndex(x => x.UiType.Equals(uiType));
                        if (index >= 0) //发现这个类型有ui,先销毁掉
                        {
                            var info = uiInfos[index];
                            if (info.WorldSpaceUIItem != null)
                            {
                                UnityEngine.Object.Destroy(info.WorldSpaceUIItem.gameObject);
                            }

                            uiInfos.RemoveAt(index);
                        }
                    }
                    else
                    {
                        //可堆叠的ui就不检查之前有没有了
                    }
                }
                else
                {
                    uiInfos = new List<WorldSpaceUIInfo>(5);
                    _worldSpaceUIMap.Add(followTr, uiInfos);
                }

                var uiInfo = new WorldSpaceUIInfo()
                {
                    Args = args,
                    Follow = followTr,
                    UiType = uiType
                };
                if (BeStackableUI(uiType))
                {
                    if (args.Length <= 0 || args[0] is not string)
                    {
                        Debug.LogError($"WorldSpaceUI系统中的 {uiType} 类型调用时候发现参数(args)的第一个参数不是字符串类型,它应该是堆叠id");
                    }

                    uiInfo.StackId = args[0] as string;
                }

                uiInfos.Add(uiInfo);
                uiInfo.WorldSpaceUIItem = CreatItem(uiInfo);
            }
            else
            {
                var uiInfo = new WorldSpaceUIInfo()
                {
                    Args = args,
                    Follow = followTr,
                    UiType = uiType
                };

                CreatItem(uiInfo);
            }
        }

        /// <summary>
        /// 销毁某个ui,通过传入的跟随的tr来销毁它
        /// </summary>
        /// <param name="followTr"></param>
        /// <param name="uiType"></param>
        /// <param name="args">如果需要销毁的是可堆叠的,那么在args中的第一个必须是stackId(int)</param>
        public void DestroyWorldSpaceUI(Transform followTr, Enum uiType, object[] args)
        {
            if (_worldSpaceUIMap.TryGetValue(followTr, out var uiInfos))
            {
                if (!BeStackableUI(uiType))
                {
                    var index = uiInfos.FindIndex(x => x.UiType.Equals(uiType));
                    if (index >= 0) //发现这个类型有ui,先销毁掉
                    {
                        var info = uiInfos[index];
                        if (info.WorldSpaceUIItem != null)
                        {
                            info.WorldSpaceUIItem.Destroy();
                        }

                        uiInfos.RemoveAt(index);
                    }
                }
                else
                {
                    //移除的时候如果是可堆叠的,就必须要传入stackId才能正确的移除
                    if (args.Length <= 0 || args[0] is not string)
                    {
                        Debug.LogError($"WorldSpaceUI系统中的 {uiType} 类型 销毁 的时候发现参数(args)的第一个参数不是字符串类型,它应该是堆叠id");
                    }

                    string stackId = args[0] as string;

                    var index = uiInfos.FindIndex(x => x.UiType.Equals(uiType) && x.StackId.Equals(stackId));
                    if (index >= 0) //发现这个类型有ui,先销毁掉
                    {
                        var info = uiInfos[index];
                        if (info.WorldSpaceUIItem != null)
                        {
                            info.WorldSpaceUIItem.Destroy();
                        }

                        uiInfos.RemoveAt(index);
                    }
                }
            }
        }

        private WorldSpaceUIItemBase CreatItem(WorldSpaceUIInfo uiInfo)
        {
            if (_itemTypeFguiInfoMap.TryGetValue(uiInfo.UiType, out var demo))
            {
                // var item = FairyGUI.UIPackage.CreateObject(demo.PackageName, demo.ViewName).asCom;

                var go = new GameObject(uiInfo.UiType.ToString())
                {
                    transform =
                    {
                        parent = this.transform
                    }
                };

                var panel = go.AddComponent<UIPanel>();
                panel.packageName = demo.PackageName;
                panel.componentName = demo.ViewName;
                panel.container.renderMode = RenderMode.WorldSpace;
                panel.CreateUI();

                go.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                go.AddComponent<SortingGroup>().sortingLayerName = "WorldUILayer";
                WorldSpaceUIItemBase uiItem = null;

                if (_scriptMap.TryGetValue(uiInfo.UiType, out Type baseClass))
                {
                    uiItem = go.AddComponent(baseClass) as WorldSpaceUIItemBase;
                    uiItem.MyUIPanel = panel;
                }
                else
                {
                    Debug.LogError(
                        $"添加 WorldSpaceUIItem 类型{uiInfo.UiType} 时,"
                        + "未找到其对应的控制脚本,它必须继承于WorldSpaceUIItemBase,且需在WorldSpaceUI类的InitUIItemDemo函数内添加");
                }

                uiItem.Init(uiInfo.Follow, uiInfo.Args);
                return uiItem;
            }
            else
            {
                Debug.LogError($"没有找到类型为 {uiInfo.UiType} 的FguiInfo," +
                               "它需要在需在WorldSpaceUI类的InitUIItemDemo函数内添加");
            }

            return default;
        }

        /// <summary>
        /// 刷新显示ui
        /// </summary>
        /// <param name="followTr"></param>
        /// <param name="uiType"></param>
        /// <param name="args">如果需要刷新的是可堆叠的,那么在args中的第一个必须是stackId(int)</param>
        public void RefreshWorldSpaceUI(Transform followTr, Enum uiType, object[] args)
        {
            if (!BeNoManagerUI(uiType))
            {
                if (_worldSpaceUIMap.TryGetValue(followTr, out var uiInfos))
                {
                    int index = -1;
                    if (!BeStackableUI(uiType))
                    {
                        //非堆叠的ui直接刷新就好,
                        index = uiInfos.FindIndex(x => x.UiType.Equals(uiType));
                    }
                    else
                    {
                        //可堆叠的ui,就必须要传入stackId才能正确的找到
                        if (args.Length <= 0 || args[0] is not string)
                        {
                            Debug.LogError($"WorldSpaceUI系统中的 {uiType} 类型 销毁 的时候发现参数(args)的第一个参数不是字符串类型,它应该是堆叠id");
                        }

                        string stackId = args[0] as string;
                        index = uiInfos.FindIndex(x => x.UiType.Equals(uiType) && x.StackId.Equals(stackId));
                    }

                    if (index >= 0) //发现这个类型有ui,重置参数,然后刷新
                    {
                        var info = uiInfos[index];
                        info.Args = args;
                        info.WorldSpaceUIItem.Refresh(followTr, args);
                    }
                }
            }
        }

        /// <summary>
        /// 检查是否存在某个ui
        /// </summary>
        /// <param name="followTr"></param>
        /// <param name="uiType"></param>
        /// <param name="subId"></param>
        /// <param name="cb"></param>
        public bool CheckExistWorldSpaceUI(Transform followTr, Enum uiType, string subId)
        {
            if (!BeNoManagerUI(uiType))
            {
                if (_worldSpaceUIMap.TryGetValue(followTr, out var uiInfos))
                {
                    int index = -1;
                    if (!BeStackableUI(uiType))
                    {
                        index = uiInfos.FindIndex(x => x.UiType.Equals(uiType));
                        return index >= 0;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(subId))
                        {
                            Debug.LogError($"WorldSpaceUI系统中的 {uiType} 类型 查询 的时候发现堆叠id为空,这个类型是堆叠类型,必须传入有效的堆叠id才能查到");

                            return false;
                        }

                        //可堆叠的ui,就必须要传入stackId才能正确的找到
                        index = uiInfos.FindIndex(x => x.UiType.Equals(uiType) && x.StackId.Equals(subId));
                        if (index >= 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        private void OnDestroy()
        {
            DestroyAllUIItem();
        }

        /// <summary>
        /// 销毁所有存在的uiItem
        /// </summary>
        public void DestroyAllUIItem()
        {
            foreach ((var key, var list) in _worldSpaceUIMap)
            {
                if (list != null)
                {
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        var ui = list[i];
                        if (ui != null && ui.Follow != null)
                        {
                            Object.Destroy(ui.Follow.gameObject);
                        }
                    }
                }
            }

            _worldSpaceUIMap.Clear();
        }
    }

    /// <summary>
    /// 世界空间ui信息
    /// </summary>
    public class WorldSpaceUIInfo
    {
        public Transform Follow;
        public Enum UiType;
        public object[] Args;
        public WorldSpaceUIItemBase WorldSpaceUIItem;

        /// <summary>
        /// 可堆叠的id,要求参数(Args)的第一个必须是字符串类型的id
        /// </summary>
        public string StackId;
    }

    public class WorldSpaceUIFguiInfo
    {
        public string PackageName;
        public string ViewName;
    }

    public abstract class WorldSpaceUIItemBase : MonoBehaviour
    {
        public abstract Enum UIItemType { get; }
        public abstract void Init(Transform follow, object[] args);

        [NonSerialized] public UIPanel MyUIPanel;
        private Camera _uiCamera;

        /// <summary>
        /// 跟随的偏移
        /// </summary>
        public abstract Vector3 FollowOffset { get; }

        protected Camera UiCamera
        {
            get
            {
                if (_uiCamera == null)
                    _uiCamera = Camera.main;
                return _uiCamera;
            }
            set => _uiCamera = value;
        }

        protected void FollowTr(Transform tr)
        {
            // // 将世界坐标转换为屏幕坐标
            // Vector3 screenPosition = UiCamera.WorldToScreenPoint(tr.position);
            //
            // // 将屏幕坐标转换为UI坐标
            // Vector2 uiPosition;
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //     this.transform.parent as RectTransform,
            //     screenPosition,
            //     null,
            //     out uiPosition
            // );

            // 设置UI元素的位置
            this.transform.position = tr.position + FollowOffset;
        }

        public virtual void Destroy()
        {
            Destroy(this.gameObject);
        }

        //更新数据时调用
        public virtual void Refresh(Transform follow, object[] args)
        {
            Init(follow, args);
        }
    }

    public static class FguiWorldSpaceUIMgrExtension
    {
        private static FguiWorldSpaceUIMgr _fguiWorldSpaceUIMgr;

        public static FguiWorldSpaceUIMgr FguiWorldSpaceUIMgr(this FW fw)
        {
            if (_fguiWorldSpaceUIMgr == null)
            {
                _fguiWorldSpaceUIMgr = new GameObject("FguiWorldSpaceUIMgr").AddComponent<FguiWorldSpaceUIMgr>();
                UnityEngine.Object.DontDestroyOnLoad(_fguiWorldSpaceUIMgr.gameObject);
            }

            return _fguiWorldSpaceUIMgr;
        }
    }
}