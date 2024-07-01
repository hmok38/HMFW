using System;
using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HMFW.SampleURP.GameState
{
    public class GameStateSampleLoading : HMFW.Core.GameStateBase
    {
        public override async UniTask EnterState(params object[] args)
        {
            Debug.Log($"开始进入{this.GetType()}");
            /*
             * 这里也可以使用一些耗时操作.例如加载某个很大的场景,最好是异步
             * 游戏的整体逻辑不会受到影响,等完成后再关掉之前开启的遮蔽用的UI
             */

            await FW.AssetsMgr.LoadSceneAsync("Assets/HMFW/Samples/SampleURP/Scenes/SampleMainScene.unity");

            await UniTask.NextFrame(); //等待一帧
            await UniTask.NextFrame(); //等待一帧

            // var gc = new GComponent();
            // gc.gameObjectName = "测试组1";
            // gc.name = "测试组1";
            // GRoot.inst.AddChild(gc);
            // var gc2 = new GComponent();
            // gc2.gameObjectName = "测试组2";
            // gc2.name = "测试组2";
            // GRoot.inst.AddChild(gc2);
            //
            // //加载包
            // await LoadPackage("Assets/Bundles/Fgui/Common/UICommon");
            // await LoadPackage("Assets/Bundles/Fgui/MainScene/UIHome");
            // //异步创建对象
            // bool beHomeUI = false;
            // UIPackage.CreateObjectAsync("UIHome", "Home", homeUI =>
            // {
            //     beHomeUI = true;
            //     gc2.AddChild(homeUI);
            // });
            //
            // UIPackage.CreateObjectAsync("UIHome", "FguiSample", homeUI =>
            // {
            //     beHomeUI = true;
            //     gc.AddChild(homeUI);
            // });
            //
            // await UniTask.WaitUntil(() => beHomeUI);

            // UIPackage.branch = "en";
            // var lag = await FW.AssetsMgr.LoadAsync<TextAsset>("Assets/Bundles/Fgui/Translation/English.xml");
            // string fileContent = lag.text; //自行载入语言文件，这里假设已载入到此变量
            // FairyGUI.Utils.XML xml = new FairyGUI.Utils.XML(fileContent);
            // UIPackage.SetStringsSource(xml);

            await FW.UIMgr.OpenUI("FguiSample", 300,UIOpenType.Wait);
            
            await UniTask.Delay(2000);
            await FW.UIMgr.OpenUI("FguiSample", 200,UIOpenType.Wait);
            await FW.UIMgr.OpenUI("Home", 200,UIOpenType.Wait);
            await UniTask.Delay(4000);
            await FW.UIMgr.CloseUI("UISampleLoading"); //慢慢关闭ui

            //await FW.UIMgr.CloseAllUI(new[] { "FguiSampleUI" });

            await UniTask.Delay(4000);

            //await FW.UIMgr.CloseUI("FguiSampleUI");
            await UniTask.Delay(1000);

            //await FW.UIMgr.OpenUI("FguiSampleUI", "哈哈");
            Debug.Log($"进入完成{this.GetType()}");
        }

        public static async UniTask LoadPackage(string packagePath)
        {
            Debug.Log($"{packagePath}");
            var descDataAssetUIHome =
                await FW.AssetsMgr.LoadAsync<TextAsset>($"{packagePath}_fui.bytes");
            var descData = descDataAssetUIHome.bytes;

            UIPackage.AddPackage(descData, packagePath, OnLoadResourceAsync);
        }

        public static async void OnLoadResourceAsync(string name, string extension, Type type, PackageItem item)
        {
            Debug.Log(name);
            Debug.Log(extension);
            Debug.Log(type.FullName);
            Debug.Log(item.file);

            // string paths = LocalFGUIPath + name + extension;
            // instance.otherRes.Add(paths);
            var obj = await FW.AssetsMgr.LoadAsync<Object>(item.file);
            item.owner.SetItemAsset(item, obj, DestroyMethod.None);
            // instance.fGuiLoadedOtherRes.Add(name);
        }

        public override async UniTask LeaveState(params object[] args)
        {
            Debug.Log($"开始离开{this.GetType()}");
            await UniTask.Delay(1000);

            Debug.Log($"离开完成{this.GetType()}");
        }

        public override void OnUpdate()
        {
        }
    }
}