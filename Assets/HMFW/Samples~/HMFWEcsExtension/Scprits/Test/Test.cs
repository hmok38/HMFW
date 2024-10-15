using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs.Test
{
    public class Test : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var entity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity(
                typeof(HMFW.Ecs.PrefabInstantiateComponent));
            var prefabInstantiate = new PrefabInstantiateComponent()
            {
                ResId = 0,
                AutoDestroyTime = 3,
                Rotation = quaternion.identity,
                Scale = 1
            };
            World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(entity, prefabInstantiate);

            var entity2 = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity(
                typeof(HMFW.Ecs.PrefabInstantiateComponent));
            var prefabInstantiate2 = new PrefabInstantiateComponent()
            {
                Position = new float3(1.5f, 0, 0),
                ResId = 0,
                AutoDestroyTime = 0,
                Rotation = quaternion.identity,
                Scale = 1
            };
            World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(entity2, prefabInstantiate2);
        }
    }
}