# HMFW框架对Ecs的扩展包
### 本扩展包包含部分常用的ecs组件和功能

1. `AutoDestroySystem` **自动销毁系统**  
    某些特效预制体需要计时销毁的话可以使用它,向一个`Entity`添加`AutoDestroyComponent`组件即可设定自动销毁`Entity`和其`子Entity`.
   另外预制体还可以添加`AutoDestroyAuthoring`脚本自动添加销毁组件
2. `EcsGlobalEventSystem` **Ecs中的全局事件系统**  
    某些时候需要从ecs中发出全局事件,那么就可以使用本系统,会在下一帧统一调用HMFW的全局事件系统发送事件.便捷使用方式:EcsGlobalEventComponent实例.SendEvent()或者SentEventInJob(),因为是最终是调用ecb来发送,所以需要传入ecb使用
3. `PrefabResSystem` **预制体资源系统**  和 `PrefabResAuthoring` **预制体烘焙组件**  
    项目或者子场景中预制体资源的管理,请采用预制体管理系统进行:  
    3.1 请先配置`PrefabsResConfigSO`的配置表,可通过菜单栏`Assets->Create->HMFW->Ecs->PrefabsResConfig`来创建,或者右键点击资源目录后选择`Create/HMFW/Ecs/PrefabsResConfig`创建.  
    3.2 将预制体拖入配置表,并赋值一个id.  
    3.3 在Ecs中即可根据这个id从 `SystemAPI.GetSingleton<PrefabResSingleton>().PrefabMap` 组件中的HashMap中拿到这个预制体的Entity  
    3.4 在游戏场景的某个`SubScene` 添加一个空物体,然后向其添加`PrefabResAuthoring`脚本,并将刚刚配置好的`PrefabsResConfigSO`拖入其中
    注意:此预制体id是`World`唯一,如果2个不同的游戏场景资源id相同没有问题,否则请自行管理好此id
4. `PrefabInstantiateSystem` **预制体实例化系统**  
    需要实例化某个预制体的时候,请创建一个 `Entity` 并向其添加 `PrefabInstantiateComponent` 组件,设置好坐标,预制体id,即可在结束帧时创建新的预制体实例
5. `EntityDestroyExtension` **销毁Entity的扩展方法**  
    扩展了两个将子entity一起销毁的方法