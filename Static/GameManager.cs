using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HMFW
{

    /// <summary>
    /// 游戏逻辑管理器,
    /// </summary>
    public class GameManager : MonoSingleton<GameManager>
    {
        /// <summary>
        /// 游戏状态机
        /// </summary>
        public GameStateFsm GameStateFsm;

        /// <summary>
        /// 游戏管理器初始化
        /// </summary>
        public void Awake()
        {
            GameStateFsm = new GameStateFsm(this.transform);
            GameFsmDefine.RegGameFsm();
        }

        public void Update()
        {
            GameStateFsm.OnUpdate();
        }
    }
}