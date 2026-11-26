using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

[assembly: InternalsVisibleTo("HM.HMFW.Editor")]

namespace HMFW.Tools
{
    internal static class FrameworkClassData
    {
        internal const string Menu = "HMFW/收集框架子类";
        internal const string UI = "HMFWUIClassData";
        internal const string GameStates = "HMFWGameStateClassData";
        internal const string FsmStates = "HMFWFsmStateClassData";

        private static readonly Dictionary<string, JToken> Files = new Dictionary<string, JToken>();
        private static readonly Dictionary<Type, Dictionary<string, Type>> Maps = new Dictionary<Type, Dictionary<string, Type>>();
        internal static int Version { get; private set; }

        // 重新收集或关闭 Domain Reload 后再次进入 Play 时，重新读取 Resources。
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Reset()
        {
            Files.Clear();
            Maps.Clear();
            Version++;
        }

        internal static string TypeName(Type type) => $"{type.FullName}, {type.Assembly.GetName().Name}";

        internal static Dictionary<string, Type> GetTypes(string file, Type baseType)
        {
            if (Maps.TryGetValue(baseType, out var map)) return map;

            var root = LoadFile(file);
            if (root != null)
            {
                try
                {
                    map = ResolveTypes(file, baseType, root);
                }
                catch (Exception e) when (e is IOException || e is ArgumentException || e is TypeLoadException ||
                                          e is BadImageFormatException || e is InvalidDataException)
                {
                    Debug.LogError($"{file}.json 数据无效：{e.Message} 请运行「{Menu}」。");
                    map = null;
                }
            }

            Maps.Add(baseType, map);
            return map;
        }

        private static Dictionary<string, Type> ResolveTypes(string file, Type baseType, JToken entries)
        {
            if (file == FsmStates)
            {
                if (!(entries is JObject hosts)) throw new InvalidDataException("缺少 FSM 宿主分组。");
                entries = hosts[TypeName(baseType.GetGenericArguments()[0])] ?? new JArray();
            }
            if (!(entries is JArray names)) throw new InvalidDataException("类型列表必须是 JSON 数组。");

            var map = new Dictionary<string, Type>();
            foreach (var entry in names)
            {
                var name = entry.Type == JTokenType.String ? (string)entry : null;
                var type = string.IsNullOrWhiteSpace(name) ? null : Type.GetType(name, false);
                if (type == null || type.IsAbstract || type.ContainsGenericParameters || !type.IsSubclassOf(baseType))
                    throw new InvalidDataException($"无效的 {baseType} 子类“{entry}”。");
                map.Add(file == UI ? type.FullName : type.Name, type);
            }
            return map;
        }

        internal static JToken LoadFile(string file)
        {
            if (Files.TryGetValue(file, out var data)) return data;
            var asset = Resources.Load<TextAsset>(file);
            if (asset == null)
            {
                Debug.LogError($"Resources 中缺少 {file}.json，请运行「{Menu}」生成。");
            }
            else
            {
                try
                {
                    data = JToken.Parse(asset.text);
                }
                catch (JsonException e)
                {
                    Debug.LogError($"{file}.json 解析失败：{e.Message} 请运行「{Menu}」。");
                }
            }

            Files.Add(file, data);
            return data;
        }
    }
}
