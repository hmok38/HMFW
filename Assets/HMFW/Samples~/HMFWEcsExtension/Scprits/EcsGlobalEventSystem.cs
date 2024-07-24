using Unity.Entities;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct EcsGlobalEventSystem : Unity.Entities.ISystem
    {
        private bool _init;

        public void OnCreate(ref SystemState state)
        {
            if (!_init)
            {
                _init = true;
                //创建全局GlobalEvent
                state.EntityManager.CreateSingletonBuffer<EcsGlobalEventComponent>();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!_init) return;
            var buf = SystemAPI.GetSingletonBuffer<EcsGlobalEventComponent>();
            if (buf.Length >= 1)
            {
                for (int i = 0; i < buf.Length; i++)
                {
                    FW.GEventMgr.Trigger(buf[i].EventName.ToString(), buf[i].Entity);
                }

                buf.Clear();
            }
        }
    }
}