using Unity.Burst;
using Unity.Entities;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs
{
    [BurstCompile]
    public static class EntityDestroyExtension
    {
        /// <summary>
        /// 连带所有子entity一起删除
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityManager"></param>
        /// <param name="ecb"></param>
        [BurstCompile]
        public static void DestroyEntityWithChildren(this in Entity entity, in EntityManager entityManager,
            ref EntityCommandBuffer ecb)
        {
            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            if (entityManager.HasBuffer<Unity.Transforms.Child>(entity))
            {
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                var children = entityManager.GetBuffer<Unity.Transforms.Child>(entity);
                foreach (var child in children)
                {
                    ecb.DestroyEntity(child.Value);
                }
            }

            ecb.DestroyEntity(entity);
        }

        /// <summary>
        /// 在job里面删除Entity和全部子Entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityIndexInQuery">IJobEntity函数的字段,如:[EntityIndexInQuery] int entityIndexInQuery</param>
        /// <param name="ecb">EndSimulationEntityCommandBufferSystem.Singleton 创建的EntityCommandBuffer.ParallelWriter</param>
        /// <param name="children">IJobEntity函数添加的字段 in DynamicBuffer&lt;Child&gt; children</param>
        [BurstCompile]
        public static void DestroyEntityWithChildrenInJob(this in Entity entity,
            [EntityIndexInQuery] int entityIndexInQuery,
            ref EntityCommandBuffer.ParallelWriter ecb, in DynamicBuffer<Unity.Transforms.Child> children)
        {
            foreach (var c in children)
            {
                ecb.DestroyEntity(entityIndexInQuery, c.Value);
            }

            ecb.DestroyEntity(entityIndexInQuery, entity);
        }
    }
}