using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源管理器,统一的资源获取位置
/// </summary>
public  class AssetManager : MonoSingleton<AssetManager>
{

    /// <summary>
    /// 根据名字获取游戏物体
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject GetGameObjectByName(string name)
    {
        var obj = Resources.Load(name);
        
        return GameObject.Instantiate(obj) as GameObject;

    }
    /// <summary>
    /// 获取资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T GetObjByName<T>(string name) where T:Object
    {
        var obj = Resources.Load(name,typeof(T));
        return obj as T;
    }
}
