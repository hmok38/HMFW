using System;
using System.Collections.Generic;
using System.Linq;
using ETEditor;
using UnityEditor;
using UnityEngine;

public class PlayerPrefsEditor : EditorWindow
{
    private static PlayerPrefsEditor prefsEditor;
    private readonly string[] options = { "Add", "Add Int", "Add Float", "Add String" };

    private readonly Dictionary<int, string> prefTypeDic = new Dictionary<int, string>
        { { 0, "Int" }, { 1, "Float" }, { 2, "String" } };

    private List<PlayerPrefPair> prefs;
    private Vector3 scrollPos = Vector2.zero;
    private readonly Dictionary<string, PlayerPrefPair> playerPrefsDict = new Dictionary<string, PlayerPrefPair>();
    private string searchKey = String.Empty;

    [MenuItem("HMFW/PlayerPrefs Editor", false, 10)]
    public static void ShowWindow()
    {
        if (prefsEditor == null)
        {
            prefsEditor = GetWindow<PlayerPrefsEditor>();
            prefsEditor.minSize = new Vector2(960, 600);
        }
        else
        {
            prefsEditor.Close();
            prefsEditor = null;
        }
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDisable()
    {
        prefsEditor = null;
    }

    private void AddPlayerPrefs(int index)
    {
        if (index != 0)
        {
            PlayerPrefPair playerPrefPair = new PlayerPrefPair();
            playerPrefPair.Key = options[index];
            playerPrefPair.Value = "0";
            playerPrefPair.type = index - 1;
            prefs.Insert(0, playerPrefPair);
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save(Ctrl+S)", EditorStyles.toolbarButton))
        {
            SaveUpdatePrefs();
        }

        if (GUILayout.Button("FormatAll", EditorStyles.toolbarButton))
        {
            for (int i = 0; i < prefs.Count; i++)
            {
                prefs[i].Value = JsonFormatter.FromatOrCompress(prefs[i].Value);
            }
        }

        if (GUILayout.Button("Clear All", EditorStyles.toolbarButton))
        {
            if (EditorUtility.DisplayDialog("Clear", "Are you clear all PlayerPrefs info?", "confirm", "cancel"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Refresh();
            }
        }

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
        {
            Refresh();
        }

        int idx = EditorGUILayout.Popup(0, options, EditorStyles.toolbarDropDown, GUILayout.Width(50));
        AddPlayerPrefs(idx);

        GUILayout.EndHorizontal();

        //绘制label:
        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
        labelStyle.normal.textColor = Color.cyan;
        EditorGUILayout.LabelField("输入要搜索的key:", labelStyle, GUILayout.Width(120));
        //绘制输入框:
        var style = new GUIStyle(EditorStyles.textField);
        style.fontStyle = FontStyle.Bold;
        searchKey = GUILayout.TextField(searchKey, style, GUILayout.ExpandWidth(true));
        GUILayout.Space(15);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginVertical();
        for (int i = 0; i < prefs.Count; i++)
        {
            if (string.IsNullOrEmpty(searchKey) || prefs[i].Key.Contains(searchKey))
            {
                DrawItem(prefs[i]);
            }
        }

        GUILayout.EndVertical();
        GUILayout.Space(10);
        EditorGUILayout.EndScrollView();
        if (Event.current.modifiers.Equals(Event.KeyboardEvent("^S").modifiers) &&
            Event.current.keyCode == Event.KeyboardEvent("^S").keyCode)
        {
            SaveUpdatePrefs();
        }
    }

    private void Refresh()
    {
        prefs = PlayerPrefsExtension.GetAll().ToList();
        for (int i = 0; i < prefs.Count; i++)
        {
            //过滤unity默认保存的信息
            if (prefs[i].Key.ToLower().StartsWith("unity"))
            {
                prefs.RemoveAt(i);
                i--;
            }
        }

        prefs.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.Ordinal));
    }

    private void SaveUpdatePrefs()
    {
        try
        {
            foreach (var playerPrefPair in playerPrefsDict)
            {
                Debug.Log("Save:" + playerPrefPair.Key);
                SetPlayerPrefs(playerPrefPair.Value);
            }

            playerPrefsDict.Clear();
            Refresh();
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", "Type error:" + e, "confirm");
        }
    }

    private void AddUpdatePrefs(string key, PlayerPrefPair playerPrefPair)
    {
        if (playerPrefsDict.ContainsKey(key))
        {
            playerPrefsDict[key] = playerPrefPair;
        }
        else
        {
            playerPrefsDict.Add(key, playerPrefPair);
        }
    }

    private void DrawItem(PlayerPrefPair pref)
    {
        var colorTemp = GUI.color;
        if (pref.focus)
        {
            GUI.color = Color.magenta;
        }

        GUILayout.BeginHorizontal();
        string keyTemp = GUILayout.TextArea(pref.Key, GUILayout.Width(200));
        if (!string.Equals(keyTemp, pref.Key))
        {
            pref.Key = keyTemp;
            AddUpdatePrefs(keyTemp, pref);
        }

        GUILayout.Space(10);
        string valTemp = GUILayout.TextArea(pref.Value);
        if (!string.Equals(valTemp, pref.Value))
        {
            pref.Value = valTemp;
            AddUpdatePrefs(keyTemp, pref);
        }

        GUILayout.Space(10);
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        style.fixedWidth = 40;
        GUILayout.Label(prefTypeDic[pref.type], style);
        if (GUILayout.Button("Delete", GUILayout.Width(50)))
        {
            if (EditorUtility.DisplayDialog("Delete", $"Do you want to delete【{pref.Key}】 ?", "confirm", "cancel"))
            {
                prefs.Remove(pref);
                PlayerPrefs.DeleteKey(pref.Key);
                PlayerPrefs.Save();
            }
        }

        if (GUILayout.Button("Copy", GUILayout.Width(50)))
        {
            GUIUtility.systemCopyBuffer = pref.Value;
        }

        if (GUILayout.Button("Format", GUILayout.Width(50)))
        {
            pref.Value = JsonFormatter.FromatOrCompress(pref.Value);
        }

        var frontVal = pref.focus;
        pref.focus = GUILayout.Toggle(frontVal, "", GUILayout.Width(15));
        if (pref.focus != frontVal)
        {
            EditorPrefs.SetBool(pref.Key, pref.focus);
        }

        GUI.color = colorTemp;
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private void SetPlayerPrefs(PlayerPrefPair playerPrefs)
    {
        if (playerPrefs.type == 0)
        {
            PlayerPrefs.SetInt(playerPrefs.Key, Convert.ToInt32(playerPrefs.Value));
        }
        else if (playerPrefs.type == 1)
        {
            PlayerPrefs.SetFloat(playerPrefs.Key, Convert.ToSingle(playerPrefs.Value));
        }
        else
        {
            PlayerPrefs.SetString(playerPrefs.Key, playerPrefs.Value);
        }

        PlayerPrefs.Save();
    }
}