
/// <summary>
/// 框架使用者自定义的事件类型(枚举请保留,内容可编辑)
/// </summary>
public  enum EventName 
{
    E_GameStart,
    E_GameOver,
    /// <summary>
    /// ui上的角色模型改变
    /// </summary>
    E_OnUIActorModelNameChange,
    /// <summary>
    /// 角色即将进入舞台
    /// </summary>
    E_ActorWillEnterStage,
}