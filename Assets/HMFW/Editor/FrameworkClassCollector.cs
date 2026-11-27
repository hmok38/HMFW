using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using HMFW.Core;
using HMFW.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;
using UnityEngine;

namespace HMFW.Editor
{
    internal sealed class FrameworkClassCollector : BuildPlayerProcessor
    {
        private const string Output = "Assets/HMFWGenerated";
        private const string ResourcesFolder = Output + "/Resources";

        [MenuItem(FrameworkClassData.Menu)]
        private static void Collect()
        {
            try
            {
                var preserved = new HashSet<Type>();
                var lists = CollectClassLists(preserved);
                var files = lists.ToDictionary(p => $"{ResourcesFolder}/{p.Key}.json", p => p.Value.ToString(Formatting.Indented));
                files.Add($"{Output}/link.xml", LinkXml(preserved));
                WriteFiles(files);
                AssetDatabase.Refresh();
                FrameworkClassData.Reset();
                var fsmCount = lists[FrameworkClassData.FsmStates].Children<JProperty>().Sum(host => host.Value.Count());
                Debug.Log($"已收集 {lists[FrameworkClassData.UI].Count()} 个 UI、{lists[FrameworkClassData.GameStates].Count()} 个游戏状态和 {fsmCount} 个 FSM 状态，保存到 {ResourcesFolder}。");
            }
            catch (Exception e)
            {
                Debug.LogError($"收集框架子类失败：{e.Message}");
            }
        }

        [InitializeOnLoadMethod]
        private static void RegisterPlayModeCheck()
        {
            EditorApplication.playModeStateChanged += CheckOnPlay;
        }

        private static void CheckOnPlay(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode) return;

            var error = GetValidationError(FrameworkClassData.LoadFile);
            if (error != null) Debug.LogError(error);
        }

        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            var error = GetValidationError(ReadGeneratedFile);
            if (error != null) throw new BuildFailedException(error);
        }

        private static JToken ReadGeneratedFile(string name)
        {
            var path = $"{ResourcesFolder}/{name}.json";
            if (!File.Exists(path)) throw new FileNotFoundException($"缺少清单文件 {path}。", path);

            return JToken.Parse(File.ReadAllText(path), new JsonLoadSettings
            {
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error
            });
        }

        private static string GetValidationError(Func<string, JToken> loadFile)
        {
            var errors = new List<string>();
            try
            {
                foreach (var list in CollectClassLists())
                {
                    var differences = new List<string>();
                    try
                    {
                        var saved = loadFile(list.Key);
                        if (saved == null) continue;
                        CompareList(list.Value, saved, string.Empty, differences);
                    }
                    catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is JsonException)
                    {
                        differences.Add(e.Message);
                    }
                    if (differences.Count > 0)
                        errors.Add($"{list.Key}.json\n{string.Join("\n", differences)}");
                }
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
            }

            return errors.Count == 0 ? null
                : $"框架子类清单校对失败，请重新运行「{FrameworkClassData.Menu}」。\n{string.Join("\n", errors)}";
        }

        private static Dictionary<string, JToken> CollectClassLists(HashSet<Type> preserved = null)
        {
            var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);
            var playerNames = new HashSet<string>(assemblies.Select(a => a.name));
            playerNames.UnionWith(assemblies.SelectMany(a => a.compiledAssemblyReferences)
                .Select(path => Path.GetFileNameWithoutExtension(path)));

            var ui = FindSubclasses(typeof(UIBase), playerNames);
            var gameStates = FindSubclasses(typeof(GameStateBase), playerNames);
            var fsmStates = FindSubclasses(typeof(FsmState<>), playerNames);

            CheckNames(ui, t => t.FullName);
            CheckNames(ui, t =>
            {
                var attribute = t.GetCustomAttribute<UIAttribute>();
                if (attribute?.UIAlias == null) throw new InvalidDataException($"{t.FullName} 缺少 UIAttribute 或 UIAlias。");
                preserved?.Add(attribute.GetType());
                return attribute.UIAlias;
            });
            CheckNames(gameStates, t => t.Name);
            var hosts = new JObject();
            foreach (var group in fsmStates.GroupBy(GetFsmHost).OrderBy(g => FrameworkClassData.TypeName(g.Key), StringComparer.Ordinal))
            {
                CheckNames(group, t => t.Name);
                hosts.Add(FrameworkClassData.TypeName(group.Key), Names(group));
            }

            preserved?.UnionWith(ui);
            preserved?.UnionWith(gameStates);
            preserved?.UnionWith(fsmStates);
            return new Dictionary<string, JToken>
            {
                { FrameworkClassData.UI, Names(ui) },
                { FrameworkClassData.GameStates, Names(gameStates) },
                { FrameworkClassData.FsmStates, hosts }
            };
        }

        private static void CompareList(JToken expected, JToken saved, string context, List<string> differences)
        {
            if (expected is JObject hosts && saved is JObject savedHosts)
            {
                foreach (var host in hosts.Properties())
                {
                    var states = savedHosts[host.Name];
                    if (states == null) differences.Add($"缺少宿主：{host.Name}");
                    CompareList(host.Value, states ?? new JArray(), $"宿主 {host.Name}：", differences);
                }
                foreach (var host in savedHosts.Properties())
                    if (hosts[host.Name] == null) differences.Add($"多余宿主：{host.Name}");
                return;
            }

            if (!(expected is JArray names) || !(saved is JArray savedNames) || savedNames.Any(t => t.Type != JTokenType.String))
            {
                differences.Add($"{context}清单格式错误。");
                return;
            }

            var missing = new HashSet<string>(names.Values<string>(), StringComparer.Ordinal);
            foreach (var name in savedNames.Values<string>())
                if (!missing.Remove(name)) differences.Add($"{context}多余或重复的类：{name}");
            foreach (var name in missing.OrderBy(n => n, StringComparer.Ordinal))
                differences.Add($"{context}缺少类：{name}");
        }

        private static List<Type> FindSubclasses(Type baseType, HashSet<string> playerAssemblies)
        {
            return TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract && !t.ContainsGenericParameters && playerAssemblies.Contains(t.Assembly.GetName().Name))
                .ToList();
        }

        private static Type GetFsmHost(Type state)
        {
            for (var parent = state.BaseType; parent != null; parent = parent.BaseType)
            {
                if (parent.IsGenericType && parent.GetGenericTypeDefinition() == typeof(FsmState<>))
                    return parent.GetGenericArguments()[0];
            }
            throw new InvalidDataException($"无法确定状态 {state.FullName} 的宿主。");
        }

        private static void CheckNames(IEnumerable<Type> types, Func<Type, string> key)
        {
            var names = new Dictionary<string, Type>();
            foreach (var type in types)
            {
                var name = key(type);
                if (names.TryGetValue(name, out var other))
                    throw new InvalidDataException($"名称“{name}”重复：{FrameworkClassData.TypeName(other)} 和 {FrameworkClassData.TypeName(type)}。");
                names.Add(name, type);
            }
        }

        private static JArray Names(IEnumerable<Type> types) =>
            new JArray(types.Select(FrameworkClassData.TypeName).OrderBy(name => name, StringComparer.Ordinal));

        private static string LinkXml(IEnumerable<Type> types)
        {
            var root = new XElement("linker");
            foreach (var group in types.GroupBy(t => t.Assembly.GetName().Name).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                var assembly = new XElement("assembly", new XAttribute("fullname", group.Key));
                foreach (var type in group.OrderBy(t => t.FullName, StringComparer.Ordinal))
                    assembly.Add(new XElement("type", new XAttribute("fullname", type.FullName.Replace('+', '/')),
                        new XAttribute("preserve", "all")));
                root.Add(assembly);
            }
            return root.ToString();
        }

        private static void WriteFiles(Dictionary<string, string> files)
        {
            Directory.CreateDirectory(ResourcesFolder);
            var previous = new Dictionary<string, byte[]>();
            try
            {
                foreach (var file in files)
                {
                    if (File.Exists(file.Key) && File.ReadAllText(file.Key) == file.Value) continue;
                    previous.Add(file.Key, File.Exists(file.Key) ? File.ReadAllBytes(file.Key) : null);
                    File.WriteAllText(file.Key, file.Value);
                }
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                foreach (var file in previous)
                {
                    if (file.Value == null) File.Delete(file.Key);
                    else if (!File.Exists(file.Key) || !file.Value.SequenceEqual(File.ReadAllBytes(file.Key)))
                        File.WriteAllBytes(file.Key, file.Value);
                }
                throw;
            }
        }
    }
}
