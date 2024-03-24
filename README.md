#HMFW Unity游戏开发框架
***

###框架简介

>* 为小型开发团队准备的高性能高扩展的多人协作游戏开发框架
>* 整体游戏采用有限状态机控制游戏的各阶段,逻辑清晰,各阶段互相解耦
>* 各阶段使用不同场景(scene)开发,满足多小组同时开发,依赖最小化
>* 各功能模块接口统一,但又可方便的自行扩展接口

***
###框架前置依赖
请按顺序添加
* 异步编程插件:  
   >  https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.3.3
* 基于UnityAddressables的资源管理系统插件:  
   >  https://github.com/hmok38/hmaddressable.git?path=Assets/HMAddressable
* 自动化配置表插件:  
   > https://github.com/hmok38/HMExcelConfig.git?path=Assets/HMExcelConfig  
* 安装路径:  
   > https://github.com/hmok38/HMFW.git?path=Assets/HMFW

###框架的使用

>* 命名空间HMFW,模块统一访问接口 FW.API
>* 按照规范增加新模块或者扩展旧模块可以非常方便的让业务层访问并统一接口,当需要更换插件或者修改逻辑时,业务层逻辑基本不用修改,本框架也无需更新和修改
>* 功能模块的扩展和增加新模块的方式请参考示例中的FWExtend类

***

###功能模块
>1. 游戏逻辑控制器(GameFsmManager),访问接口 FW.API.GameFsmManager,它是一个单例