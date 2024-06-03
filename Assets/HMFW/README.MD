#HMFW Unity游戏开发框架
***

###框架简介

>* 为小型开发团队准备的高性能高扩展的多人协作游戏开发框架
>* 整体游戏采用有限状态机控制游戏的各阶段,逻辑清晰,各阶段互相解耦
>* 各阶段使用不同场景(scene)开发,满足多小组同时开发,依赖最小化
>* 各功能模块接口统一,但又可方便的自行扩展接口

***

###框架的使用

>* 命名空间HMFW,模块统一访问接口 "`FW.`" 和自定义模块 "`FW.CustomAPI.`"
>* 按照规范增加新模块或者扩展旧模块可以非常方便的让业务层访问并统一接口,当需要更换插件或者修改逻辑时,业务层逻辑基本不用修改,本框架也无需更新和修改
>* 模块的扩展和增加新模块的方式请参考示例中的 **`FWExtend`** 类

***

###功能模块
>1. 游戏逻辑控制器(GameFsmMgr),访问接口 `FW.GameFsmMgr`
>2. 全局框架数据类（FWData）,访问接口 `FW.FWData`
>3. 资源管理器(AssetsMgr),访问接口 `FW.AssetsMgr`
>4. UI管理器(UIMgr),访问接口 `FW.UIMgr`
>5. 全局事件管理器(GEventMgr),访问接口 `FW.GEventMgr`
>6. 全局数据管理器(GDataMgr),访问接口 `FW.GDataMgr`
>7. 框架模块:示例的模块(SampleMgrBase),访问接口 `FW.SampleMgrBase`
>8. 测试用的自定义新增示例模块（FWTestMgr），访问接口 `FW.CustomAPI.FWTestMgr()`