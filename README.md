# HMFW
hmok的unity游戏框架

添加游戏入口脚本/Mono/GameStart.cs,将其挂在第一个场景的摄像机上即可
  
现在支持:  
1:全局事件中心,使用HMFW.EventManager 访问,需要定义的事件枚举请增加到Custom/EventDefine.cs中  
2:自消失的提示信息(微信那样的),使用HMFW.TipsManager访问,如需修改样式,请修改Resources/TipsPrefeb预制体,使用时,不需要手动挂载到场景中.  
3:对话框UI,使用HMFW.DiaglogManager访问,有2种样式,并且坑定义大小,标题,且能增加回调得知玩家的选择,如需修改样式,请修改Resources/DialogManagerPrefeb;
4:添加UI管理器,使用HMFW.UIManager访问,可以传递参数数据(请在场景中创建UIRoot空物体,并将本场景中需要用到的UI放入其下,不需要在UIRoot中手动添加本类)
5:添加游戏数据管理器,使用HMFW.GameDataManager访问,可以持久化游戏数据,最多可以保存100个存档数据(暂时保存单机数据),所有需要保存的数据,请在Custom/GameSaveData.cs中添加
6:添加游戏资源管理器,使用HMFW.AssetManager访问,可以根据名字获取到GameObject(暂时使用 Resources.load方式)
7:添加游戏状态管理器,使用HMFW.GameStateManager 访问,可以实现切换主游戏状态(一个状态对应一个场景),新建游戏状态时请继承GameSateBase,不需要注册!
