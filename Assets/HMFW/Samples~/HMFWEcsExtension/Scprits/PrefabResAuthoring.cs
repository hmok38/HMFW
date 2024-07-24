using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace HMFW.Ecs
{
    /// <summary>
    /// 预制体资源组件,可在ECS中通过查询prefabResId或者相应的预制体的ENTITY
    /// </summary>
    public class PrefabResAuthoring : MonoBehaviour
    {
        public List<PrefabsResConfigSO> prefabsResConfigs;


        class Baker : Unity.Entities.Baker<PrefabResAuthoring>
        {
            public override void Bake(PrefabResAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                var buf = AddBuffer<PrefabsResBufferElement>(entity);

                for (var i = 0; i < authoring.prefabsResConfigs.Count; i++)
                {
                    var config = authoring.prefabsResConfigs[i];
                    for (int j = 0; j < config.prefabResInfos.Count; j++)
                    {
                        var info = config.prefabResInfos[j];
                        var bufEle = new PrefabsResBufferElement()
                        {
                            PrefabResId = info.prefabResId,
                            PrefabEntity = GetEntity(info.prefab, TransformUsageFlags.None)
                        };
                        buf.Add(bufEle);
                    }
                }
            }
        }
    }

    public struct PrefabsResBufferElement : Unity.Entities.IBufferElementData
    {
        public uint PrefabResId;
        public Entity PrefabEntity;
    }
}