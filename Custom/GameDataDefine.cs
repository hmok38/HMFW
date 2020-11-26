using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏数据相关定义
/// </summary>
public static class GameDataDefine 
{

   public static Dictionary<GameDataType, Type>  dataTypeMap = new Dictionary<GameDataType, Type>();
    /// <summary>
    /// 注册数据类型-只能注册类,如果是值类型,请用类封装
    /// </summary>
    public static void RegGameDataType()
    {
        dataTypeMap.Add(GameDataType.GameSystemData, typeof(GameSystemData));
        dataTypeMap.Add(GameDataType.AlreadyUseNames, typeof(Dictionary<int, List<int>>));
        dataTypeMap.Add(GameDataType.PlayerActorData, typeof(List<ActorData>));

    }
}



/// <summary>
/// 游戏数据类型
/// </summary>
public enum GameDataType
{
    /// <summary>
    /// 游戏系统数据
    /// </summary>
    GameSystemData,
    /// <summary>
    /// 已经使用的名字列表
    /// </summary>
    AlreadyUseNames,
    /// <summary>
    /// 玩家方角色(部队)信息(为ActorList)
    /// </summary>
    PlayerActorData,


}
