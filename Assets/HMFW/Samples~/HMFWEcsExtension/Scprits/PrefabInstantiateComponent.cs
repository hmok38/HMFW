using Unity.Mathematics;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs
{
    public struct PrefabInstantiateComponent : Unity.Entities.IComponentData
    {
        public uint ResId;
        public float3 Position;
        public float Scale;
        public quaternion Rotation;

        /// <summary>
        /// 自动销毁时间,<=0不自动销毁,且不添加AutoDestroyComponent组件
        /// </summary>
        public float AutoDestroyTime;
    }
}