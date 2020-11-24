using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HMFW
{
    /// <summary>
    /// 对话框UI管理器
    /// </summary>
    public class DialogManager : MonoSingleton<DialogManager>
    {

        private GameObject prefebObj;
        private void Awake()
        {
            this.LoadPrefeb();
        }
        protected void LoadPrefeb()
        {
            Object tipsPrefeb = Resources.Load("DialogManagerPrefeb");
            prefebObj = GameObject.Instantiate(tipsPrefeb, transform) as GameObject;
            prefebObj.name = "DialogManagerPrefeb";
        }
    }
}