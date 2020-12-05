using System.Collections;
using System.Collections.Generic;
using UnityEngine;




/// <summary>
/// 此单例继承于Mono,如果需要挂接预制体(部分UI单例),则请在awake里面加载预制体,并成为子物体
/// 不需要手动挂载
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T> 
{
    /// <summary>
    /// 是否需要成为全局物体
    /// </summary>
    /// <returns></returns>
    public virtual bool IsGolbal() { return true; }
    
    
    private static T _instance;
    /// <summary>
    /// 线程锁
    /// </summary>
    private static readonly object _lock = new object();
    /// <summary>
    /// 程序是否正在退出
    /// </summary>
    protected static bool ApplicationIsQuitting { get; private set; }

    static MonoSingleton()
    {
        ApplicationIsQuitting = false;
       
    }

    public static T Instance
    {
        get
        {
            if (ApplicationIsQuitting)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.LogWarning("[Singleton] " + typeof(T) +
                                            " already destroyed on application quit." +
                                            " Won't create again - returning null.");
                }

                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // 先在场景中找寻
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        if (Debug.isDebugBuild)
                        {
                            Debug.LogWarning("[Singleton] " + typeof(T).Name + " should never be more than 1 in scene!");
                        }

                        return _instance;
                    }

                    // 场景中找不到就创建新物体挂载
                    if (_instance == null)
                    {
                       
                        GameObject singletonObj = new GameObject();
                        _instance = singletonObj.AddComponent<T>();
                        singletonObj.name = "(singleton) " + typeof(T);
                        
                        if (_instance.IsGolbal() && Application.isPlaying)
                        {
                            DontDestroyOnLoad(singletonObj);
                        }

                        return _instance;
                    }
                }

                return _instance;
            }
        }
    }

    
    /// <summary>
    /// 当工程运行结束，在退出时，不允许访问单例
    /// </summary>
    public void OnApplicationQuit()
    {
        ApplicationIsQuitting = true;
    }
}