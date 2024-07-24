using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs
{
    [BurstCompile]
    public partial struct PrefabResSystem : Unity.Entities.ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new PrefabResSingleton()
            {
                PrefabMap = new NativeHashMap<uint, Entity>(30, Allocator.Persistent)
            });
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            state.RequireForUpdate<PrefabsResBufferElement>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingletonRW<PrefabResSingleton>(out var prgc))
            {
                var ecb = new EntityCommandBuffer(Allocator.Temp);
                //将所有的预制体资源添加到spwanSystem数据中,并删除他们
                foreach (var (comp, entity) in SystemAPI.Query<DynamicBuffer<PrefabsResBufferElement>>()
                             .WithEntityAccess())
                {
                    var buf = state.EntityManager.GetBuffer<PrefabsResBufferElement>(entity);
                    for (int i = 0; i < buf.Length; i++)
                    {
                        var element = buf[i];
                        if (!prgc.ValueRW.PrefabMap.ContainsKey(element.PrefabResId))
                            prgc.ValueRW.PrefabMap.Add(element.PrefabResId, element.PrefabEntity);
                        else
                        {
                            prgc.ValueRW.PrefabMap[element.PrefabResId] = element.PrefabEntity;
                        }
                    }

                    ecb.DestroyEntity(entity);
                }

                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (SystemAPI.TryGetSingletonRW<PrefabResSingleton>(out var prgc))
            {
                prgc.ValueRW.PrefabMap.Dispose();
            }
        }
    }
}