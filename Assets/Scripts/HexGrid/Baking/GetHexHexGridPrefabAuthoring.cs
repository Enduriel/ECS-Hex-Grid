using Unity.Entities;
using UnityEngine;

namespace Trideria.HexGrid
{
	public struct HexHexGridPrefabComponent : IComponentData
	{
		public Entity Value;
	}

	public class GetHexHexGridPrefabAuthoring : MonoBehaviour
	{
		public GameObject prefab;
	}

	public class GetPrefabBaker : Baker<GetHexHexGridPrefabAuthoring>
	{
		public override void Bake(GetHexHexGridPrefabAuthoring authoring)
		{
			var entityPrefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new HexHexGridPrefabComponent() { Value = entityPrefab });
		}
	}
}