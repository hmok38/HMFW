using Unity.Entities;
using Unity.Transforms;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs
{
    public partial struct PrefabInstantiateSystem : Unity.Entities.ISystem
    {
        void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PrefabInstantiateComponent>();
            state.RequireForUpdate<PrefabResSingleton>();
        }

        void OnUpdate(ref SystemState state)
        {
            var resSingleton = SystemAPI.GetSingleton<PrefabResSingleton>();

            var ecbSys = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSys.CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (component, entity) in SystemAPI.Query<PrefabInstantiateComponent>().WithEntityAccess())
            {
                if (resSingleton.PrefabMap.TryGetValue(component.ResId, out var prefab))
                {
                    var entityInstance = ecb.Instantiate(prefab);
                    ecb.SetComponent(entityInstance, new LocalTransform()
                    {
                        Position = component.Position,
                        Rotation = component.Rotation,
                        Scale = component.Scale
                    });
                    if (component.AutoDestroyTime > 0)
                    {
                        ecb.AddComponent<AutoDestroyComponent>(entityInstance, new AutoDestroyComponent()
                        {
                            DestroyTime = component.AutoDestroyTime
                        });
                    }

                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}