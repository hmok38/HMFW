using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HMFW;
/// <summary>
/// 游戏状态自定义
/// </summary>
public static class GameFsmDefine 
{
    
    /// <summary>
    /// 向游戏管理器的状态机注册各状态
    /// </summary>
   public static void RegGameFsm(GameStateManager gameStateManager)
    {
        gameStateManager.RegState<InitState>();
        gameStateManager.RegState<MainMenuState>();
        gameStateManager.RegState<ReadyState>();
    }
}
