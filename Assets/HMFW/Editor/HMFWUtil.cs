using System.IO;
using UnityEditor;
using UnityEngine;

public class HMFWUtil
{
    [MenuItem("HMFW/打开特殊目录/open streamingAssetsPath")]
    public static void OpenStreamingAssetsFolder()
    {
        if (Directory.Exists(Application.streamingAssetsPath))
        {
            OpenInFileBrowser(Application.streamingAssetsPath);
        }
        else
        {
            Debug.Log($"{Application.streamingAssetsPath}  不存在");
        }
    }

    [MenuItem("HMFW/打开特殊目录/open dataPath")]
    public static void OpenDataPathFolder()
    {
        if (Directory.Exists(Application.dataPath))
        {
            OpenInFileBrowser(Application.dataPath);
        }
        else
        {
            Debug.Log($"{Application.dataPath}  不存在");
        }
    }

    [MenuItem("HMFW/打开特殊目录/open persistentData")]
    public static void OpenPersistentDataFolder()
    {
        if (Directory.Exists(Application.persistentDataPath))
        {
            OpenInFileBrowser(Application.persistentDataPath);
        }
        else
        {
            Debug.Log($"{Application.persistentDataPath}  不存在");
        }
    }

    [MenuItem("HMFW/打开特殊目录/open temporaryCachePath")]
    public static void OpenTemporaryCachePathFolder()
    {
        if (Directory.Exists(Application.temporaryCachePath))
        {
            OpenInFileBrowser(Application.temporaryCachePath);
        }
        else
        {
            Debug.Log($"{Application.temporaryCachePath}  不存在");
        }
    }

    [MenuItem("HMFW/打开特殊目录/open consoleLogPath")]
    public static void OpenConsoleLogPathFolder()
    {
        if (File.Exists(Application.consoleLogPath))
        {
            FileInfo fileInfo = new FileInfo(Application.consoleLogPath);
            OpenInFileBrowser(fileInfo.Directory.FullName);
        }
        else
        {
            Debug.Log($"{Application.consoleLogPath}  不存在");
        }
    }


    public static void OpenInFileBrowser(string path)
    {
#if UNITY_EDITOR_WIN
        System.Diagnostics.Process.Start("explorer.exe", path.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX
            System.Diagnostics.Process.Start("open", path);
#elif UNITY_EDITOR_LINUX
            System.Diagnostics.Process.Start("xdg-open", path);
#endif
    }
}