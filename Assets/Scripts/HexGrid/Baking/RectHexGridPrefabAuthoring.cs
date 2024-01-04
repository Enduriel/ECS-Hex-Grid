using Unity.Entities;
using UnityEngine;

namespace Trideria.HexGrid
{
	public struct RectHexGridPrefabComponent : IComponentData
	{
		public Entity Value;
	}

	public class GetRectHexGridPrefabAuthoring : MonoBehaviour
	{
		public GameObject prefab;
	}

	public class GetRectHexGridPrefabBaker : Baker<GetRectHexGridPrefabAuthoring>
	{
		public override void Bake(GetRectHexGridPrefabAuthoring authoring)
		{
			var entityPrefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new RectHexGridPrefabComponent { Value = entityPrefab });
		}
	}
}