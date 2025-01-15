# HMFW Unity游戏开发框架
***

## 框架简介

>* 为小型开发团队准备的高性能高扩展的多人协作游戏开发框架
>* 整体游戏采用有限状态机控制游戏的各阶段,逻辑清晰,各阶段互相解耦
>* 各阶段使用不同场景(scene)开发,满足多小组同时开发,依赖最小化
>* 各功能模块接口统一,但又可方便的自行扩展接口

***

### 框架的使用

>* 本包依赖`https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`
>* 本包依赖`https://github.com/hmok38/hmaddressable.git?path=Assets/HMAddressable`
>* 请先到UnityPackageManager中安装以上2个依赖
>* 然后在UnityPackageManager中安装本包:`https://github.com/hmok38/HMFW.git?path=Assets/HMFW`
>* 命名空间HMFW,模块统一访问接口 "`FW.`" 和自定义模块 "`FW.CustomAPI.`"
>* 按照规范增加新模块或者扩展旧模块可以非常方便的让业务层访问并统一接口,当需要更换插件或者修改逻辑时,业务层逻辑基本不用修改,本框架也无需更新和修改
>* 模块的扩展和增加新模块的方式请参考示例中的 **`Assets/HMFW/Samples/SampleURP/FWExtend.cs`** 类

***

### 现有功能模块
>1. 游戏逻辑控制器(GameFsmMgr),访问接口 `FW.GameFsmMgr`
>2. 全局框架数据类（FWData）,访问接口 `FW.FWData`
>3. 资源管理器(AssetsMgr),访问接口 `FW.AssetsMgr`
>4. UI管理器(UIMgr),访问接口 `FW.UIMgr` _**支持对FairyGUI的扩展,请安装示例:`FairyGUIExtention`**_
>5. 全局事件管理器(GEventMgr),访问接口 `FW.GEventMgr`
>6. 全局数据管理器(GDataMgr),访问接口 `FW.GDataMgr`
>7. 音频管理器(AudioMgr),访问接口 `FW.AudioMgr`
>8. 音频管理器(ObjectPoolMgr),访问接口 `FW.ObjectPoolMgr`
>9. 返回键队列管理器(BackBtnQueueMgr),访问接口 `FW.BackBtnQueueMgr` _**按返回键按队列关闭UI或者调用Func,UI系统自动支持**_

### 框架扩展方法
_模块的扩展和增加新模块的方式请参考示例中的 **`Assets/HMFW/Samples/SampleURP/FWExtend.cs`** 类_
#### 框架所有模块分为2种:
> ##### 1.框架自带模块
> 我们可以 **彻底替换** ,或者在其上 **扩展函数**
>1. 彻底替换:在使用模块前,直接定义相应的类(需要继承基类) `FW.UIMgr = new UIMgrFairyGUI();`其中`UIMgrFairyGUI`继承于`UIMgr`
>2. 扩展接口:可以在模块上扩展一些需要的接口:
 ```csharp
public static class SampleUIMgrExtend
{
    //用这样的方式就通过 FW.UIMgr.UIMgrExtendMethod 访问了
    public static void UIMgrExtendMethod(this UIMgrBase uiMgrBase)
    {
        if (uiMgrBase.GetType() == typeof(UIMgr))
        {
            var uiMgr = uiMgrBase as UIMgr;
            //----扩展逻辑
        }
    }
}
```

> ##### 2.自定义模块
> 我们可以自由的添加项目需要的模块,并保持访问一致,这一功能对特色项目的特殊需求会非常有帮助
> 例如:我们想要扩展一个叫做Timer的模块,那么首先建立这个Timer类的主要逻辑,然后使用静态扩展即可,
> 访问的方式非常简单,通过`FW.CustomAPI.Timer()`即可访问
```csharp
public static class FWExtend
{
    public static Timer Timer(this FW fw)
    {
        return Timer.Instance;
    }
}
```

 ***
### UI管理器(UIMgr)模块使用说明
UI管理器(UIMgr),访问接口 `FW.UIMgr`  
_**框架ui已经支持对`FairyGUI`的扩展,请在安装完`FairyGUI SDK`后到`UnityPackageManager`中`HMFW`包中安装示例:`FairyGUIExtention`**_
>* 优先级(priority)的定义
>1. 框架对所有管理的ui进行了优先级(priority)的设定,且按照priority每100进行分组,如(0-99)为一个ui组,(100-199)又是一个ui组
>2. 在打开ui时可传入priority(默认100),即可对ui的展示层级进行排序.

>* UI组(UIGroup)的作用
>1. 快速关闭全组ui或者组内部分ui.
>2. 对组内ui数量进行限制,当超过限制数量时,新打开的ui会等待打开,并在同组其他ui被关闭时自动打开
>3. 对组内已经显示的ui进行遮蔽操作,隐藏/遮蔽同组的其他ui
>4. 需要设置组限制,请调用`FW.UIMgr.GetGroupSetting(200).BusyLimit = 1 `,默认为0不限制

>* 打开/关闭类型(OpenType/UICloseType)的作用
>1. 代表打开ui时的方式,可以排队打开/立刻打开/覆盖打开等,具体请见枚举`UIOpenType`
>2. 代表关闭ui时的方式,在关闭一组或者全部ui的时候,可以选择不同的关闭方式,可以关闭全部/等待中的/显示中的,具体请见枚举`UICloseType`

>* 脚本定义
>* 脚本会自动附加到ui预制体的根节点上,预制体根节点可以是画布也可以不是画布
>1. 每个ui必须有一个继承于UIBase的类,其继承于MonoBehaviour,可以使用所有unity自带的生命周期,这就是ui的脚本
>2. 继承UIBase后,还必须实现特性`UGUIResAttribute`(使用UGUI时)其继承于`UIAttribute`,需要输入其预制体的加载地址(aa包)及别名
>3. 对于`UGUIResAttribute`特性中资源地址中某些需要运行时确定的字段,我们可以使用自定义的标签来代替,并在运行时设置其对应的值,ui系统在加载时会自动替代,例如多国语言的预制体在不同的目录下时,非常有用
>4. 例如 `FW.UIMgr.SetUrlReplace("[L]","English");`即可将`UGUIResAttribute`特性中资源地址中带有`[L]`的地方替换为`English`

>* 返回键队列管理器(BackBtnQueueMgr)自动支持
>* 重写UIBase的beBackBtnQueueUI为True即可将UI添加到返回键队列管理器(BackBtnQueueMgr),当返回键被触发,会按照顺序关闭设定的ui.此字段默认为false

### 对FairyGUI的支持
>1. 针对项目需求,我们添加了对FairyGUI的管理支持,请先在项目中添加`FairyGUI SDK`后到`UnityPackageManager`中`HMFW`包中安装示例:`FairyGUIExtention`**_
>2. Fgui的所有启用/关闭/等待/覆盖功能都已如ugui使用.
>3. FGUI的脚本需要继承`FairyGUIBase`,其也继承`UIBase`,只是扩展了一个`MyGObject`字段,用来访问脚本所在的`GObject`(FairyGUI)对象
>4. Fgui的脚本需要实现`FGUIResUrlAttribute` 特性,其跟`UGUIResAttribute`一样,也是继承于`UIAttribute`,但需要填写的内容有所不同

### 对Unity Ecs的扩展支持

>针对项目需求,我们添加了对Unity Ecs的扩展和支持,请先在项目的`UnityPackageManager`中添加`com.unity.entities.graphics`后,再选择`HMFW`包中安装示例:`HMFWEcsExtension`**_
>1. `AutoDestroySystem` **自动销毁系统**  
    某些特效预制体需要计时销毁的话可以使用它,向一个`Entity`添加`AutoDestroyComponent`组件即可设定自动销毁`Entity`和其`子Entity`.
    另外预制体还可以添加`AutoDestroyAuthoring`脚本自动添加销毁组件
>2. `EcsGlobalEventSystem` **Ecs中的全局事件系统**  
   某些时候需要从ecs中发出全局事件,那么就可以使用本系统,会在下一帧统一调用HMFW的全局事件系统发送事件.便捷使用方式:EcsGlobalEventComponent实例.SendEvent()或者SentEventInJob(),因为是最终是调用ecb来发送,所以需要传入ecb使用
>3. `PrefabResSystem` **预制体资源系统**  和 `PrefabResAuthoring` **预制体烘焙组件**  
   项目或者子场景中预制体资源的管理,请采用预制体管理系统进行:  
   3.1 请先配置`PrefabsResConfigSO`的配置表,可通过菜单栏`Assets->Create->HMFW->Ecs->PrefabsResConfig`来创建,或者右键点击资源目录后选择`Create/HMFW/Ecs/PrefabsResConfig`创建.  
   3.2 将预制体拖入配置表,并赋值一个id.  
   3.3 在Ecs中即可根据这个id从 `SystemAPI.GetSingleton<PrefabResSingleton>().PrefabMap` 组件中的HashMap中拿到这个预制体的Entity  
   3.4 在游戏场景的某个`SubScene` 添加一个空物体,然后向其添加`PrefabResAuthoring`脚本,并将刚刚配置好的`PrefabsResConfigSO`拖入其中
   注意:此预制体id是`World`唯一,如果2个不同的游戏场景资源id相同没有问题,否则请自行管理好此id
>4. `PrefabInstantiateSystem` **预制体实例化系统**  
   需要实例化某个预制体的时候,请创建一个 `Entity` 并向其添加 `PrefabInstantiateComponent` 组件,设置好坐标,预制体id,即可在结束帧时创建新的预制体实例
>5. `EntityDestroyExtension` **销毁Entity的扩展方法**  
   扩展了两个将子entity一起销毁的方法
>6.  `GameStateSceneTestWithSubScene` **测试场景的ecs专用脚本**  
       场景中有subScene的时候,如果定义了 `UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP` 编译符,不自动创建世界的话,会出现subScene不自动加载的情况,可以将场景中`GameStateSceneTest`脚本实例替换为`GameStateSceneTestWithSubScene`
