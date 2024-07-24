using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs
{
    [CreateAssetMenu(fileName = "PrefabsResConfig", menuName = "HMFW/Ecs/PrefabsResConfig")]
    public class PrefabsResConfigSO : UnityEngine.ScriptableObject
    {
        public List<PrefabResInfo> prefabResInfos;
    }

    [Serializable]
    public struct PrefabResInfo
    {
        public uint prefabResId;
        public GameObject prefab;
    }
}