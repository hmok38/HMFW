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
    public int CurrentActorId;
    public List<ActorData> PlayerTeamActorDatas=new List<ActorData>();
    public Dictionary<int, List<int>> alreadyUseNames=new Dictionary<int, List<int>>();


   public GameSaveData MyClone()
    {
        return (GameSaveData)Clone();
    }

   public object Clone()
    {
        return (GameSaveData)MemberwiseClone();
    }
}
