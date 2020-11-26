



public abstract class UIBase:UnityEngine.MonoBehaviour
{
    /// <summary>
    /// 仅允许由uimanager调用-
    /// </summary>
    /// <param name="args"></param>
    public void Open(params object[] args)
    {
        gameObject.SetActive(true);
        OnOpen(args);
    }

    /// <summary>
    /// 当被打开的时候调用,先于OnEnable
    /// </summary>
    /// <param name="args"></param>
    public abstract void OnOpen(params object[] args);

    /// <summary>
    /// 仅允许由uimanager调用-
    /// </summary>
    /// <param name="args"></param>
    public void Close(params object[] args)
    {
        gameObject.SetActive(false);
        OnClose(args);
    }
    
    /// <summary>
    /// 当被打开的时候调用,先于OnDisable
    /// </summary>
    /// <param name="args"></param>
    public abstract void OnClose(params object[] args);

}
