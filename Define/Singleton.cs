using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> where T : class, new()
{
    protected static T _instance = null;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }
            return _instance;
        }
    }
    protected Singleton()
    {
        if (_instance != null)
        {
            throw new SingletonException("This " + typeof(T).ToString() + " Singleton Instance is not null !!!");
        }
        _Init();
    }
    protected virtual void _Init()
    { }
}
public class SingletonException : Exception
{
    public SingletonException(string msg) : base(msg)
    { }
}