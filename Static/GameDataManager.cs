using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 需要保存的数据,请先在Custom/GameDataDefine/RegGameDataType()中注册类型和枚举
/// </summary>
public class GameDataManager : Singleton<GameDataManager>
{
    Dictionary<GameDataType, object> dataMap = new Dictionary<GameDataType, object>();
   

    public override void Init()
    {
        base.Init();
        GameDataDefine.RegGameDataType();
        LoadAllFromIO();
    }

   


    /// <summary>
    /// 从硬盘读取数据
    /// </summary>
    public void LoadAllFromIO()
    {
        dataMap.Clear();
       
        foreach (GameDataType item in Enum.GetValues(typeof( GameDataType)))
        {
            Type type = GameDataDefine.dataTypeMap[item];
            if (PlayerPrefs.HasKey(item.ToString()))
            {
                var str = PlayerPrefs.GetString(item.ToString(), "");
               
                dataMap.Add(item, JsonConvert.DeserializeObject(str, type));
            }
            else
            {
                dataMap.Add(item, type.Assembly.CreateInstance(type.FullName));
            }
        

        }
    }

    /// <summary>
    /// 存档所有数据
    /// </summary>
    public void SaveAllData()
    {
        foreach (GameDataType item in Enum.GetValues(typeof(GameDataType)))
        {
            SaveDataHandle(item);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 将某个数据存档
    /// </summary>
    /// <param name="gameDataType"></param>
    /// <returns></returns>
    public bool SaveSingleData(GameDataType gameDataType)
    {
      var suc= SaveDataHandle(gameDataType);
        PlayerPrefs.Save();
        return suc;
    }

   
    /// <summary>
    /// 获取数据,得到的数据要求保持引用关系,不得删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gameDataType"></param>
    /// <returns></returns>
    public T GetGameData<T>(GameDataType gameDataType) where T : class
    {
        if (CheckType<T>(gameDataType))
        {
            return dataMap[gameDataType] as T;
        }
        
        return null;
    }
   

    private bool SaveDataHandle(GameDataType gameDataType)
    {
        object value;
        if(dataMap.TryGetValue(gameDataType,out value))
        {
           string str= JsonConvert.SerializeObject(value);
            PlayerPrefs.SetString(gameDataType.ToString(), str);
            return true;
        }
        return false;
    }

    private bool CheckType<T>(GameDataType gameDataType)
    {
        if (dataMap.ContainsKey(gameDataType))
        {
            var right = typeof(T) == GameDataDefine.dataTypeMap[gameDataType];

            if (!right)
            {
                Debug.LogError("获取数据时,发现" + gameDataType + " 获取的类型=" + GameDataDefine.dataTypeMap[gameDataType].Name + " 跟要求的类型" + typeof(T).Name + " 不符");
            }

            return right;
        }

        Debug.LogError("获取数据时,发现" + gameDataType + " 获取的数据不存在");
        return false;

    }
}




