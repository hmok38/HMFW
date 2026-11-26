# HMFW框架对FairyGUI的扩展包
### 本扩展包可以让HMFW框架支持和管理FairyGUI制作UI

`FairyGuiSampleInitGameState.EnterState` 创建 `UIMgrFairyGUI` 后，调用 `FW.UIMgr.SetUITypeListJson(json)` 注册加载页 `FguiSampleLoadingUI` 和两个 FairyGUI 页面 `FguiSample`、`Home`。同时导入 SampleURP 时，示例也会加入它的两个 UI 类。

将示例接入已有项目时，可以先传入空字符串，再把编辑器错误末尾的完整 JSON 作为业务列表传入。注册成功后，示例才会打开加载页。
