using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs
{
    [BurstCompile]
    public partial struct AutoDestroySystem : Unity.Entities.ISystem
    {
        [BurstCompile]
        void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AutoDestroyComponent>();
        }

        [BurstCompile]
        void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var deltaTime = Time.deltaTime;
            foreach (var (componentRW, entity) in SystemAPI.Query<RefRW<AutoDestroyComponent>>().WithEntityAccess())
            {
                componentRW.ValueRW.DestroyTime -= deltaTime;
                if (componentRW.ValueRW.DestroyTime <= 0)
                {
                    if (state.EntityManager.HasBuffer<Child>(entity))
                    {
                        var children = state.EntityManager.GetBuffer<Child>(entity);
                        foreach (var child in children)
                        {
                            ecb.DestroyEntity(child.Value);
                        }
                    }

                    ecb.DestroyEntity(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}