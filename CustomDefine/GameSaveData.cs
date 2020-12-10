using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏存档数据-游戏中需要添加的数据请添加在此
/// </summary>
public class GameSaveData : ICloneable
{
    public int SaveID;
    public string SaveTime;
    /// <summary>
    /// 生成角色用的ID
    /// </summary>
    public int CurrentActorId;
    /// <summary>
    /// 玩家部队角色数据
    /// </summary>
    public List<ActorData> PlayerTeamActorDatas = new List<ActorData>();
    /// <summary>
    /// 随机姓名系统-用过的玩家数据
    /// </summary>
    public Dictionary<int, List<int>> alreadyUseNames = new Dictionary<int, List<int>>();
    /// <summary>
    /// 玩家的游戏进程
    /// </summary>
    public int GameLevel;
    /// <summary>
    /// 玩家的名字
    /// </summary>
    public string PlayerName;
    /// <summary>
    /// 玩家军队名称
    /// </summary>
    public string PlayerArmyName;

    /// <summary>
    /// 玩家已经上场的角色ID列表及队列相对位置
    /// </summary>
    public Dictionary<int, Vector2> AlreadyOnStageActorIDAndTeamPos = new Dictionary<int, Vector2>();


    /// <summary>
    /// 克隆(请保留)
    /// </summary>
    /// <returns></returns>
    public GameSaveData MyClone()
    {
        return (GameSaveData)Clone();
    }

    /// <summary>
    /// 克隆(请保留)
    /// </summary>
    /// <returns></returns>
    public object Clone()
    {
        return (GameSaveData)MemberwiseClone();
    }
}
