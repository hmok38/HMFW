using Unity.Collections;
using Unity.Entities;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs
{
    public struct PrefabResSingleton : Unity.Entities.IComponentData
    {
        public NativeHashMap<uint, Entity> PrefabMap;
    }
}