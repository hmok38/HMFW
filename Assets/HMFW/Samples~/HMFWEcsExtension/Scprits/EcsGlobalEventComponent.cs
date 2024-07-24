using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs
{
    /// <summary>
    /// ECS系统内的全局事件,触发方法请见下面的扩展方法
    /// 会在统一的时候调用系统的 FW.GEventMgr.Trigger,参数为entity
    /// </summary>
    [InternalBufferCapacity(0)]
    public struct EcsGlobalEventComponent : Unity.Entities.IBufferElementData
    {
        public FixedString32Bytes EventName;
        public Unity.Entities.Entity Entity;
    }

    [BurstCompile]
    public static class EcsGlobalEventExtension
    {
        /// <summary>
        /// 主线程发送全局事件的扩展方法
        /// </summary>
        /// <param name="globalEvent">事件组件</param>
        /// <param name="ecb">发送事件的EntityCommandBuffer</param>
        /// <param name="ecsGlobalEventComponentSingletonEntity">获取管理Entity 通过 SystemAPI.GetSingletonEntity&lt;EcsGlobalEventComponent&gt;()</param>
        [BurstCompile]
        public static void SendEvent(this in EcsGlobalEventComponent globalEvent, ref EntityCommandBuffer ecb,
            in Unity.Entities.Entity ecsGlobalEventComponentSingletonEntity)
        {
            ecb.AppendToBuffer(ecsGlobalEventComponentSingletonEntity, globalEvent);
        }

        /// <summary>
        /// Job内发送事件的扩展方法
        /// </summary>
        /// <param name="globalEvent">事件组件</param>
        /// <param name="entityIndexInQuery">IJobEntity函数的字段,如:[EntityIndexInQuery] int entityIndexInQuery</param>
        /// <param name="ecb">EndSimulationEntityCommandBufferSystem.Singleton 创建的EntityCommandBuffer.ParallelWriter</param>
        /// <param name="ecsGlobalEventComponentSingletonEntity">获取管理Entity 通过 SystemAPI.GetSingletonEntity&amp;lt;EcsGlobalEventComponent&amp;gt;()</param>
        [BurstCompile]
        public static void SentEventInJob(this in EcsGlobalEventComponent globalEvent, int entityIndexInQuery,
            ref EntityCommandBuffer.ParallelWriter ecb, in Unity.Entities.Entity ecsGlobalEventComponentSingletonEntity)
        {
            ecb.AppendToBuffer(entityIndexInQuery, ecsGlobalEventComponentSingletonEntity, globalEvent);
        }
    }
}