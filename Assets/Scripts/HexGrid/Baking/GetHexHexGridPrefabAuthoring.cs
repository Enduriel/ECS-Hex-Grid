using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Rendering;
using UnityEngine;

namespace MyNamespace
{
    public struct HexHexGridPrefabComponent : IComponentData
    {
        public Entity Value;
    }

    public class GetHexHexGridPrefabAuthoring  : MonoBehaviour
    {
        public GameObject Prefab;
    }
    
    public class GetPrefabBaker : Baker<GetHexHexGridPrefabAuthoring>
    {
        public override void Bake(GetHexHexGridPrefabAuthoring authoring)
        {
            var entityPrefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic);
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HexHexGridPrefabComponent() { Value = entityPrefab });
        }
    }
}