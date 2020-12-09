using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace HMFW
{



    /// <summary>
    /// 需要保存的数据,请先在Custom/GameSaveData 中添加进去,如果可以保存100个存档数据,S/L大法,因为比较耗时,建议在游戏初始化阶段调用InitData
    /// </summary>
    public class GameDataManager : Singleton<GameDataManager>
    {
        /// <summary>
        /// 已经保存到了硬盘的存档ID列表
        /// </summary>
        List<int> gameSaveList = new List<int>();
        int lastUseSaveId = -1;
        /// <summary>
        /// 保存过的存档
        /// </summary>
        Dictionary<int, GameSaveData> savedDataMap = new Dictionary<int, GameSaveData>();
        /// <summary>
        /// 当前使用的存档ID
        /// </summary>
        public GameSaveData currentSaveData { get; private set; }
        

        /// <summary>
        /// 初始化游戏数据,加载硬盘游戏数据
        /// </summary>
        public void InitData()
        {
            LoadSaveListFromIO();
            LoadAllFromIO();
        }

        /// <summary>
        /// 从硬盘中获取游戏存档列表(key列表)
        /// </summary>
        private void LoadSaveListFromIO()
        {
            if (PlayerPrefs.HasKey("GameSaveList"))
            {
                var str = PlayerPrefs.GetString("GameSaveList", "");
                gameSaveList = JsonConvert.DeserializeObject<List<int>>(str);
            }
            if (PlayerPrefs.HasKey("LastUseSaveId"))
            {
                var str = PlayerPrefs.GetInt("LastUseSaveId", 0);
                lastUseSaveId = str;
            }
        }
        /// <summary>
        /// 从硬盘读取数据
        /// </summary>
        private void LoadAllFromIO()
        {
            savedDataMap.Clear();

            for (int i = 0; i < gameSaveList.Count; i++)
            {
                string id = "GameSaveID" + gameSaveList[i];
                if (PlayerPrefs.HasKey(id))
                {
                  var str=  PlayerPrefs.GetString(id, "");
                  var save=  JsonConvert.DeserializeObject<GameSaveData>(str);
                    savedDataMap[save.SaveID] = save;
                }
                else
                {
                    HMFW.TipManager.Instance.ShowTips(string.Format("不好啦,{0}号存档丢失了!", id));
                }
            }
            //获取之前使用的存档
            if(savedDataMap.ContainsKey( lastUseSaveId))
            {
                currentSaveData = savedDataMap[lastUseSaveId].MyClone();
            }

        }
        private void SaveLastUseSaveIdToIo()
        {
            PlayerPrefs.SetInt("LastUseSaveId", this.lastUseSaveId);
        }
        /// <summary>
        /// 创建一个新的存档,并设置为当前使用的存档
        /// </summary>
        public GameSaveData CreatNewSave()
        {
           
            this.currentSaveData = new GameSaveData();
            this.currentSaveData.SaveID = GetNewSaveID();

            return this.currentSaveData;
        }
        /// <summary>
        /// 检查当前使用的存档是否被保存,没有保存的话恢复上一次保存的存档
        /// </summary>
        public void RestoreToSavedGameSaveData()
        {
            if(this.currentSaveData==null|| !gameSaveList.Contains(this.currentSaveData.SaveID))
            {
                if (this.lastUseSaveId >= 0)
                {
                    this.currentSaveData = savedDataMap[this.lastUseSaveId].MyClone();
                }
                else
                {
                    this.currentSaveData = null;
                }
            }
        }
        /// <summary>
        /// 获取全部的存档数据
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, GameSaveData> GetAllSaveData()
        {
            return savedDataMap;
        }
        /// <summary>
        /// 使用一个旧的存档
        /// </summary>
        public void UseOldSave(int saveID)
        {
            this.currentSaveData = this.savedDataMap[saveID].MyClone();
            this.lastUseSaveId = saveID;
            SaveLastUseSaveIdToIo();
        }

        /// <summary>
        /// 保存数据到一个新的存档位置
        /// </summary>
        public void SaveCurrentDataToNew()
        {
            var id = GetNewSaveID();

            SaveCurrentDataToID(id);
        }
        /// <summary>
        /// 将数据保存到某个位置
        /// </summary>
        public void SaveCurrentDataToID(int saveId)
        {
            if (currentSaveData != null)
            {
                currentSaveData.SaveID = saveId;
                if (!gameSaveList.Contains(saveId))
                {
                    gameSaveList.Add(currentSaveData.SaveID);
                    SaveGameSaveList();
                }
              
                SaveData(currentSaveData);
                PlayerPrefs.Save();
                GameSaveData newDate = currentSaveData.MyClone();
                if (savedDataMap.ContainsKey(saveId))
                {
                    savedDataMap[currentSaveData.SaveID] = newDate;
                }
                else
                {
                    savedDataMap.Add(currentSaveData.SaveID, newDate);
                }
               

            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="gameSaveData"></param>
        private void SaveData(GameSaveData gameSaveData)
        {
            string id = "GameSaveID" + gameSaveData.SaveID;
            gameSaveData.SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString(id, JsonConvert.SerializeObject(gameSaveData));
            lastUseSaveId = gameSaveData.SaveID;
            SaveLastUseSaveIdToIo();
        }
        /// <summary>
        /// 获取新的存档ID
        /// </summary>
        /// <returns></returns>
       private int GetNewSaveID()
        {
            for (int i = 0; i < 100; i++)
            {
                if (!gameSaveList.Contains(i))
                {
                    return i;
                }
            }
            TipManager.Instance.ShowTips("存档超过100个啦!");
            return 100;
        }
        /// <summary>
        /// 保存游戏列表
        /// </summary>
        private void SaveGameSaveList()
        {
            PlayerPrefs.SetString("GameSaveList", JsonConvert.SerializeObject(gameSaveList));
        }




    }




}