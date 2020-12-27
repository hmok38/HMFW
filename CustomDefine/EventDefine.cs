
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
    /// 角色直接放置上舞台
    /// </summary>
    E_ReadyState_ActorDirectPlaceOnStage,
    /// <summary>
    /// 角色离开舞台
    /// </summary>
    E_ReadyState_ActorLeaveStage,
    /// <summary>
    /// 角色被从舞台提起(移动位置,手持状态)
    /// </summary>
    E_ReadyState_ActorBeTake,
    /// <summary>
    /// 玩家的上场角色数据需要刷新
    /// </summary>
    E_PlayerStageActorDataNeedUpdate,

}