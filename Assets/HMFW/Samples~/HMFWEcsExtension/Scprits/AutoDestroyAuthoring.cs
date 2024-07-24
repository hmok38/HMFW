using Unity.Entities;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs
{
    public class AutoDestroyAuthoring : MonoBehaviour
    {
        public float autoDestroyTime = 1;

        private class AutoDestroyBaker : Baker<AutoDestroyAuthoring>
        {
            public override void Bake(AutoDestroyAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new AutoDestroyComponent() { DestroyTime = authoring.autoDestroyTime });
            }
        }
    }

    public struct AutoDestroyComponent : Unity.Entities.IComponentData
    {
        public float DestroyTime;
    }
}