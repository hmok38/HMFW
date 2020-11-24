# HMFW
hmok的unity游戏框架  
现在支持:  
1:全局事件中心,使用HMFW.EventManager 访问,需要定义的事件枚举请增加到Custom/EventDefine.cs中  
2:自消失的提示信息(微信那样的),使用HMFW.TipsManager访问,如需修改样式,请修改Resources/TipsPrefeb预制体,使用时,不需要手动挂载到场景中.  
3:对话框UI,使用HMFW.DiaglogManager访问,有2种样式,并且坑定义大小,标题,且能增加回调得知玩家的选择;
